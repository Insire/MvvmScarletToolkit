using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Nerdbank.GitVersioning;

namespace Build
{
    public sealed class BuildContext : FrostingContext
    {
        public const string Platform = "AnyCPU";
        public const string BuildConfiguration = "Release";

        public string DotnetExeName { get; }

        public DirectoryPath SourcePath { get; }
        public DirectoryPath PackagePath { get; }
        public DirectoryPath ResultsPath { get; }
        public DirectoryPath CoveragePath { get; }
        public DirectoryPath CoberturaResultsPath { get; }
        public DirectoryPath ReportsPath { get; }

        public FilePath SolutionFile { get; }
        public FilePath XPlatSolutionFile { get; }
        public FilePath AssemblyInfoFile { get; }
        public FilePath CoberturaResultFile { get; }

        public VersionOracle? GitVersion { get; internal set; }
        public string? Branch { get; internal set; }

        public bool IsPublicRelease { get; internal set; }

        public List<(DirectoryPath Folder, FilePath ProjectFile)> NugetPackageProjects { get; }

        public BuildContext(ICakeContext context)
            : base(context)
        {
            DotnetExeName = Environment.Platform.IsWindows() ? "dotnet.exe" : "dotnet";
            var workingDirectory = context.Environment.WorkingDirectory;

            SourcePath = new DirectoryPath("src").MakeAbsolute(workingDirectory);
            ResultsPath = new DirectoryPath("results").MakeAbsolute(workingDirectory);
            PackagePath = new DirectoryPath("packages").MakeAbsolute(workingDirectory);

            CoveragePath = ResultsPath.Combine("coverage").MakeAbsolute(workingDirectory);
            ReportsPath = ResultsPath.Combine("reports").MakeAbsolute(workingDirectory);
            CoberturaResultsPath = ReportsPath.Combine("cobertura").MakeAbsolute(workingDirectory);

            NugetPackageProjects = (context.Environment.Platform.IsWindows()
                ? GetWindowsNugetPackageProjects()
                : GetXPlatNugetPackageProjects())
            .Select(p =>
            {
                var folder = DirectoryPath.FromString(p.Folder).MakeAbsolute(workingDirectory);
                var projectFile = FilePath.FromString(p.ProjectFile).MakeAbsolute(folder);

                return (folder, projectFile);
            })
            .ToList();

            XPlatSolutionFile = FilePath.FromString("MvvmScarletToolkit.XPlat.slnf").MakeAbsolute(workingDirectory);

            SolutionFile = FilePath.FromString("MvvmScarletToolkit.slnx").MakeAbsolute(workingDirectory);
            AssemblyInfoFile = SourcePath.CombineWithFilePath("SharedAssemblyInfo.cs").MakeAbsolute(workingDirectory);
            CoberturaResultFile = CoberturaResultsPath.CombineWithFilePath("Cobertura.xml").MakeAbsolute(workingDirectory);

            IsPublicRelease = context.EnvironmentVariable("PublicRelease", false);
        }

        private static IEnumerable<(string Folder, string ProjectFile)> GetWindowsNugetPackageProjects()
        {
            return
            [
                (Folder: @"src\MvvmScarletToolkit", ProjectFile: "MvvmScarletToolkit.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Abstractions", ProjectFile: "MvvmScarletToolkit.Abstractions.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Commands", ProjectFile:  "MvvmScarletToolkit.Commands.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Observables", ProjectFile:  "MvvmScarletToolkit.Observables.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Wpf", ProjectFile:  "MvvmScarletToolkit.Wpf.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Mediator", ProjectFile:  "MvvmScarletToolkit.Mediator.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Avalonia", ProjectFile:  "MvvmScarletToolkit.Avalonia.csproj"),
                (Folder: @"src\MvvmScarletToolkit.ImageLoading", ProjectFile:  "MvvmScarletToolkit.ImageLoading.csproj")
            ];
        }

        private static IEnumerable<(string Folder, string ProjectFile)> GetXPlatNugetPackageProjects()
        {
            return
            [
                (Folder: @"src\MvvmScarletToolkit", ProjectFile: "MvvmScarletToolkit.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Abstractions", ProjectFile: "MvvmScarletToolkit.Abstractions.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Commands", ProjectFile:  "MvvmScarletToolkit.Commands.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Observables", ProjectFile:  "MvvmScarletToolkit.Observables.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Mediator", ProjectFile:  "MvvmScarletToolkit.Mediator.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Avalonia", ProjectFile:  "MvvmScarletToolkit.Avalonia.csproj"),
                (Folder: @"src\MvvmScarletToolkit.ImageLoading", ProjectFile:  "MvvmScarletToolkit.ImageLoading.csproj")
            ];
        }

        internal static IEnumerable<(string Folder, string ProjectFile, string[] Frameworks)> GetTestProjects()
        {
            return
            [
                (Folder: @"tests\MvvmScarletToolkit.Mediator.Tests", ProjectFile: "MvvmScarletToolkit.Mediator.Tests.csproj",["net9.0"]),
                (Folder: @"tests\MvvmScarletToolkit.Observables.Tests", ProjectFile: "MvvmScarletToolkit.Observables.Tests.csproj",["net9.0"]),
                (Folder: @"tests\MvvmScarletToolkit.Wpf.Tests", ProjectFile:  "MvvmScarletToolkit.Wpf.Tests.csproj",["net8.0-windows", "net9.0-windows"]),
            ];
        }
    }
}
