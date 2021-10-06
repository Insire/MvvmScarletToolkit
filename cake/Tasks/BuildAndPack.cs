using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Linq;

namespace Build
{
    public sealed class BuildAndPack : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            foreach (var project in context.NugetPackageProjects)
            {
                var semver = context.GitVersion.SemVer2;
                var settings = new ProcessSettings()
                    .UseWorkingDirectory(".")
                    .WithArguments(builder => builder
                        .Append("pack")
                        .AppendQuoted("./MvvmScarletToolkit.slnf")
                        .Append($"-c {BuildContext.BuildConfiguration}")
                        .Append($"--output \"{BuildContext.PackagePath}\"")
                        .Append($"-p:PackageVersion={context.GitVersion.SemVer2}")
                        .Append($"-p:PublicRelease={context.IsPublicRelease}") // Nerdbank.GitVersioning - omit git commit ID

                        // Creating symbol packages
                        .Append($"-p:IncludeSymbols=true")
                        .Append($"-p:SymbolPackageFormat=snupkg")

                        // enable source linking
                        .Append($"-p:PublishRepositoryUrl=true")

                        // Deterministic Builds
                        .Append($"-p:EmbedUntrackedSources=true")

                        .Append($"-p:DebugType=portable")
                        .Append($"-p:DebugSymbols=true")
                    );

                context.StartProcess("dotnet", settings);
            }
        }
    }
}
