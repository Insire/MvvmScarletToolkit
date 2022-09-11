using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Linq;

namespace Build
{
    [IsDependentOn(typeof(Test))]
    public sealed class ConvertCoverage : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            foreach (var file in context.GetFiles($"{BuildContext.ResultsPath}/coverage/**/*.coverage"))
            {
                var codeCoverageExe = context.Tools.Resolve("CodeCoverage.exe");
                var result = System.IO.Path.ChangeExtension(file.FullPath, ".xml");

                var settings = new ProcessSettings()
                        .UseWorkingDirectory(BuildContext.ResultsPath)
                        .WithArguments(builder => builder
                            .Append("analyze")
                            .AppendSwitchQuoted(@"-output", ":", result)
                            .Append(file.FullPath)
                        );

                context.StartProcess(codeCoverageExe, settings);
            }
        }

        public override bool ShouldRun(BuildContext context)
        {
            return base.ShouldRun(context)
                && context.Tools.Resolve("CodeCoverage.exe") != null;
        }
    }
}
