using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

public sealed class PushGithub : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        foreach (var package in context.GetFiles(Context.PackagePath + "/*.nupkg"))
        {
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("nuget push")
                    .AppendQuoted(package.FullPath)

                    .AppendSwitchQuotedSecret("--api-key", context.EnvironmentVariable("GITHUB_APIKEY"))
                    .AppendSwitch("--source", "https://nuget.pkg.github.com/insire/index.json")
                    .Append("--skip-duplicate")
                );

            context.StartProcess("dotnet", settings);
        }
    }

    public override bool ShouldRun(Context context)
    {
        return base.ShouldRun(context)
            //&& (context.BuildSystem().IsRunningOnAzurePipelines || context.BuildSystem().IsRunningOnAzurePipelinesHosted)
            && !string.IsNullOrEmpty(context.EnvironmentVariable("GITHUB_APIKEY"))
            && !context.IsPublicRelease;
    }
}
