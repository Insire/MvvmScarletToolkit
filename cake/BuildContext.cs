using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Nerdbank.GitVersioning;

public class BuildContext : FrostingContext
{
    public const string Platform = "AnyCPU";
    public const string BuildConfiguration = "Release";

    public const string SolutionPath = "./MvvmScarletToolkit.sln";
    public const string AssemblyInfoPath = "./src/SharedAssemblyInfo.cs";
    public const string PackagePath = "./packages";
    public const string ResultsPath = "./results";
    public const string CoberturaResultsPath = "./results/reports/cobertura";
    public const string LocalNugetDirectoryPath = @"D:\Drop\NuGet";

    public DirectoryPath ReportsFolder { get; }
    public FilePath CoberturaResultFile { get; }

    public VersionOracle GitVersion { get; internal set; }
    public string Branch { get; internal set; }

    public bool IsPublicRelease { get; internal set; }

    public string[] NugetPackageProjects { get; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        ReportsFolder = new DirectoryPath(ResultsPath).Combine("reports").MakeAbsolute(context.Environment.WorkingDirectory);
        CoberturaResultFile = new DirectoryPath(CoberturaResultsPath).CombineWithFilePath("Cobertura.xml").MakeAbsolute(context.Environment.WorkingDirectory);

        NugetPackageProjects = new[]
        {
            @".\src\MvvmScarletToolkit\MvvmScarletToolkit.csproj",
            @".\src\MvvmScarletToolkit.Wpf\MvvmScarletToolkit.Wpf.csproj",
            @".\src\MvvmScarletToolkit.Xamarin.Forms\MvvmScarletToolkit.Xamarin.Forms.csproj",
        };

        IsPublicRelease = context.EnvironmentVariable("PublicRelease", false);
    }
}
