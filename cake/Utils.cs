using Cake.Common;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;

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
    }
}
