using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NuspecDependenciesUpdater.Models;

namespace NuspecDependenciesUpdater.Extensions
{
    public static class PackageFileExtensions
    {
        #region parse packages.config

        public static IDictionary<string, string> GetComponentsFromPackages(this IEnumerable<FileInfo> packageInfoFiles)
        {
            Dictionary<string, string> components = new Dictionary<string, string>();

            foreach (FileInfo packageFile in packageInfoFiles)
            {
                foreach (Component component in packageFile.GetComponentsFromPackage())
                {
                    if (!components.TryGetValue(component.Id, out string version) ||
                        component.Version.IsVersionNewerThan(version))
                    {
                        components[component.Id] = component.Version;
                    }
                }
            }

            return components;
        }

        public static IEnumerable<Component> GetComponentsFromPackage(this FileInfo packageFile)
        {
            if (!packageFile.Exists)
            {
                yield break;
            }

            XmlDocument document = new XmlDocument();
            document.Load(packageFile.FullName);
            XmlNodeList componentList = document.SelectNodes(PackageFileConstants.PackageComponentPath);
            if (componentList != null)
            {
                foreach (XmlElement component in componentList.OfType<XmlElement>())
                {
                    yield return new Component(component.GetAttribute(PackageFileConstants.IdAttribute), component.GetAttribute(PackageFileConstants.VersionAttribute));
                }
            }
        }

        #endregion

        #region parse .csproj
        
        public static IDictionary<string, string> GetComponentsFromProjectFiles(this IEnumerable<FileInfo> projectFiles)
        {
            Dictionary<string, string> components = new Dictionary<string, string>();

            foreach (FileInfo projectFile in projectFiles)
            {
                foreach (Component component in projectFile.GetComponentsFromProjectFile())
                {
                    if (!components.TryGetValue(component.Id, out string version) ||
                        component.Version.IsVersionNewerThan(version))
                    {
                        components[component.Id] = component.Version;
                    }
                }
            }

            return components;
        }

        public static IEnumerable<Component> GetComponentsFromProjectFile(this FileInfo projectFile)
        {
            if (!projectFile.Exists)
            {
                yield break;
            }

            XmlDocument document = new XmlDocument();
            document.Load(projectFile.FullName);
            XmlNodeList componentList = document.SelectNodes(PackageFileConstants.PackageReferencePath);
            if (componentList != null)
            {
                foreach (XmlElement component in componentList.OfType<XmlElement>())
                {
                    string id = component.GetAttribute(PackageFileConstants.IncludeAttribute);
                    string version = component.GetAttribute(PackageFileConstants.CamelCaseVersionAttribute).ExtractVersionFromPackageReference();

                    yield return new Component(id, version);
                }
            }
        }

        public static string ExtractVersionFromPackageReference(this string packageReferenceVersion)
        {
            // look for range constraints
            Match rangedMatch = PackageFileConstants.RangedVersionRegex.Match(packageReferenceVersion);
            if (rangedMatch.Success)
            {
                return rangedMatch.Groups[2].Value;
            }

            // look for single version constraints
            Match singleMatch = PackageFileConstants.SingleVersionRegex.Match(packageReferenceVersion);
            if (singleMatch.Success)
            {
                return singleMatch.Groups[2].Value;
            }

            return packageReferenceVersion;
        }

        #endregion

        #region parse .nuspec

        public static IDictionary<string, string> GetComponentsFromNuspecs(this IEnumerable<FileInfo> nuspecFiles)
        {
            Dictionary<string, string> components = new Dictionary<string, string>();

            foreach (FileInfo nuspecFile in nuspecFiles)
            {
                Component nuspecComponent = nuspecFile.GetComponentFromNuspec();
                if (nuspecComponent != null)
                {
                    if (!components.TryGetValue(nuspecComponent.Id, out string version) ||
                        nuspecComponent.Version.IsVersionNewerThan(version))
                    {
                        components[nuspecComponent.Id] = nuspecComponent.Version;
                    }
                }
            }

            return components;
        }

        public static Component GetComponentFromNuspec(this FileInfo nuspecFile)
        {
            XmlDocument document = new XmlDocument();
            document.Load(nuspecFile.FullName);
            XmlNamespaceManager namespaceManager = CreateNuspecNamespaceManager(document);
            XmlElement idElement = GetIdElement(document, namespaceManager);
            XmlElement versionElement = GetVersionElement(document, namespaceManager);
            if (idElement == null || versionElement == null)
            {
                return null;
            }
            return new Component(idElement.InnerText, versionElement.InnerText);
        }

        #endregion
        
        #region XML handling

        public static XmlNamespaceManager CreateNuspecNamespaceManager(this XmlDocument document)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace(PackageFileConstants.NuspecPefix, PackageFileConstants.Namespace);
            return namespaceManager;
        }

        private static XmlElement GetVersionElement(this XmlDocument document, XmlNamespaceManager namespaceManager)
        {
            return document.SelectSingleNode(PackageFileConstants.VersionPath, namespaceManager) as XmlElement;
        }

        private static XmlElement GetIdElement(this XmlDocument document, XmlNamespaceManager namespaceManager)
        {
            return document.SelectSingleNode(PackageFileConstants.IdPath, namespaceManager) as XmlElement;
        }

        #endregion

        #region version comparison

        public static bool IsVersionNewerThan(this string version, string otherVersion)
        {
            ComponentVersion versionA = new ComponentVersion(version);
            ComponentVersion versionB = new ComponentVersion(otherVersion);

            return versionA.CompareTo(versionB) > 0;
        }

        #endregion
    }
}
