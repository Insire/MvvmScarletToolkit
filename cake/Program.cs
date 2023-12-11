using Cake.Frosting;
using System;

namespace Build
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new CakeHost()
                 .InstallTool(new Uri("nuget:?package=Codecov&version=1.13.0"))
                 .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.16.3"))
                 .InstallTool(new Uri("nuget:?package=ReportGenerator&version=5.2.0"))
                 .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.12.0"))
                 .InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=17.8.0"))
                 .InstallTool(new Uri("nuget:?package=nuget.commandline&version=6.8.0"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
