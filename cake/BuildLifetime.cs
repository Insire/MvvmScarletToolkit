using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Frosting;
using Cake.GitVersioning;

namespace Build
{
    public sealed class BuildLifetime : FrostingLifetime<BuildContext>
    {
        public override void Setup(BuildContext context, ISetupContext info)
        {
            context.GitVersion = context.GitVersioningGetVersion();
            context.Branch = context.GitVersion().BranchName;

            context.Information("Branch: {0}", context.Branch);

            if (context.IsPublicRelease && context.Branch == "master")
            {
                context.Information("Building a {0} release.", "public");
            }
            else
            {
                context.Information("Building a {0}release.", "pre-");
            }

            context.Information("Provider: {0}", context.BuildSystem().Provider);
            context.Information("Platform: {0} ({1})", context.Environment.Platform.Family, context.Environment.Platform.Is64Bit ? "x64" : "x86");

            context.Information("NUGETORG_APIKEY was {0} set.", string.IsNullOrEmpty(context.EnvironmentVariable("NUGETORG_APIKEY")) ? "not" : "");
            context.Information("GITHUB_APIKEY was {0} set.", string.IsNullOrEmpty(context.EnvironmentVariable("GITHUB_APIKEY")) ? "not" : "");
            context.Information("CODECOV_TOKEN was {0} set.", string.IsNullOrEmpty(context.EnvironmentVariable("CODECOV_TOKEN")) ? "not" : "");

            context.Information("reportsFolder: {0}", context.ReportsPath.FullPath);
            context.Information("coberturaResultFile: {0}", context.CoberturaResultFile.FullPath);

            context.Information("dotnet tool: {0}", context.Tools.Resolve("dotnet.exe"));
        }

        public override void Teardown(BuildContext context, ITeardownContext info)
        {
        }
    }
}
