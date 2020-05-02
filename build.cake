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
        Information("IsLocalBuild: " + BuildSystem.IsLocalBuild);
        Information("IsRunningOnAppVeyor: " + BuildSystem.IsRunningOnAppVeyor);
        Information("IsRunningOnAzurePipelines: " + BuildSystem.IsRunningOnAzurePipelines);
        Information("IsRunningOnAzurePipelinesHosted: " + BuildSystem.IsRunningOnAzurePipelinesHosted);

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
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Commit date:",
                    Value = gitVersionInformation.GitCommitDate?.ToString(),
                },
            }
        };

        CreateAssemblyInfo(new FilePath(AssemblyInfoPath), settings);
});

Task("Build")
    .Does(()=>
    {
        var version = GitVersioningGetVersion().SemVer2;

        BuildAndPack(@".\src\MvvmScarletToolkit.Abstractions\MvvmScarletToolkit.Abstractions.csproj");
        BuildAndPack(@".\src\MvvmScarletToolkit\MvvmScarletToolkit.csproj");
        BuildAndPack(@".\src\MvvmScarletToolkit.Messenger\MvvmScarletToolkit.Messenger.csproj");
        BuildAndPack(@".\src\MvvmScarletToolkit.Commands\MvvmScarletToolkit.Commands.csproj");
        BuildAndPack(@".\src\MvvmScarletToolkit.Observables\MvvmScarletToolkit.Observables.csproj");
        BuildAndPack(@".\src\MvvmScarletToolkit.Wpf\MvvmScarletToolkit.Wpf.csproj");
        BuildAndPack(@".\src\MvvmScarletToolkit.Xamarin.Forms\MvvmScarletToolkit.Xamarin.Forms.csproj");

        BuildAndTest(@".\src\MvvmScarletToolkit.Tests\MvvmScarletToolkit.Tests.csproj");

        void BuildAndPack(string path)
        {
            Build(path);
            Pack(path);
        }

        void BuildAndTest(string path)
        {
            Build(path);
            Test(path);
        }

        void Build(string path)
        {
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("build")
                    .AppendQuoted(path)
                    .Append($"-c {Configuration}")
                    .Append($"-p:GeneratePackageOnBuild=false") // we package only specific projects and we do that in a second cli call
                    .Append($"-p:DebugType=full") // required for opencover codecoverage and sourcelinking
                    .Append($"-p:SourceLinkCreate=true")
            );

            StartProcess("dotnet", settings);
        }

        void Test(string path)
        {
            var resultFile = new DirectoryPath(ResultsPath).CombineWithFilePath("coverage.xml");
            var testSettings = new DotNetCoreTestSettings
            {
                NoBuild = true,
                NoRestore = true,
                ResultsDirectory = ResultsPath,
                VSTestReportPath = new DirectoryPath(ResultsPath).CombineWithFilePath("testresults.xml"),
                Configuration = Configuration,
                Framework = "netcoreapp3.1"
            };

            var openCoverSettings = new OpenCoverSettings()
            {
                OldStyle = true,
            };

            OpenCover(tool => tool.DotNetCoreTest(path, testSettings), resultFile, openCoverSettings);

            if(BuildSystem.IsLocalBuild)
            {
                // generate local html report
                GenerateReport(resultFile, ReportGeneratorReportType.Html, "html");
            }

            if(BuildSystem.IsRunningOnAzurePipelinesHosted)
            {
                // generate report for azure devops
                GenerateReport(resultFile, ReportGeneratorReportType.Cobertura, "cobertura");

                // generate report codecov
                var codeCovToken = EnvironmentVariable("CODECOV_TOKEN");
                if(string.IsNullOrEmpty(codeCovToken))
                {
                    return;
                }

                Codecov(new[] { resultFile.FullPath }, codeCovToken);
            }
        }

        void GenerateReport(FilePath inputFile, ReportGeneratorReportType type, string subFolder)
        {
            var folder = new DirectoryPath(ResultsPath).Combine("reports");
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

            ReportGenerator(inputFile, folder.Combine(subFolder), ReportGeneratorSettings);
        }

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
                    .Append($"-c {Configuration}")
                    .Append($"--output \"{PackagePath}\"")
                    .Append($"-p:PackageVersion={version}")
                );

            StartProcess("dotnet", settings);
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
    .IsDependentOn("Build")
    .IsDependentOn("PushLocally");

RunTarget(Argument("target", "Default"));
