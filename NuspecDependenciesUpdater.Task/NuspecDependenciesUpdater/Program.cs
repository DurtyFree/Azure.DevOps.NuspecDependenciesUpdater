using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NuGet;
using NuspecDependenciesUpdater.Extensions;
using NuspecDependenciesUpdater.Models;

namespace NuspecDependenciesUpdater
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("RootDirectory path must be given.");
            }
            string rootDirectory = args[0];

            if (!Directory.Exists(rootDirectory))
            {
                throw new ArgumentException("Given RootDirectory does not exist.");
            }

            Console.WriteLine($"Collecting nuspec & packages files on Root Directory {rootDirectory}");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Execute(rootDirectory);
            watch.Stop();

            Console.WriteLine($"Done in {watch.Elapsed}.");
        }

        public static bool Execute(string rootDirectoryPath)
        {
            DirectoryInfo root = new DirectoryInfo(rootDirectoryPath);
            List<FileInfo> nuspecFiles = root.GetFiles("*.nuspec", SearchOption.AllDirectories).ToList();
            if (!nuspecFiles.Any())
            {
                Console.WriteLine("Could not find any nuspec files!");
                return false;
            }
            Console.WriteLine($"Found {nuspecFiles.Count} nuspec files.");

            Dictionary<string, string> components = new Dictionary<string, string>();

            #region Collect versions from **\packages.config

            List<FileInfo> packageInfoFiles = root.GetFiles("packages.config", SearchOption.AllDirectories).ToList();

            IDictionary<string, string> componentsFromPackages = packageInfoFiles.GetComponentsFromPackages();
            Console.WriteLine($"Found {componentsFromPackages.Count} components from packages.");
            foreach (var componentFromPackages in componentsFromPackages)
            {
                string existingVersion;
                if (!components.TryGetValue(componentFromPackages.Key, out existingVersion))
                {
                    components.Add(
                        componentFromPackages.Key,
                        componentFromPackages.Value);
                }
                else
                {
                    string currentVersion = componentFromPackages.Value;
                    if (currentVersion.IsVersionNewerThan(existingVersion))
                    {
                        components[componentFromPackages.Key] = currentVersion;
                    }
                }
            }

            #endregion

            #region Collect versions from **\*.csproj (PackageReference attributes)

            List<FileInfo> projectFiles = root.GetFiles("*.csproj", SearchOption.AllDirectories).ToList();

            IDictionary<string, string> componentsFromProjects = projectFiles.GetComponentsFromProjectFiles();
            Console.WriteLine($"Found {componentsFromProjects.Count} components from project files.");
            foreach (var componentsFromProject in componentsFromProjects)
            {
                string existingVersion;
                if (!components.TryGetValue(componentsFromProject.Key, out existingVersion))
                {
                    components.Add(
                        componentsFromProject.Key,
                        componentsFromProject.Value);
                }
                else
                {
                    string currentVersion = componentsFromProject.Value;
                    if (currentVersion.IsVersionNewerThan(existingVersion))
                    {
                        components[componentsFromProject.Key] = currentVersion;
                    }
                }
            }

            #endregion

            #region Collect versions from **\*.nuspec

            IDictionary<string, string> componentsFromNuspecs = nuspecFiles.GetComponentsFromNuspecs();
            Console.WriteLine($"Found {componentsFromNuspecs.Count} components from nuspecs.");
            foreach (var componentFromNuspecs in componentsFromNuspecs)
            {
                string existingVersion;
                if (!components.TryGetValue(componentFromNuspecs.Key, out existingVersion))
                {
                    components.Add(
                        componentFromNuspecs.Key,
                        componentFromNuspecs.Value);
                }
                else
                {
                    string currentVersion = componentFromNuspecs.Value;
                    if (currentVersion.IsVersionNewerThan(existingVersion))
                    {
                        components[componentFromNuspecs.Key] = currentVersion;
                    }
                }
            }

            #endregion

            #region Log all found packages and versions

            foreach (KeyValuePair<string, string> component in components)
            {
                Console.WriteLine($"Component {component.Key} = {component.Value}");
            }

            #endregion

            #region Synchronize dependencies verions for nuspec file

            foreach (FileInfo nuspecFile in nuspecFiles)
            {
                Console.WriteLine($"Synchronizing dependencies for {nuspecFile.FullName}");

                FileAttributes attributes = nuspecFile.Attributes;
                nuspecFile.Attributes = FileAttributes.Normal;
                SetComponentVersions(nuspecFile, components);
                nuspecFile.Attributes = attributes;
            }

            #endregion

            return true;
        }

        #region Ajdust nuspec with known packages/versions

        private static void SetComponentVersions(FileInfo nuspecFile, IDictionary<string, string> components)
        {
            XmlDocument document = new XmlDocument();
            document.Load(nuspecFile.FullName);
            XmlNamespaceManager namespaceManager = document.CreateNuspecNamespaceManager();

            XmlNode dependencyRoot = document.SelectSingleNode(PackageFileConstants.DependencyRootPath, namespaceManager);
            if (dependencyRoot != null)
            {
                List<XmlElement> dependencyNodes = dependencyRoot
                    .ChildNodes.OfType<XmlElement>()
                    .Where(e => e.Name == PackageFileConstants.DependencyName)
                    .ToList();

                List<XmlElement> groupedDependencyNodes = dependencyRoot
                    .ChildNodes.OfType<XmlElement>()
                    .Where(e => e.Name == PackageFileConstants.DependencyGroupName)
                    .SelectMany(e => e.ChildNodes.OfType<XmlElement>())
                    .Where(e => e.Name == PackageFileConstants.DependencyName)
                    .ToList();

                foreach (XmlElement dependency in dependencyNodes.Concat(groupedDependencyNodes))
                {
                    string componentId = dependency.GetAttribute(PackageFileConstants.IdAttribute);
                    string existingVersionAttribute = dependency.GetAttribute(PackageFileConstants.VersionAttribute);
                    if (components.TryGetValue(componentId, out string version))
                    {
                        if (!string.IsNullOrEmpty(existingVersionAttribute))
                        {
                            version = GetAdjustedVersionAttribute(existingVersionAttribute, version);
                            dependency.SetAttribute(PackageFileConstants.VersionAttribute, version);
                        }
                        else
                        {
                            dependency.SetAttribute(PackageFileConstants.VersionAttribute, version);
                        }

                        Console.WriteLine($"Adjusted component {componentId} from {existingVersionAttribute} to version {version}.");
                    }
                }
            }
            document.Save(nuspecFile.FullName);
        }

        private static string GetAdjustedVersionAttribute(string existingVersionAttribute, string versionNumber)
        {
            // look for range constraints
            Match rangedMatch = PackageFileConstants.RangedVersionRegex.Match(existingVersionAttribute);
            if (rangedMatch.Success)
            {
                return $"{rangedMatch.Groups[1]}{versionNumber}{rangedMatch.Groups[3]}{rangedMatch.Groups[4]}{rangedMatch.Groups[5]}"; // ) / ]
            }

            // look for single version constraints
            Match singleMatch = PackageFileConstants.SingleVersionRegex.Match(existingVersionAttribute);
            if (singleMatch.Success)
            {
                return $"{singleMatch.Groups[1]}{versionNumber}{singleMatch.Groups[3]}"; // ) / ]
            }

            // overwrite as usual
            return versionNumber;
        }

        #endregion
    }
}
