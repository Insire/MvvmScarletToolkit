using System.Text.Json;
using System.Xml.Linq;

namespace Build
{
    /// <summary>
    /// Versions of the tools Cake installs, from the files Dependabot keeps up to date.
    /// Hardcoded versions in <see cref="Program"/> fell behind the manifest, which build.ps1 and build.sh restore as well.
    /// </summary>
    internal sealed class ToolVersions
    {
        private const string ManifestPath = ".config/dotnet-tools.json";
        private const string PackagesPath = "Directory.Packages.props";

        private readonly JsonElement _tools;
        private readonly XDocument _packages;

        private ToolVersions(JsonElement tools, XDocument packages)
        {
            _tools = tools;
            _packages = packages;
        }

        /// <summary>
        /// Searches the repository root upwards from the working directory, which is the cake folder when started with dotnet run.
        /// </summary>
        public static ToolVersions Load()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, ManifestPath)))
            {
                directory = directory.Parent;
            }

            if (directory is null)
            {
                throw new InvalidOperationException($"{ManifestPath} wasn't found above {Directory.GetCurrentDirectory()}");
            }

            using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory.FullName, ManifestPath)));
            var packages = XDocument.Load(Path.Combine(directory.FullName, PackagesPath));

            return new ToolVersions(manifest.RootElement.GetProperty("tools").Clone(), packages);
        }

        /// <summary>
        /// The version of a dotnet tool in .config/dotnet-tools.json
        /// </summary>
        public string DotNetTool(string package)
        {
            if (!_tools.TryGetProperty(package, out var tool))
            {
                throw new InvalidOperationException($"{package} is missing in {ManifestPath}");
            }

            return tool.GetProperty("version").GetString()
                ?? throw new InvalidOperationException($"{package} has no version in {ManifestPath}");
        }

        /// <summary>
        /// The version of a package in Directory.Packages.props, for tools that come as NuGet packages instead of dotnet tools
        /// </summary>
        public string Package(string package)
        {
            var version = _packages.Descendants()
                .Where(p => p.Name.LocalName == "PackageVersion" && string.Equals((string?)p.Attribute("Include"), package, StringComparison.OrdinalIgnoreCase))
                .Select(p => (string?)p.Attribute("Version"))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(version) || version.Contains("$(", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{package} has no literal version in {PackagesPath}");
            }

            return version;
        }
    }
}
