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
            // every project runs, so that the coverage report and test results stay complete,
            // failures are collected and fail the build in TestAndUploadReport after publishing
            foreach (var (folder, projectFile, frameworks) in context.GetTestProjects())
            {
                var projectFilePath = context.Environment.WorkingDirectory.Combine(folder).CombineWithFilePath(projectFile);

                foreach (var framework in frameworks)
                {
                    var name = System.IO.Path.GetFileNameWithoutExtension(projectFile) + $"_{framework}";
                    var projectCoverage = context.CoveragePath.CombineWithFilePath(name + "_.coverage");
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
                            .AppendSwitchQuoted("--results-directory", " ", context.TestResultsPath.FullPath)
                            .Append("--report-xunit-trx")
                            .AppendSwitchQuoted("--report-xunit-trx-filename", " ", name + ".trx")
                        );

                    settings.EnvironmentVariables = new Dictionary<string, string>() { ["Environment"] = "Test" };

                    var exitCode = context.StartProcess("dotnet", settings);
                    if (exitCode != 0)
                    {
                        context.TestFailures.Add($"{projectFile} ({framework}) exited with {exitCode}");
                    }
                }
            }
        }
    }
}
