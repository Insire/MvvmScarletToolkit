using Cake.Frosting;
using System;

public class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
             .UseContext<Context>()
             .UseWorkingDirectory("..")
             .InstallTool(new Uri("nuget:?package=Codecov&version=1.12.3"))
             .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.11.1"))
             .InstallTool(new Uri("nuget:?package=ReportGenerator&version=4.8.4"))
             .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.6.0"))
             .InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=16.8.3"))
             .InstallTool(new Uri("nuget:?package=nuget.commandline&version=5.8.1"))
             .Run(args);
    }
}
