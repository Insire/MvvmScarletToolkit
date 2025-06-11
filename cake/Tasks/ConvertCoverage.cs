using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build
{
    [IsDependentOn(typeof(Test))]
    public sealed class ConvertCoverage : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var dotnetExe = context.Tools.Resolve(context.DotnetExeName);
            var codeCoverageExe = context.Tools.Resolve("dotnet-coverage.dll");

            foreach (var file in context.GetFiles($"{context.ResultsPath.FullPath}/coverage/**/*.coverage"))
            {
                var result = System.IO.Path.ChangeExtension(file.FullPath, ".xml");

                var settings = new ProcessSettings()
                        .UseWorkingDirectory(context.ResultsPath)
                        .WithArguments(builder => builder
                            .AppendQuoted(codeCoverageExe.FullPath)
                            .Append("merge")
                            .Append("--remove-input-files")
                            .AppendSwitchQuoted(@"--output", " ", result)
                            .AppendSwitch("--output-format", "xml")
                            .Append(file.FullPath)
                        );

                context.StartProcess(dotnetExe.FullPath, settings);
            }
        }

        public override bool ShouldRun(BuildContext context)
        {
            return context.Tools.Resolve("dotnet-coverage.dll") != null;
        }
    }
}
