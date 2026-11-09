using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Xml.Linq;

namespace Build.Tasks
{
    public sealed class BuildAndPack : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var semver = context.GitVersion?.SemVer2;
            if (semver is null)
            {
                return;
            }

            var traversal = CreateTraversal(context);

            // the properties apply to every project the traversal packs
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("pack")
                    .AppendQuoted(traversal.FullPath)
                    .Append($"-c {BuildContext.BuildConfiguration}")
                    .Append($"--output \"{context.PackagePath.FullPath}\"")
                    .Append($"-p:PackageVersion={semver}")
                    .Append($"-p:PublicRelease={context.IsPublicRelease}") // Nerdbank.GitVersioning - omit git commit ID

                    // Creating symbol packages
                    .Append("--include-symbols")
                    .Append("--include-source")
                    .Append("-p:SymbolPackageFormat=snupkg")

                    // enable source linking
                    .Append("-p:PublishRepositoryUrl=true")

                    // Deterministic Builds
                    .Append("-p:EmbedUntrackedSources=true")

                    .Append("-p:DebugType=portable")
                    .Append("-p:DebugSymbols=true")
                );

            context.StartProcessOrFail("dotnet", settings, "Packing the packages");
        }

        /// <summary>
        /// Packs all projects in one parallel build. A dotnet pack per project ran one after another and built the shared projects again in every process,
        /// on the build agent that took about 2 minutes, mostly for the WPF and Avalonia packages.
        /// </summary>
        private static FilePath CreateTraversal(BuildContext context)
        {
            var traversal = context.ResultsPath.CombineWithFilePath("packages.proj");
            var projects = context.NugetPackageProjects.Select(p => new XElement("PackageProject", new XAttribute("Include", p.ProjectFile.FullPath)));

            var project = new XElement("Project",
                new XAttribute("DefaultTargets", "Pack"),
                new XElement("ItemGroup", projects),
                new XElement("Target", new XAttribute("Name", "Restore"),
                    new XElement("MSBuild", new XAttribute("Projects", "@(PackageProject)"), new XAttribute("Targets", "Restore"))),
                new XElement("Target", new XAttribute("Name", "Pack"),
                    new XElement("MSBuild", new XAttribute("Projects", "@(PackageProject)"), new XAttribute("Targets", "Pack"), new XAttribute("BuildInParallel", "true"))));

            System.IO.Directory.CreateDirectory(context.ResultsPath.FullPath);
            project.Save(traversal.FullPath);

            return traversal;
        }
    }
}
