using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Linq;

[Dependency(typeof(Test))]
public sealed class ConvertCoverage : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        foreach (var file in context.GetFiles($"{Context.ResultsPath}/coverage/**/*.coverage"))
        {
            var codeCoverageExe = context.Tools.Resolve("CodeCoverage.exe");
            var result = System.IO.Path.ChangeExtension(file.FullPath, ".xml");

            var settings = new ProcessSettings()
                    .UseWorkingDirectory(Context.ResultsPath)
                    .WithArguments(builder => builder
                        .Append("analyze")
                        .AppendSwitchQuoted(@"-output", ":", result)
                        .Append(file.FullPath)
                    );

            context.StartProcess(codeCoverageExe.FullPath, settings);
        }
    }

    public override bool ShouldRun(Context context)
    {
        return base.ShouldRun(context)
            && context.Tools.Resolve("CodeCoverage.exe") != null;
    }
}
