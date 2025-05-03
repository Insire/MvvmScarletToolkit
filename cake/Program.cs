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
                 .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.20.0"))
                 .InstallTool(new Uri("nuget:?package=ReportGenerator&version=5.4.5"))
                 .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.12.0"))
                 .InstallTool(new Uri("nuget:?package=nuget.commandline&version=6.13.2"))
                 .InstallTool(new Uri("nuget:?package=dotnet-coverage&version=17.14.2"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
