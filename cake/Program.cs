using Cake.Frosting;
using System;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
             .InstallTool(new Uri("nuget:?package=Codecov&version=1.13.0"))
             .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.12.0"))
             .InstallTool(new Uri("nuget:?package=ReportGenerator&version=4.8.12"))
             .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.6.11"))
             .InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=16.10.0"))
             .InstallTool(new Uri("nuget:?package=nuget.commandline&version=5.8.1"))
             .UseContext<BuildContext>()
             .UseLifetime<BuildLifetime>()
             .UseWorkingDirectory("..")
             .Run(args);
    }
}
