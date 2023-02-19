using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Nerdbank.GitVersioning;
using System.Collections.Generic;
using System.Linq;

namespace Build
{
    public sealed class BuildContext : FrostingContext
    {
        public const string Platform = "AnyCPU";
        public const string BuildConfiguration = "Release";

        public DirectoryPath SourcePath { get; }
        public DirectoryPath PackagePath { get; }
        public DirectoryPath ResultsPath { get; }
        public DirectoryPath CoveragePath { get; }
        public DirectoryPath CoberturaResultsPath { get; }
        public DirectoryPath ReportsPath { get; }

        public FilePath SolutionFile { get; }
        public FilePath AssemblyInfoFile { get; }
        public FilePath CoberturaResultFile { get; }

        public VersionOracle GitVersion { get; internal set; }
        public string Branch { get; internal set; }

        public bool IsPublicRelease { get; internal set; }

        public List<(DirectoryPath Folder, FilePath ProjectFile)> NugetPackageProjects { get; }

        public BuildContext(ICakeContext context)
            : base(context)
        {
            var workingDirectory = context.Environment.WorkingDirectory;

            SourcePath = new DirectoryPath("src").MakeAbsolute(workingDirectory);
            ResultsPath = new DirectoryPath("results").MakeAbsolute(workingDirectory);
            PackagePath = new DirectoryPath("packages").MakeAbsolute(workingDirectory);

            CoveragePath = ResultsPath.Combine("coverage").MakeAbsolute(workingDirectory);
            ReportsPath = ResultsPath.Combine("reports").MakeAbsolute(workingDirectory);
            CoberturaResultsPath = ReportsPath.Combine("cobertura").MakeAbsolute(workingDirectory);

            NugetPackageProjects = new[]
            {
                (Folder: @"src\MvvmScarletToolkit", ProjectFile: "MvvmScarletToolkit.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Abstractions", ProjectFile: "MvvmScarletToolkit.Abstractions.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Commands", ProjectFile:  "MvvmScarletToolkit.Commands.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Observables", ProjectFile:  "MvvmScarletToolkit.Observables.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Wpf", ProjectFile:  "MvvmScarletToolkit.Wpf.csproj"),
                (Folder: @"src\MvvmScarletToolkit.Xamarin.Forms", ProjectFile:  "MvvmScarletToolkit.Xamarin.Forms.csproj"),
            }
            .Select(p =>
            {
                var folder = DirectoryPath.FromString(p.Folder).MakeAbsolute(workingDirectory);
                var projectFile = FilePath.FromString(p.ProjectFile).MakeAbsolute(folder);

                return (folder, projectFile);
            })
            .ToList();

            SolutionFile = FilePath.FromString("MvvmScarletToolkit.sln").MakeAbsolute(workingDirectory);
            AssemblyInfoFile = SourcePath.CombineWithFilePath("SharedAssemblyInfo.cs").MakeAbsolute(workingDirectory);
            CoberturaResultFile = CoberturaResultsPath.CombineWithFilePath("Cobertura.xml").MakeAbsolute(workingDirectory);

            IsPublicRelease = context.EnvironmentVariable("PublicRelease", false);
        }
    }
}
