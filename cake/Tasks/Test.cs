using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Test;
using Cake.Core;
using Cake.Frosting;
using System.Collections.Generic;

namespace Build
{
    public sealed class Test : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var testSettings = new DotNetTestSettings
            {
                Configuration = BuildContext.BuildConfiguration,
                NoBuild = false,
                NoRestore = false,
                NoLogo = false,
                ResultsDirectory = context.CoveragePath,
                Loggers = new[] { "trx" },
                Collectors = new[] { "Code Coverage" },
                EnvironmentVariables = new Dictionary<string, string>() { ["Environment"] = "Test" },
            };

            // var files = context.GetFiles(context.SourcePath.FullPath + "/*/*.Tests.csproj");
            // foreach (var file in files)
            // {
            var slnFile = context.Environment.Platform.IsWindows() ? context.SolutionFile : context.XPlatSolutionFile;
                context.DotNetTest(slnFile.FullPath, testSettings);
            // }
        }
    }
}
