using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build.Tasks
{
    public sealed class Test : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            // every project runs, so that the coverage report stays complete, the task fails afterwards
            var failures = new List<string>();

            foreach (var (folder, projectFile, frameworks) in context.GetTestProjects())
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
                            .AppendSwitch("--framework", " ", framework)
                            .Append("--coverage")
                            .Append("--coverage-output-format cobertura")
                            .AppendSwitchQuoted("--coverage-output", " ", projectCoverage.FullPath)
                        );

                    settings.EnvironmentVariables = new Dictionary<string, string>() { ["Environment"] = "Test" };

                    var exitCode = context.StartProcess("dotnet", settings);
                    if (exitCode != 0)
                    {
                        failures.Add($"{projectFile} ({framework}) exited with {exitCode}");
                    }
                }
            }

            if (failures.Count > 0)
            {
                throw new CakeException("Tests failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
            }
        }
    }
}
