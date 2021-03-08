using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;
using Cake.Incubator.Project;
using System.Linq;

namespace Build
{
    public static class Utils
    {
        internal static void Build(this Context context, string path)
        {
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("build")
                    .AppendQuoted(path)
                    .Append("--nologo")
                    .Append($"-c {Context.BuildConfiguration}")
                    .Append("-p:GeneratePackageOnBuild=false") // we package only specific projects and we do that in a second cli call
                    .Append("-p:DebugType=full") // required for opencover codecoverage and sourcelinking
                    .Append("-p:DebugSymbols=true") // required for opencover codecoverage
                    .Append("-p:SourceLinkCreate=true")
            );

            context.StartProcess("dotnet", settings);
        }

        internal static void GenerateReport(this Context context, FilePath inputFile, ReportGeneratorReportType type, string subFolder)
        {
            var ReportGeneratorSettings = new ReportGeneratorSettings()
            {
                AssemblyFilters = new[]
                {
                    "-MvvmScarletToolkit.Tests*",
                    "-nunit3*",
                },
                ClassFilters = new[]
                {
                    "-System*",
                    "-Microsoft*",
                },
                ReportTypes = new[]
                {
                    type
                },
            };

            context.ReportGenerator(inputFile, context.ReportsFolder.Combine(subFolder), ReportGeneratorSettings);
        }

        internal static void MergeReports(this Context context, string pattern, ReportGeneratorReportType type, string subFolder)
        {
            var ReportGeneratorSettings = new ReportGeneratorSettings()
            {
                AssemblyFilters = new[]
                {
                    "-MvvmScarletToolkit.Tests*",
                    "-nunit3*",
                },
                ClassFilters = new[]
                {
                    "-System*",
                    "-Microsoft*",
                },
                ReportTypes = new[]
                {
                    type
                },
            };

            context.ReportGenerator(new GlobPattern(pattern), context.ReportsFolder.Combine(subFolder), ReportGeneratorSettings);
        }

        public static void Clean(this Context context, bool cleanBin, bool cleanObj, bool cleanOutput, bool cleanMisc)
        {
            var solution = context.ParseSolution(Context.SolutionPath);

            foreach (var project in solution.Projects)
            {
                // check solution items and exclude solution folders, since they are virtual
                if (project.Type == "{2150E333-8FDC-42A3-9474-1A3956D46DE8}")
                {
                    continue;
                }

                var projectFile = project.Path; // FilePath
                if (cleanBin)
                {
                    var binFolder = projectFile.GetDirectory().Combine("bin");
                    if (context.DirectoryExists(binFolder))
                    {
                        context.CleanDirectory(binFolder);
                    }
                }

                if (cleanObj)
                {
                    var objFolder = projectFile.GetDirectory().Combine("obj");
                    if (context.DirectoryExists(objFolder))
                    {
                        context.CleanDirectory(objFolder);
                    }
                }

                if (cleanOutput)
                {
                    var customProject = context.ParseProject(project.Path, configuration: Context.BuildConfiguration, platform: Context.Platform);
                    foreach (var path in customProject.OutputPaths.Where(p => p != null))
                    {
                        context.CleanDirectory(path.FullPath);
                    }
                }
            }

            if (cleanMisc)
            {
                var folders = new[]
                {
                    Context.PackagePath,
                    Context.ResultsPath,
                };

                foreach (var folder in folders)
                {
                    context.EnsureDirectoryExists(folder);
                    context.CleanDirectory(folder, (file) => !file.Path.Segments.Last().Contains(".gitignore"));
                }
            }
        }
    }
}
