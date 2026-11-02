using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Diagnostics;
using System.Xml.Linq;

namespace Build.Tasks
{
    public sealed class Test : FrostingTask<BuildContext>
    {
        private sealed record TestRun(FilePath ProjectFile, string Name, string Framework);

        public override void Run(BuildContext context)
        {
            var runs = context.GetTestProjects()
                .SelectMany(p => p.Frameworks.Select(framework => new TestRun(
                    context.Environment.WorkingDirectory.Combine(p.Folder).CombineWithFilePath(p.ProjectFile),
                    System.IO.Path.GetFileNameWithoutExtension(p.ProjectFile),
                    framework)))
                .ToArray();

            Build(context, runs);

            // every run happens, so that the coverage report and test results stay complete,
            // failures are collected and fail the build in TestAndUploadReport after publishing
            var outputLock = new object();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount) };

            Parallel.ForEach(runs, options, run =>
            {
                var stopwatch = Stopwatch.StartNew();
                var (exitCode, output) = Execute(context, run);

                lock (outputLock)
                {
                    Report(context, run, exitCode, output, stopwatch.Elapsed);

                    if (exitCode != 0)
                    {
                        context.TestFailures.Add($"{run.ProjectFile.GetFilename()} ({run.Framework}) exited with {exitCode}");
                    }
                }
            });
        }

        /// <summary>
        /// Builds all runs in one parallel build, instead of every dotnet run building the shared projects again
        /// </summary>
        private static void Build(BuildContext context, TestRun[] runs)
        {
            var traversal = context.ResultsPath.CombineWithFilePath("tests.proj");
            var project = new XElement("Project",
                new XAttribute("DefaultTargets", "Build"),
                new XElement("ItemGroup",
                    runs.Select(p => p.ProjectFile.FullPath).Distinct().Select(p => new XElement("RestoreProject", new XAttribute("Include", p))),
                    runs.Select(p => new XElement("TestProject",
                        new XAttribute("Include", p.ProjectFile.FullPath),
                        new XAttribute("AdditionalProperties", $"TargetFramework={p.Framework}")))),
                new XElement("Target", new XAttribute("Name", "Restore"),
                    new XElement("MSBuild", new XAttribute("Projects", "@(RestoreProject)"), new XAttribute("Targets", "Restore"))),
                new XElement("Target", new XAttribute("Name", "Build"),
                    new XElement("MSBuild", new XAttribute("Projects", "@(TestProject)"), new XAttribute("Targets", "Build"), new XAttribute("BuildInParallel", "true"))));

            System.IO.Directory.CreateDirectory(context.ResultsPath.FullPath);
            project.Save(traversal.FullPath);

            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("build")
                    .AppendQuoted(traversal.FullPath)
                    .Append($"-c {BuildContext.BuildConfiguration}")
                    .Append("-m")
                );

            settings.EnvironmentVariables = new Dictionary<string, string>() { ["Environment"] = "Test" };

            context.StartProcessOrFail("dotnet", settings, "Building the test projects");
        }

        private static (int ExitCode, IReadOnlyList<string> Output) Execute(BuildContext context, TestRun run)
        {
            var projectCoverage = context.CoveragePath.CombineWithFilePath($"{run.Name}_{run.Framework}.cobertura.xml");
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("run")
                    .AppendSwitchQuoted("--project", " ", run.ProjectFile.FullPath)
                    .Append($"-c {BuildContext.BuildConfiguration}")
                    .AppendSwitch("--framework", " ", run.Framework)
                    .Append("--no-build")
                    .Append("--coverage")
                    .Append("--coverage-output-format cobertura")
                    .AppendSwitchQuoted("--coverage-output", " ", projectCoverage.FullPath)
                    .AppendSwitchQuoted("--results-directory", " ", context.TestResultsPath.FullPath)
                    .Append("--report-xunit-trx")
                    .AppendSwitchQuoted("--report-xunit-trx-filename", " ", $"{run.Name}_{run.Framework}.trx")
                );

            // runs write at the same time, their output is printed as a whole once they are done
            settings.RedirectStandardOutput = true;
            settings.RedirectStandardError = true;
            settings.EnvironmentVariables = new Dictionary<string, string>() { ["Environment"] = "Test" };

            var exitCode = context.StartProcess("dotnet", settings, out var standardOutput, out var standardError);

            return (exitCode, standardOutput.Concat(standardError).ToList());
        }

        private static void Report(BuildContext context, TestRun run, int exitCode, IReadOnlyList<string> output, TimeSpan duration)
        {
            var title = $"{run.Name} ({run.Framework}) {(exitCode == 0 ? "passed" : "failed")} after {duration:mm\\:ss}";

            // collapses passing runs in the log, failing ones stay open
            var group = exitCode == 0 && context.BuildSystem().IsRunningOnAzurePipelines;

            context.Information("{0}", group ? $"##[group]{title}" : title);

            foreach (var line in output)
            {
                context.Information("{0}", line);
            }

            if (group)
            {
                context.Information("##[endgroup]");
            }
        }
    }
}
