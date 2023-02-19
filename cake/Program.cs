using Cake.Frosting;
using System;

namespace Build
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new CakeHost()
                 .InstallTool(new Uri("nuget:?package=CodecovUploader&version=0.3.2"))
                 .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.16.2"))
                 .InstallTool(new Uri("nuget:?package=ReportGenerator&version=5.1.17"))
                 .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.12.0"))
                 .InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=17.4.1"))
                 .InstallTool(new Uri("nuget:?package=nuget.commandline&version=6.4.0"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
