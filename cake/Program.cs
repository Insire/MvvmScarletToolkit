using Cake.Frosting;
using System;

namespace Build
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new CakeHost()
                 .InstallTool(new Uri("nuget:?package=CodecovUploader&version=0.2.4"))
                 .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.15.2"))
                 .InstallTool(new Uri("nuget:?package=ReportGenerator&version=5.1.10"))
                 .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.10.3"))
                 .InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=17.3.1"))
                 .InstallTool(new Uri("nuget:?package=nuget.commandline&version=6.2.1"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
