using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNetCore.Test;
using Cake.Core;
using Cake.Frosting;
using System.Linq;

namespace Build
{
    public sealed class Test : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var projectFile = @"./src/MvvmScarletToolkit.Wpf.Tests/MvvmScarletToolkit.Wpf.Tests.csproj";
            var testSettings = new DotNetCoreTestSettings
            {
                ToolPath = context.Tools.Resolve("dotnet.exe"),
                Configuration = "Release",
                NoBuild = false,
                NoRestore = false,
                NoLogo = true,
                HandleExitCode = HandleExitCode,
                ArgumentCustomization = builder => builder
                    .Append("--results-directory:./Results/coverage")
                    .Append("-p:DebugType=full")
                    .Append("-p:DebugSymbols=true")
                    .AppendSwitchQuoted("--collect", ":", "\"\"Code Coverage\"\"")
                    .Append($"--logger:trx;"),
            };

            context.DotNetTest(projectFile, testSettings);
        }

        private static bool HandleExitCode(int code)
        {
            return true;
        }
    }
}
