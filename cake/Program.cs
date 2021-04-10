using Cake.Frosting;
using System;

public class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
             .UseContext<Context>()
             .UseWorkingDirectory("..")
             .InstallTool(new Uri("nuget:?package=Codecov&version=1.13.0"))
             .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.12.0"))
             .InstallTool(new Uri("nuget:?package=ReportGenerator&version=4.8.7"))
             .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.6.8"))
             .InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=16.9.4"))
             .InstallTool(new Uri("nuget:?package=nuget.commandline&version=5.8.1"))
             .Run(args);
    }
}
