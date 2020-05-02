#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1
#tool nuget:?package=OpenCover&version=4.7.922
#tool nuget:?package=ReportGenerator&version=4.5.6
#tool nuget:?package=Codecov&version=1.10.0

#addin nuget:?package=Cake.Codecov&version=0.8.0
#addin nuget:?package=Cake.Incubator&version=5.1.0
#addin nuget:?package=Cake.GitVersioning&version=3.1.91

using Cake.Core;

///////////////////////////////////////////////////////////////////////////////
// SETUP
///////////////////////////////////////////////////////////////////////////////

var Configuration = Argument("configuration", "Release");

const string Platform = "AnyCPU";
const string SolutionPath =".\\MvvmScarletToolkit.sln";
const string AssemblyInfoPath =".\\src\\SharedAssemblyInfo.cs";
const string PackagePath = ".\\packages";
const string ResultsPath = ".\\results";

var reportsFolder = new DirectoryPath(ResultsPath).Combine("reports");
var coverageResultFile = new DirectoryPath(ResultsPath).CombineWithFilePath("coverage.xml");
var vstestResultsFilePath = new DirectoryPath(ResultsPath).CombineWithFilePath("testresults.xml");

var nugetPackageProjects = new[]
{
    @".\src\MvvmScarletToolkit.Abstractions\MvvmScarletToolkit.Abstractions.csproj",
    @".\src\MvvmScarletToolkit\MvvmScarletToolkit.csproj",
    @".\src\MvvmScarletToolkit.Messenger\MvvmScarletToolkit.Messenger.csproj",
    @".\src\MvvmScarletToolkit.Commands\MvvmScarletToolkit.Commands.csproj",
    @".\src\MvvmScarletToolkit.Observables\MvvmScarletToolkit.Observables.csproj",
    @".\src\MvvmScarletToolkit.Wpf\MvvmScarletToolkit.Wpf.csproj",
    @".\src\MvvmScarletToolkit.Xamarin.Forms\MvvmScarletToolkit.Xamarin.Forms.csproj",
};

private void Build(string path)
{
    var settings = new ProcessSettings()
        .UseWorkingDirectory(".")
        .WithArguments(builder => builder
            .Append("build")
            .AppendQuoted(path)
            .Append("--nologo")
            .Append($"-c {Configuration}")
            .Append($"-p:GeneratePackageOnBuild=false") // we package only specific projects and we do that in a second cli call
            .Append($"-p:DebugType=full") // required for opencover codecoverage and sourcelinking
            .Append($"-p:SourceLinkCreate=true")
    );

    StartProcess("dotnet", settings);
}

Setup(ctx =>
{
    if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    {
        var appveyorRepoTag = EnvironmentVariable("APPVEYOR_REPO_TAG") ;
        Information($"APPVEYOR_REPO_TAG: {appveyorRepoTag}");
    }
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Debug")
    .Does(()=>
    {
        Debug("IsLocalBuild: " + BuildSystem.IsLocalBuild);
        Debug("IsRunningOnAppVeyor: " + BuildSystem.IsRunningOnAppVeyor);
        Debug("IsRunningOnAzurePipelines: " + BuildSystem.IsRunningOnAzurePipelines);
        Debug("IsRunningOnAzurePipelinesHosted: " + BuildSystem.IsRunningOnAzurePipelinesHosted);

        Information("Provider: " + BuildSystem.Provider);

        foreach(var entry in Context.EnvironmentVariables())
        {
            Verbose(entry.Key + " " + entry.Value);
        }
    });

Task("CleanSolution")
    .Does(() =>
    {
        var solution = ParseSolution(SolutionPath);

        foreach(var project in solution.Projects)
        {
            // check solution items and exclude solution folders, since they are virtual
            if(project.Name == "Solution Items")
                continue;

            var customProject = ParseProject(project.Path, configuration: Configuration, platform: Platform);

            foreach(var path in customProject.OutputPaths)
            {
                CleanDirectory(path.FullPath);
            }
        }

        var folders = new[]
        {
            new DirectoryPath(PackagePath),
            new DirectoryPath(ResultsPath),
        };

        foreach(var folder in folders)
        {
            EnsureDirectoryExists(folder);
            CleanDirectory(folder,(file) => !file.Path.Segments.Last().Contains(".gitignore"));
        }
});

Task("UpdateAssemblyInfo")
    .Does(() =>
    {
        var gitVersionInformation = GitVersioningGetVersion();
        var assemblyInfoParseResult = ParseAssemblyInfo(AssemblyInfoPath);

        var settings = new AssemblyInfoSettings()
        {
            Product                 = assemblyInfoParseResult.Product,
            Company                 = assemblyInfoParseResult.Company,
            Trademark               = assemblyInfoParseResult.Trademark,
            Copyright               = $"© {DateTime.Today.Year} Insire",

            InternalsVisibleTo      = assemblyInfoParseResult.InternalsVisibleTo,

            MetaDataAttributes = new []
            {
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Platform",
                    Value = Platform,
                },
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Compiled on:",
                    Value = "[UTC]" + DateTime.UtcNow.ToString(),
                },
            }
        };

        CreateAssemblyInfo(new FilePath(AssemblyInfoPath), settings);
});

Task("BuildAndPack")
    .DoesForEach(nugetPackageProjects, project=>
    {
        Build(project);
        Pack(project);

        void Pack(string path)
        {
            var version = GitVersioningGetVersion().SemVer2;

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
                    .Append($"-c {Configuration}")
                    .Append($"--output \"{PackagePath}\"")
                    .Append($"-p:PackageVersion={version}")
                );

            StartProcess("dotnet", settings);
        }
    });

Task("BuildAndTest")
    .Does(()=>
    {
        var project = @".\src\MvvmScarletToolkit.Tests\MvvmScarletToolkit.Tests.csproj";

        Build(project);
        Test(project);

        void Test(string path)
        {
            var testSettings = new DotNetCoreTestSettings
            {
                NoBuild = true,
                NoRestore = true,
                ResultsDirectory = ResultsPath,
                VSTestReportPath = vstestResultsFilePath,
                Configuration = Configuration,
                Framework = "netcoreapp3.1",
                ArgumentCustomization = builder=> builder
                                                    .AppendQuoted("--nologo")
            };

            var openCoverSettings = new OpenCoverSettings()
            {
                OldStyle = true,
            };

            Information("generating opencover report");
            OpenCover(tool => tool.DotNetCoreTest(path, testSettings), coverageResultFile, openCoverSettings);

            if(BuildSystem.IsLocalBuild)
            {
                Information("generating html report");
                GenerateReport(coverageResultFile, ReportGeneratorReportType.Html, "html");
            }

            if(BuildSystem.IsRunningOnAzurePipelinesHosted)
            {
                Information("generating report for azure devops");
                GenerateReport(coverageResultFile, ReportGeneratorReportType.Cobertura, "cobertura");

                var codeCovToken = EnvironmentVariable("CODECOV_TOKEN");
                if(string.IsNullOrEmpty(codeCovToken))
                {
                    return;
                }

                Information("uploading report to codecov");
                Codecov(new[] { coverageResultFile.FullPath }, codeCovToken);
            }
        }

        void GenerateReport(FilePath inputFile, ReportGeneratorReportType type, string subFolder)
        {
            var ReportGeneratorSettings = new ReportGeneratorSettings()
            {
                AssemblyFilters = new[]
                {
                    "-MvvmScarletToolkit.Tests*",
                    "-NUnit*",
                },
                ClassFilters = new[]
                {
                    "-System*",
                    "-Microsoft*",
                },
                ReportTypes = new[]
                {
                    type
                }
            };

            ReportGenerator(inputFile, reportsFolder.Combine(subFolder), ReportGeneratorSettings);
        }
    });

Task("PushLocally")
    .WithCriteria(() => BuildSystem.IsLocalBuild && DirectoryExists(@"D:\Drop\NuGet"))
    .DoesForEach(() => GetFiles(PackagePath + "\\*.nupkg"), path =>
    {
        var settings = new ProcessSettings()
            .UseWorkingDirectory(".")
            .WithArguments(builder => builder
            .Append("push")
            .AppendSwitchQuoted("-source", @"D:\Drop\NuGet")
            .AppendQuoted(path.FullPath));

        StartProcess(".\\tools\\nuget.exe",settings);
    });

Task("Default")
    .IsDependentOn("Debug")
    .IsDependentOn("CleanSolution")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("BuildAndPack")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("PushLocally");

RunTarget(Argument("target", "Default"));
