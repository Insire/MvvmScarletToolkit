using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

public class Context : FrostingContext
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
    public FilePath VsTestResultsFile { get; }
    public FilePath CodeCoverageBinaryFile { get; }
    public FilePath CodeCoverageResultsFile { get; }
    public bool IsPublicRelease { get; }

    public string[] NugetPackageProjects { get; }

    public Context(ICakeContext context)
        : base(context)
    {
        ReportsFolder = new DirectoryPath(ResultsPath).Combine("reports");
        CoberturaResultFile = new DirectoryPath(CoberturaResultsPath).CombineWithFilePath("Cobertura.xml");
        VsTestResultsFile = new FilePath("vsTestResults.trx");
        CodeCoverageBinaryFile = new FilePath("vsCodeCoverage.coverag");
        CodeCoverageResultsFile = new FilePath("vsCodeCoverage.xml");

        NugetPackageProjects = new[]
        {
            @".\src\MvvmScarletToolkit.Abstractions\MvvmScarletToolkit.Abstractions.csproj",
            @".\src\MvvmScarletToolkit\MvvmScarletToolkit.csproj",
            @".\src\MvvmScarletToolkit.Messenger\MvvmScarletToolkit.Messenger.csproj",
            @".\src\MvvmScarletToolkit.Commands\MvvmScarletToolkit.Commands.csproj",
            @".\src\MvvmScarletToolkit.Observables\MvvmScarletToolkit.Observables.csproj",
            @".\src\MvvmScarletToolkit.Wpf\MvvmScarletToolkit.Wpf.csproj",
            @".\src\MvvmScarletToolkit.Xamarin.Forms\MvvmScarletToolkit.Xamarin.Forms.csproj",
        };

        IsPublicRelease = context.EnvironmentVariable("PublicRelease", false);

        if (IsPublicRelease && this.GitVersion().BranchName == "master")
        {
            IsPublicRelease = true;
            this.Information("Building a public release.");
        }
        else
        {
            this.Information("Building a pre-release.");
        }

        this.Debug("IsLocalBuild: " + this.BuildSystem().IsLocalBuild);
        this.Debug("IsRunningOnAppVeyor: " + this.BuildSystem().IsRunningOnAppVeyor);
        this.Debug("IsRunningOnAzurePipelines: " + this.BuildSystem().IsRunningOnAzurePipelines);
        this.Debug("IsRunningOnAzurePipelinesHosted: " + this.BuildSystem().IsRunningOnAzurePipelinesHosted);

        this.Information("Provider: " + this.BuildSystem().Provider);

        foreach (var entry in this.EnvironmentVariables())
        {
            this.Debug(entry.Key + " " + entry.Value);
        }
    }
}
