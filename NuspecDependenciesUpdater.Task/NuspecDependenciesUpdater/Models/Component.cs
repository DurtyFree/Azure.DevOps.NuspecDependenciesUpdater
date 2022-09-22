namespace NuspecDependenciesUpdater.Models
{
    public class Component
    {
        public Component(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; }

        public string Version { get; }

        public override string ToString()
        {
            return string.Concat(Id, ".", Version);
        }
    }
}
