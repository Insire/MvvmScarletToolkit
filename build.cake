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
   foreach(var item in VSWhereAll())
      Verbose(item.FullPath);

   var tools = new List<string>()
   {
      @".\Common7\IDE\Extensions\TestPlatform\vstest.console.exe",
      @".\Common7\IDE\MSTest.exe",
      @".\MSBuild\15.0\Bin\MSBuild.exe",
      @".\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe",
   };

   // source https://docs.microsoft.com/en-us/visualstudio/install/workload-and-component-ids
   var products = new List<string>
   {
      "Microsoft.VisualStudio.Product.Enterprise",
      "Microsoft.VisualStudio.Product.Professional",
      "Microsoft.VisualStudio.Product.Community",
      "Microsoft.VisualStudio.Product.TeamExplorer",
      "Microsoft.VisualStudio.Product.BuildTools",
      "Microsoft.VisualStudio.Product.TestAgent",
      "Microsoft.VisualStudio.Product.TestController",
      "Microsoft.VisualStudio.Product.TestProfessional",
      "Microsoft.VisualStudio.Product.FeedbackClient",
   };

   var foundMSBuild = false;
   var foundVSTest = false;

   foreach(var product in products)
   {
      foreach(var directory in VSWhereProducts(product))
      {
            foreach(var tool in tools)
            {
               var toolPath = directory.CombineWithFilePath(tool);
               if(FileExists(toolPath))
               {
                  Context.Tools.RegisterFile(toolPath);
                  if(tool.Contains("MSBuild.exe"))
                        foundMSBuild = true;
                  if(tool.Contains("vstest.console.exe"))
                        foundVSTest = true;
               }
            }
      }
   }

   if(!foundMSBuild)
      Warning("MSBuild not found");

   if(!foundVSTest)
      Warning("VSTest not found");

   if(foundMSBuild && foundVSTest)
      Information("Required tools have been found.");

   if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
   {
      var appveyorRepoTag = EnvironmentVariable("APPVEYOR_REPO_TAG") ;
      Information($"APPVEYOR_REPO_TAG: {appveyorRepoTag}");
   }

   var folders = new[]
   {
      new DirectoryPath(PackagePath),
      new DirectoryPath(ArchivePath),
   };

   foreach(var folder in folders)
   {
      EnsureDirectoryExists(folder);
      CleanDirectory(folder);
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
});

Task("RestoreNugetPackages")
    .Does(() =>
    {
        var settings = new NuGetRestoreSettings()
        {
            DisableParallelProcessing = false,
            Verbosity = NuGetVerbosity.Quiet,
            NoCache = false,
        };

        NuGetRestore(SolutionPath, settings);
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

        var settings = new MSBuildSettings()
        {
            Verbosity = Verbosity.Quiet,
        };

        if(msBuildPath != null)
            settings.ToolPath = msBuildPath;
        else
            settings.ToolVersion = MSBuildToolVersion.VS2017;

        settings.SetConfiguration(Configuration)
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

         var settings = new NuGetPackSettings()
         {
            Id                          = "MvvmScarletToolkit",
            Version                     = version,
            Authors                     = new[] {"Insire"},
            Owners                      = new[] {"Insire"},
            Description                 = $"MvvmScarletToolkit v{version}",
            Summary                     = "MvvmScarletToolkit is a personal project and framework to speed up the development process of WPF applications.",
            ProjectUrl                  = new Uri(@"https://github.com/Insire/MvvmScarletToolkit"),
            // IconUrl                      = new Uri(new FilePath("..\\src\\Resources\\Images\\logo.ico").MakeAbsolute(Context.Environment).FullPath, UriKind.Absolute),
            LicenseUrl                  = new Uri(@"https://github.com/Insire/MvvmScarletToolkit/blob/master/LICENSE.md"),
            Copyright                   = $"© {DateTime.Today.Year} Insire",
            ReleaseNotes                = new[]{""},
            Tags                        = new[]{"MvvmScarletToolkit", "MVVM", "C#", "WPF", "Windows", "Csharp", "ScarletToolkit"},
            RequireLicenseAcceptance    = true,
            Symbols                     = true,
            NoPackageAnalysis           = false,
            Files                       = new[]
            {
                  new NuSpecContent{ Source="*",Target="lib\\net471"},
            },
            BasePath                    = new DirectoryPath(".\\MvvmScarletToolkit\\bin\\Release\\"),
            OutputDirectory             = new DirectoryPath(PackagePath),
            KeepTemporaryNuSpecFile     = false,
         };

         NuGetPack(settings);
});

Task("Default")
   .IsDependentOn("CleanSolution")
   .IsDependentOn("RestoreNugetPackages")
   .IsDependentOn("UpdateAssemblyInfo")
   .IsDependentOn("Build")
   .IsDependentOn("Pack")
   .Does(() => 
   {
      Information("Hello Cake!");
   });

RunTarget(target);
