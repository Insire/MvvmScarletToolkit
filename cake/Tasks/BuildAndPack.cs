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
                var settings = new ProcessSettings()
                    .UseWorkingDirectory(".")
                    .WithArguments(builder => builder
                        .Append("pack")
                        .AppendQuoted(path)
                        .Append("--include-symbols")
                        .Append("--include-source")
                        .Append("--no-build")
                        .Append("--no-restore")
                        .Append("--nologo")
                        .Append($"-c {Context.BuildConfiguration}")
                        .Append($"--output \"{Context.PackagePath}\"")
                        .Append($"-p:PackageVersion={context.GitVersioningGetVersion().SemVer2}")
                        .Append($"-p:PublicRelease={context.IsPublicRelease}")
                    );

                context.StartProcess("dotnet", settings);
            }
        }
    }
}
