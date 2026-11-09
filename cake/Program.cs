using Cake.Frosting;

namespace Build
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var versions = ToolVersions.Load();

            return new CakeHost()
                 .InstallTool(new Uri($"nuget:?package=Codecov&version={versions.Package("Codecov")}"))
                 .InstallTool(new Uri($"dotnet:?package=gitversion.tool&version={versions.DotNetTool("gitversion.tool")}"))
                 .InstallTool(new Uri($"dotnet:?package=dotnet-reportgenerator-globaltool&version={versions.DotNetTool("dotnet-reportgenerator-globaltool")}"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
