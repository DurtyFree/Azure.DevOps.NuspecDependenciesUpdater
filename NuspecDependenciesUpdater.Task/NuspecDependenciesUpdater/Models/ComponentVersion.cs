using System;

namespace NuspecDependenciesUpdater.Models
{
    internal class ComponentVersion
            : IComparable
            , IComparable<ComponentVersion>
            , IEquatable<ComponentVersion>
    {
        public ComponentVersion(string versionString)
        {
            string[] versionParts = versionString.Split('-');
            Version = new Version(versionParts[0]);
            PreReleaseTag = versionParts.Length > 1 ? versionParts[1] : null;
        }

        public Version Version { get; }

        public string PreReleaseTag { get; }

        public bool IsPreRelease => !string.IsNullOrEmpty(PreReleaseTag);

        public int CompareTo(object obj)
        {
            return CompareTo(obj as ComponentVersion);
        }

        public int CompareTo(ComponentVersion other)
        {
            if (other == null)
            {
                return 1;
            }

            int result = Version.CompareTo(other.Version);
            return result == 0 
                ? IsPreRelease.CompareTo(other.IsPreRelease) 
                : result;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ComponentVersion);
        }

        public bool Equals(ComponentVersion other)
        {
            int result = CompareTo(other);
            return result == 0;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Version != null ? Version.GetHashCode() : 0) * 397)
                    ^ (PreReleaseTag != null ? PreReleaseTag.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return IsPreRelease ? Version + "-" + PreReleaseTag : Version.ToString();
        }
    }
}
