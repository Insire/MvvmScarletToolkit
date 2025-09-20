using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Collections.Generic;

namespace Build.Tasks
{
    public sealed class Test : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            foreach (var (folder, projectFile, frameworks) in BuildContext.GetTestProjects())
            {
                var projectFilePath = context.Environment.WorkingDirectory.Combine(folder).CombineWithFilePath(projectFile);

                foreach (var framework in frameworks)
                {
                    var projectCoverage = context.CoveragePath.CombineWithFilePath(System.IO.Path.GetFileNameWithoutExtension(projectFile) + $"_{framework}_" + ".coverage");
                    var settings = new ProcessSettings()
                        .UseWorkingDirectory(".")
                        .WithArguments(builder => builder
                            .Append("run")
                            .AppendSwitchQuoted("--project", " ", projectFilePath.FullPath)
                            .Append($"-c {BuildContext.BuildConfiguration}")
                            .AppendSwitch($"--framework", " ", framework)
                            .Append($"--coverage")
                            .Append($"--coverage-output-format cobertura")
                            .AppendSwitchQuoted($"--coverage-output", " ", projectCoverage.FullPath)
                        );

                    settings.EnvironmentVariables = new Dictionary<string, string>() { ["Environment"] = "Test" };

                    context.StartProcess("dotnet", settings);
                }
            }
        }
    }
}
