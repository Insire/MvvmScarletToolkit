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
                 .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.12.0"))
                 .InstallTool(new Uri("nuget:?package=ReportGenerator&version=4.8.13"))
                 .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.7.0"))
                 .InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=16.11.0"))
                 .InstallTool(new Uri("nuget:?package=nuget.commandline&version=5.11.0"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
