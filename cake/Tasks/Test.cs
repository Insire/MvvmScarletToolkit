using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Test;
using Cake.Core;
using Cake.Frosting;
using System.Linq;

public sealed class Test : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var projectFile = @"./src/MvvmScarletToolkit.Tests/MvvmScarletToolkit.Tests.csproj";
        var testSettings = new DotNetCoreTestSettings
        {
            Framework = "netcoreapp3.1",
            Configuration = "Release",
            NoBuild = false,
            NoRestore = false,
            ArgumentCustomization = builder => builder
                .Append("--nologo")
                .Append("--results-directory:./Results/coverage")
                .Append("-p:DebugType=full")
                .Append("-p:DebugSymbols=true")
                .AppendSwitchQuoted("--collect", ":", "\"\"Code Coverage\"\"")
                .Append($"--logger:trx;LogFileName=..\\{context.VsTestResultsFile.FullPath};"),
        };

        context.DotNetCoreTest(projectFile, testSettings);
    }
}
