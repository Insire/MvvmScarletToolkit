using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Incubator.Project;
using System.Linq;

public sealed class CleanSolution : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var solution = context.ParseSolution(Context.SolutionPath);

        foreach (var project in solution.Projects)
        {
            // check solution items and exclude solution folders, since they are virtual
            if (project.Type == "{2150E333-8FDC-42A3-9474-1A3956D46DE8}")
                continue;

            var customProject = context.ParseProject(project.Path, configuration: Context.BuildConfiguration, platform: Context.Platform);

            foreach (var path in customProject.OutputPaths)
            {
                if (path is null)
                {
                    continue;
                }

                context.CleanDirectory(path.FullPath);
            }
        }

        var folders = new[]
        {
            new DirectoryPath(Context.PackagePath),
            new DirectoryPath(Context.ResultsPath),
        };

        foreach (var folder in folders)
        {
            context.EnsureDirectoryExists(folder);
            context.CleanDirectory(folder, (file) => !file.Path.Segments.Last().Contains(".gitignore"));
        }
    }
}
