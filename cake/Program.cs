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
                 .InstallTool(new Uri("dotnet:?package=gitversion.tool&version=6.3.0"))
                 .InstallTool(new Uri("dotnet:?package=dotnet-reportgenerator-globaltool&version=5.4.7"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
