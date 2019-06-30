#addin nuget:?package=Cake.Incubator&version=5.0.1
using Cake.Core;

///////////////////////////////////////////////////////////////////////////////
// SETUP
///////////////////////////////////////////////////////////////////////////////

var Configuration = Argument("configuration", "Release");

const string Platform = "AnyCPU";
const string SolutionPath =".\\MvvmScarletToolkit.sln";
const string AssemblyInfoPath =".\\SharedAssemblyInfo.cs";
const string PackagePath = ".\\packages";

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
                CleanDirectory(path.FullPath);
        }

        var folders = new[]
        {
            new DirectoryPath(PackagePath),
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
        var assemblyInfoParseResult = ParseAssemblyInfo(AssemblyInfoPath);

        var settings = new AssemblyInfoSettings()
        {
            Version                 = assemblyInfoParseResult.AssemblyVersion,
            FileVersion             = assemblyInfoParseResult.AssemblyFileVersion,
            InformationalVersion    = assemblyInfoParseResult.AssemblyInformationalVersion,

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

        if (BuildSystem.IsLocalBuild)
        {
            settings.Version                 = Increase(settings.Version);
            settings.FileVersion             = Increase(settings.FileVersion);
            settings.InformationalVersion    = Increase(settings.InformationalVersion);
        }
        else
        {
            if(BuildSystem.IsRunningOnAzurePipelinesHosted)
            {
                var build = int.Parse(EnvironmentVariable("BUILD_BUILDNUMBER") ?? "no version found from AzurePipelinesHosted");
                settings.Version                 = IncreaseWith(settings.Version, build);
                settings.FileVersion             = IncreaseWith(settings.FileVersion, build);
                settings.InformationalVersion    = IncreaseWith(settings.InformationalVersion, build);

                Information($"Version: {settings.Version}");
                Information($"FileVersion: {settings.FileVersion}");
                Information($"InformationalVersion: {settings.InformationalVersion}");
            }
            else
            {
                var version = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "no version found from AppVeyorEnvironment";
                settings.Version                 = version;
                settings.FileVersion             = version;
                settings.InformationalVersion    = version;

                Information($"Version: {version}");
            }
        }

        CreateAssemblyInfo(new FilePath(AssemblyInfoPath), settings);

        string Increase(string data)
        {
            var version = new Version(data);
            return new Version(version.Major,version.Minor,version.Build+1, version.Revision).ToString();
        }

        string IncreaseWith(string data, int build)
        {
            var version = new Version(data);
            return new Version(version.Major,version.Minor,build, version.Revision).ToString();
        }
});

Task("Pack")
    .Does(()=>
    {
        var version = string.Empty;
        if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
        {
            version = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "no version found from AppVeyorEnvironment";
        }
        else
        {
            var assemblyInfoParseResult = ParseAssemblyInfo(AssemblyInfoPath);
            version = assemblyInfoParseResult.AssemblyVersion;
        }

        Build(@".\MvvmScarletToolkit.Abstractions\MvvmScarletToolkit.Abstractions.csproj");
        Build(@".\MvvmScarletToolkit.Commands\MvvmScarletToolkit.Commands.csproj");
        Build(@".\MvvmScarletToolkit.Observables\MvvmScarletToolkit.Observables.csproj");
        Build(@".\MvvmScarletToolkit.Implementations\MvvmScarletToolkit.Implementations.csproj");
        Build(@".\MvvmScarletToolkit\MvvmScarletToolkit.csproj");

        void Build(string path)
        {
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("build")
                    .AppendQuoted(path)
                    .Append("--force")
                    .Append($"-c {Configuration}")
            );

            StartProcess("dotnet", settings);

            Pack(path);
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
    .IsDependentOn("Pack")
    .IsDependentOn("PushLocally");

RunTarget(Argument("target", "Default"));
