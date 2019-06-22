#addin nuget:?package=Cake.Incubator

#tool nuget:?package=vswhere

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var Configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// Constants
//////////////////////////////////////////////////////////////////////
const string Platform = "AnyCPU";

const string SolutionPath =".\\MvvmScarletToolkit.sln";
const string AssemblyInfoPath =".\\SharedAssemblyInfo.cs";
const string PackagePath = ".\\Package";
const string ArchivePath = ".\\Archive";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(ctx =>
{
    var latestInstallationPath = VSWhereLatest(new VSWhereLatestSettings { IncludePrerelease = true });
    var msBuildPath = latestInstallationPath.Combine("./MSBuild/Current/Bin");
    var msBuildPathExe = msBuildPath.CombineWithFilePath("./MSBuild.exe");
    Context.Tools.RegisterFile(msBuildPathExe);

    if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    {
        var appveyorRepoTag = EnvironmentVariable("APPVEYOR_REPO_TAG") ;
        Information($"APPVEYOR_REPO_TAG: {appveyorRepoTag}");
    }
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

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
            new DirectoryPath(ArchivePath),
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

            // invalid entries:
            // Configuration           = assemblyInfoParseResult.Configuration,
            // Description             = assemblyInfoParseResult.Description,
            // Guid                    = assemblyInfoParseResult.Guid,
            // Title                   = assemblyInfoParseResult.Title,

            // posssible missing entries
            // ComVisible           = assemblyInfoParseResult.ComVisible,
            // CustomAttributes     = assemblyInfoParseResult.CustomAttributes,
            // CLSCompliant         = assemblyInfoParseResult.CLSCompliant,

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

        if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
        {
            var version = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "no version found from AppVeyorEnvironment";

            Information($"Version: {version}");

            settings.Version                 = version;
            settings.FileVersion             = version;
            settings.InformationalVersion    = version;
        }

        CreateAssemblyInfo(new FilePath(AssemblyInfoPath), settings);
});

Task("Build")
    .Does(() =>
    {
        var msBuildPath = Context.Tools.Resolve("msbuild.exe");
        var settings = new MSBuildSettings
        {
                Verbosity = Verbosity.Quiet,
                ToolPath = msBuildPath,
                Configuration = Configuration,
                ArgumentCustomization = args => args.Append("/m").Append("/nr:false") // The /nr switch tells msbuild to quite once it’s done
        };

        MSBuild(SolutionPath, settings.WithTarget("restore"));

        settings = new MSBuildSettings()
        {
            Verbosity = Verbosity.Quiet,
            Restore = true,
            NodeReuse = false,
            ToolPath = msBuildPath
        };

        settings = settings
                .SetConfiguration(Configuration)
                .SetDetailedSummary(false)
                .SetMaxCpuCount(0)
                .SetMSBuildPlatform(MSBuildPlatform.Automatic);

        MSBuild(SolutionPath, settings);
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
                // IconUrl                      = new Uri(new FilePath("..\\src\\Resources\\Images\\logo.ico").MakeAbsolute(Context.Environment).FullPath, UriKind.Absolute),
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
    .DoesForEach(GetFiles(PackagePath + "\\*.nupkg"), path =>
    {
        var settings = new ProcessSettings()
            .UseWorkingDirectory(".")
            .WithArguments(builder => builder
            .Append("push")
            .AppendSwitchQuoted("-source",@"D:\Drop\NuGet")
            .AppendQuoted(path.FullPath));

        StartProcess(".\\tools\\nuget.exe",settings);
    });

Task("Default")
    .IsDependentOn("CleanSolution")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("Build")
    .IsDependentOn("Pack")
    .IsDependentOn("PushLocally");

RunTarget(target);
