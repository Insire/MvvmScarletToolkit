#addin nuget:?package=Cake.Incubator&version=5.0.1

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

Task("Restore")
    .Does(()=>
    {
        DotNetCoreRestore(new DotNetCoreRestoreSettings
        {
            Sources = new[] {"https://api.nuget.org/v3/index.json"},
            Verbosity = DotNetCoreVerbosity.Minimal,
            DisableParallel = false,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
            {
                NoLogo = true,
            }
        });
    });

Task("Build")
    .Does(() =>
    {
        DotNetCoreBuild(".", new DotNetCoreBuildSettings
        {
            Framework = "netcoreapp3.0",
            Configuration = Configuration,
            NoRestore = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
            {
                MaxCpuCount = 0,
                ValidateProjectFile = true,
            }
        });
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

        NuGetPack(GetDefaultSettings("MvvmScarletToolkit","MvvmScarletToolkit is a personal project and framework to speed up the development process of WPF applications.", new DirectoryPath(".\\MvvmScarletToolkit\\bin\\Release\\")));
        NuGetPack(GetDefaultSettings("MvvmScarletToolkit.Abstractions","MvvmScarletToolkit.Abstractions ", new DirectoryPath(".\\MvvmScarletToolkit.Abstractions\\bin\\Release\\")));
        NuGetPack(GetDefaultSettings("MvvmScarletToolkit.Observables","MvvmScarletToolkit.Observables ", new DirectoryPath(".\\MvvmScarletToolkit.Observables\\bin\\Release\\")));
        NuGetPack(GetDefaultSettings("MvvmScarletToolkit.Commands","MvvmScarletToolkit.Commands ", new DirectoryPath(".\\MvvmScarletToolkit.Commands\\bin\\Release\\")));
        NuGetPack(GetDefaultSettings("MvvmScarletToolkit.FileSystemBrowser","MvvmScarletToolkit.FileSystemBrowser ", new DirectoryPath(".\\MvvmScarletToolkit.FileSystemBrowser\\bin\\Release\\")));
        NuGetPack(GetDefaultSettings("MvvmScarletToolkit.ConfigurableWindow","MvvmScarletToolkit.ConfigurableWindow ", new DirectoryPath(".\\MvvmScarletToolkit.ConfigurableWindow\\bin\\Release\\")));
        NuGetPack(GetDefaultSettings("MvvmScarletToolkit.Implementations","MvvmScarletToolkit.Implementations ", new DirectoryPath(".\\MvvmScarletToolkit.Implementations\\bin\\Release\\")));
        // dont provide a nuget package for incubator, because its not intended to be released
        // NuGetPack(GetDefaultSettings("MvvmScarletToolkit.Incubator","MvvmScarletToolkit.Incubator ", new DirectoryPath(".\\MvvmScarletToolkit.Incubator\\bin\\Release\\")));

        NuGetPackSettings GetDefaultSettings(string name, string summary, DirectoryPath basePath)
        {
            return new NuGetPackSettings()
            {
                Id                          = name,
                Version                     = version,
                Authors                     = new[] {"Insire"},
                Owners                      = new[] {"Insire"},
                Description                 = $"{name} v{version}",
                Summary                     = summary,
                ProjectUrl                  = new Uri(@"https://github.com/Insire/MvvmScarletToolkit"),
                LicenseUrl                  = new Uri(@"https://github.com/Insire/MvvmScarletToolkit/blob/master/LICENSE.md"),
                Copyright                   = $"© {DateTime.Today.Year} Insire",
                ReleaseNotes                = new[]{""},
                Tags                        = new[]{"MvvmScarletToolkit", "MVVM", "C#", "WPF", "Windows", "Csharp", "ScarletToolkit"},
                RequireLicenseAcceptance    = true,
                Symbols                     = true,
                NoPackageAnalysis           = false,
                BasePath                    = basePath,
                OutputDirectory             = new DirectoryPath(PackagePath),
                KeepTemporaryNuSpecFile     = false,
                Files = new[]
                {
                    new NuSpecContent{ Source="net46\\*",Target="lib\\net46"},
                    new NuSpecContent{ Source="net461\\*",Target="lib\\net461"},
                    new NuSpecContent{ Source="net462\\*",Target="lib\\net462"},
                    new NuSpecContent{ Source="net47\\*",Target="lib\\net47"},
                    new NuSpecContent{ Source="net471\\*",Target="lib\\net471"},
                    new NuSpecContent{ Source="netcoreapp3.0\\*",Target="lib\\netcoreapp3.0"},
                }
            };
        }
});

Task("PushLocally")
    .WithCriteria(() => BuildSystem.IsLocalBuild)
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
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Pack")
    .IsDependentOn("PushLocally");

RunTarget(Argument("target", "Default"));
