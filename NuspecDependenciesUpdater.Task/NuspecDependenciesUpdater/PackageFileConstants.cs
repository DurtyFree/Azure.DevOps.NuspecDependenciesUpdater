using System.Text.RegularExpressions;

namespace NuspecDependenciesUpdater
{
    public static class PackageFileConstants
    {
        public static readonly Regex RangedVersionRegex = new Regex("([\\[|\\(])(.+)(,)(.+)([]|)])", RegexOptions.Compiled);
        public static readonly Regex SingleVersionRegex = new Regex("([\\[|\\(])(.+)([]|)])", RegexOptions.Compiled);

        public const string Namespace = "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd";
        public const string NuspecPefix = "nuspec";
        public const string MetaDataPath = "/nuspec:package/nuspec:metadata";
        public const string VersionPath = MetaDataPath + "/nuspec:version";
        public const string IdPath = MetaDataPath + "/nuspec:id";
        public const string DependencyRootPath = MetaDataPath + "/" + DependencyRootName;
        public const string DependencyRootName = "nuspec:dependencies";
        public const string DependencyName = "dependency";
        public const string DependencyGroupName = "group";
        public const string VersionAttribute = "version";
        public const string IdAttribute = "id";
        
        public const string DependencyNodeName = "nuspec:dependency";
        public const string DependencyIdQuery = DependencyRootPath + "/" + DependencyNodeName + "[@" + IdAttribute + "='{0}']";

        public const string IncludeAttribute = "Include";
        public const string CamelCaseVersionAttribute = "Version";

        // package config constants
        public const string PackageComponentPath = "/packages/package";

        // project file constants
        public const string PackageReferencePath = "/Project/ItemGroup/PackageReference";
    }
}
