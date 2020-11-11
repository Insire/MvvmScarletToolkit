#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1
#tool nuget:?package=ReportGenerator&version=4.7.1
#tool nuget:?package=Codecov&version=1.12.3
#tool nuget:?package=GitVersion.CommandLine&version=5.5.0
#tool nuget:?package=Microsoft.CodeCoverage&version=16.7.1

#addin nuget:?package=Cake.Codecov&version=0.9.1
#addin nuget:?package=Cake.Incubator&version=5.1.0
#addin nuget:?package=Cake.GitVersioning&version=3.3.37

using Cake.Core;

///////////////////////////////////////////////////////////////////////////////
// SETUP
///////////////////////////////////////////////////////////////////////////////

const string Configuration = "Release";
const string Platform = "AnyCPU";
const string SolutionPath ="./MvvmScarletToolkit.sln";
const string AssemblyInfoPath ="./src/SharedAssemblyInfo.cs";
const string PackagePath = "./packages";
const string ResultsPath = "./results";
const string CoberturaResultsPath = "./results/reports/cobertura";
const string localNugetDirectory = @"D:\Drop\NuGet";


var packageFolder = MakeAbsolute(new DirectoryPath(PackagePath));
var reportsFolder = new DirectoryPath(ResultsPath).Combine("reports");
var coberturaResultFile = new DirectoryPath(CoberturaResultsPath).CombineWithFilePath("Cobertura.xml");
var vstestResultsFile = new FilePath("vsTestResults.trx");
var codeCoverageBinaryFile = new FilePath("vsCodeCoverage.coverage");
var codeCoverageResultsFile = new FilePath("vsCodeCoverage.xml");

var publicRelease = false;
var gitVersion = default(GitVersion);

// projects that are supposed to generate a nuget package
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

private void GenerateReport(FilePath inputFile, ReportGeneratorReportType type, string subFolder)
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

    ReportGenerator(inputFile, reportsFolder.Combine(subFolder), ReportGeneratorSettings);
}

private void MergeReports(string pattern, ReportGeneratorReportType type, string subFolder)
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

    ReportGenerator(pattern, reportsFolder.Combine(subFolder), ReportGeneratorSettings);
}

private void Clean(bool cleanBin, bool cleanObj, bool cleanOutput, bool cleanMisc)
{
    var solution = ParseSolution(SolutionPath);

    foreach(var project in solution.Projects)
    {
        // check solution items and exclude solution folders, since they are virtual
        if(project.Name == "Solution Items" || project.Name == "Samples")
            continue;

        var projectFile = project.Path; // FilePath
        if(cleanBin)
        {
            var binFolder = projectFile.GetDirectory().Combine("bin");
            if(DirectoryExists(binFolder))
            {
                CleanDirectory(binFolder);
            }
        }

        if(cleanObj)
        {
            var objFolder = projectFile.GetDirectory().Combine("obj");
            if(DirectoryExists(objFolder))
            {
                CleanDirectory(objFolder);
            }
        }

        if(cleanOutput)
        {
            var customProject = ParseProject(project.Path, configuration: Configuration, platform: Platform);
            foreach(var path in customProject.OutputPaths.Where(p => p != null))
            {
                CleanDirectory(path.FullPath);
            }
        }
    }
    if(cleanMisc)
    {
        var folders = new[]
        {
            new DirectoryPath(PackagePath),
            new DirectoryPath(ResultsPath),
        };

        foreach(var folder in folders)
        {
            EnsureDirectoryExists(folder);
            CleanDirectory(folder, (file) => !file.Path.Segments.Last().Contains(".gitignore"));
        }
    }
}

Setup(ctx =>
{
    gitVersion = GitVersion();
    if(gitVersion.BranchName == "master")
    {
        publicRelease = true;
        Information("Building a public release.");
    }
    else
    {
        Information("Building a pre-release.");
    }

    Information($"Provider: {BuildSystem.Provider}");
    Information($"Platform: {Context.Environment.Platform.Family} ({(Context.Environment.Platform.Is64Bit ? "x64" : "x86")})");

    Information($"nuget.exe ({ctx.Tools.Resolve("nuget.exe")}) {(FileExists(Context.Tools.Resolve("nuget.exe")) ? "was found" : "is missing")}");
    Information($"dotnet.exe ({ctx.Tools.Resolve("dotnet.exe")}) {(FileExists(Context.Tools.Resolve("dotnet.exe")) ? "was found" : "is missing")}");
    Information($"CodeCoverage.exe ({ctx.Tools.Resolve("CodeCoverage.exe")}) {(FileExists(Context.Tools.Resolve("CodeCoverage.exe")) ? "was found" : "is missing")}");

    Information($"NUGETORG_APIKEY was{(string.IsNullOrEmpty(EnvironmentVariable("NUGETORG_APIKEY")) ? " not" : "")} set.");
    Information($"CODECOV_TOKEN was{(string.IsNullOrEmpty(EnvironmentVariable("CODECOV_TOKEN")) ? " not" : "")} set.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////
Task("CleanSolution")
    .Does(() =>
    {
        Clean(true,true,true,true);
    });

Task("CleanSolutionAgain")
    .Does(() =>
    {
        Clean(true,true,true,false);
    });

Task("UpdateAssemblyInfo")
    .Does(() =>
    {
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
                    Key = "CompileDate",
                    Value = "[UTC]" + DateTime.UtcNow.ToString(),
                },
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "PublicRelease",
                    Value = publicRelease.ToString(),
                },
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Branch",
                    Value = gitVersion.BranchName,
                },
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Commit",
                    Value = gitVersion.Sha,
                },
            }
        };

        CreateAssemblyInfo(new FilePath(AssemblyInfoPath), settings);
});

Task("BuildAndPack")
    .IsDependentOn("CleanSolutionAgain")
    .Does(()=>
    {
        var settings = new ProcessSettings()
            .UseWorkingDirectory(".")
            .WithArguments(builder => builder
                .Append("pack")
                .AppendQuoted("./MvvmScarletToolkit.slnf")
                .Append($"-c {Configuration}")
                .Append($"--output \"{PackagePath}\"")
                .Append($"-p:PackageVersion={GitVersioningGetVersion().SemVer2}")
                .Append($"-p:PublicRelease={publicRelease}") // Nerdbank.GitVersioning - omit git commit ID

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

        StartProcess(Context.Tools.Resolve("dotnet.exe"), settings);
    });

Task("Test")
    .Does(()=>
    {
        var projectFile = @"./src/MvvmScarletToolkit.Tests/MvvmScarletToolkit.Tests.csproj";
        var testSettings = new DotNetCoreTestSettings
        {
            Framework = "netcoreapp3.1",
            Configuration = Configuration,
            NoBuild = false,
            NoRestore = false,
            ArgumentCustomization = builder => builder
                .Append("--nologo")
                .Append("--results-directory:./Results/coverage")
                .Append($"-p:DebugType=full") // required for opencover codecoverage and sourcelinking
                .Append($"-p:DebugSymbols=true") // required for opencover codecoverage
                .AppendSwitchQuoted("--collect",":","\"\"Code Coverage\"\"")
                .Append($"--logger:trx;LogFileName=..\\{vstestResultsFile.FullPath};"),
        };

        DotNetCoreTest(projectFile, testSettings);
    });

Task("ConvertCoverage")
    .IsDependentOn("Test")
    .WithCriteria(()=> Context.Tools.Resolve("CodeCoverage.exe") != null, $"since CodeCoverage.exe is not a registered tool.")
    .DoesForEach(()=> GetFiles($"{ResultsPath}/coverage/**/*.coverage"), file=>
    {
        var codeCoverageExe = Context.Tools.Resolve("CodeCoverage.exe");
        var result = System.IO.Path.ChangeExtension(file.FullPath, ".xml");

        var settings = new ProcessSettings()
                .UseWorkingDirectory(ResultsPath)
                .WithArguments(builder => builder
                    .Append("analyze")
                    .AppendSwitchQuoted(@"-output",":",result)
                    .Append(file.FullPath)
                );

        StartProcess(codeCoverageExe.FullPath, settings);
    });

Task("CoberturaReport")
    .IsDependentOn("ConvertCoverage")
    .WithCriteria(()=> GetFiles("./Results/coverage/**/*.xml").Count > 0, $"since there is no coverage xml file in /Results/coverage/.")
    .WithCriteria(()=> BuildSystem.IsRunningOnAzurePipelinesHosted, "since task is not running on a Azure Pipelines (Hosted).")
    .Does(()=>
    {
        MergeReports("./Results/coverage/**/*.xml", ReportGeneratorReportType.Cobertura, "cobertura");
    });

Task("HtmlReport")
    .IsDependentOn("ConvertCoverage")
    .WithCriteria(()=> GetFiles("./Results/coverage/**/*.xml").Count > 0, $"since there is no coverage xml file in /Results/coverage/.")
    .WithCriteria(()=> BuildSystem.IsLocalBuild, "since task is not running on a developer machine.")
    .Does(()=>
    {
        MergeReports("./Results/coverage/**/*.xml", ReportGeneratorReportType.Html, "html");
    });

Task("UploadCodecovReport")
    .IsDependentOn("CoberturaReport")
    .WithCriteria(()=> FileExists(coberturaResultFile), $"since {coberturaResultFile} wasn't created.")
    .WithCriteria(()=> BuildSystem.IsRunningOnAzurePipelinesHosted, "since task is not running on AzurePipelines (Hosted).")
    .WithCriteria(() => !string.IsNullOrEmpty(EnvironmentVariable("CODECOV_TOKEN")),"since environment variable CODECOV_TOKEN missing or empty.")
    .Does(()=>
    {
        Codecov(new[] { coberturaResultFile.FullPath }, EnvironmentVariable("CODECOV_TOKEN"));
    });

Task("TestAndUploadReport")
    .IsDependentOn("HtmlReport")
    .IsDependentOn("UploadCodecovReport");

Task("PushLocally")
    .IsDependentOn("BuildAndPack")
    .WithCriteria(() => BuildSystem.IsLocalBuild, "since task is not running on a developer machine.")
    .WithCriteria(() => DirectoryExists(localNugetDirectory), $@"since there is no local directory ({localNugetDirectory}) to push nuget packages to.")
    .WithCriteria(() => FileExists(Context.Tools.Resolve("nuget.exe")), $@"since there is no nuget.exe registered with cake")
    .Does(() =>
    {
        var settings = new ProcessSettings()
            .UseWorkingDirectory(".")
            .WithArguments(builder => builder
                .AppendSwitchQuoted("nuget add source", localNugetDirectory)
                .AppendSwitch("-name","Local"));

        StartProcess(Context.Tools.Resolve("dotnet.exe"), settings);

        foreach(var path in GetFiles(PackagePath + "/*.nupkg"))
        {
            settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("push")
                    .AppendSwitchQuoted("-source", localNugetDirectory)
                    .AppendQuoted(path.FullPath));

            StartProcess(Context.Tools.Resolve("nuget.exe"), settings);
        }
    });

Task("PushRemote")
    .IsDependentOn("BuildAndPack")
    .WithCriteria(() => BuildSystem.IsRunningOnAzurePipelines || BuildSystem.IsRunningOnAzurePipelinesHosted, "since task is running on a azure devops.")
    .WithCriteria(() => !string.IsNullOrEmpty(EnvironmentVariable("NUGETORG_APIKEY")),"since environment variable NUGETORG_APIKEY missing or empty.")
    .WithCriteria(() => FileExists(Context.Tools.Resolve("nuget.exe")), $@"since there is no nuget.exe registered with cake")
    .Does(() =>
    {
        foreach(var package in GetFiles(packageFolder.FullPath + "/*.nupkg"))
        {
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("push")
                    .AppendQuoted(package.FullPath)
                    .AppendSwitchSecret("-apikey", EnvironmentVariable("NUGETORG_APIKEY"))
                    .AppendSwitchQuoted("-source", "https://api.nuget.org/v3/index.json")
                    .Append("-SkipDuplicate")
                    .AppendSwitch("-Verbosity", "detailed")
                );

            StartProcess(Context.Tools.Resolve("nuget.exe"), settings);
        }
    });

Task("Push")
    .IsDependentOn("PushRemote")
    .IsDependentOn("PushLocally");

Task("Default")
    .IsDependentOn("CleanSolution")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("TestAndUploadReport")
    .IsDependentOn("BuildAndPack")
    .IsDependentOn("Push");

RunTarget(Argument("target", "Default"));
