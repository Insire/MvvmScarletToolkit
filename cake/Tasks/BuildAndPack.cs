using Build;
using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.GitVersioning;
using System.Linq;

public sealed class BuildAndPack : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        foreach (var project in context.NugetPackageProjects)
        {
            context.Build(project);
            Pack(project);

            void Pack(string path)
            {
                var semver = context.GitVersioningGetVersion().SemVer2;
                var settings = new ProcessSettings()
                    .UseWorkingDirectory(".")
                    .WithArguments(builder => builder
                        .Append("pack")
                        .AppendQuoted("./MvvmScarletToolkit.slnf")
                        .Append($"-c {Context.BuildConfiguration}")
                        .Append($"--output \"{Context.PackagePath}\"")
                        .Append($"-p:PackageVersion={context.GitVersioningGetVersion().SemVer2}")
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
