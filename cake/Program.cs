using Cake.Frosting;

namespace Build
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new CakeHost()
                 .InstallTool(new Uri("nuget:?package=Codecov&version=1.13.0"))
                 .InstallTool(new Uri("dotnet:?package=gitversion.tool&version=6.4.0"))
                 .InstallTool(new Uri("dotnet:?package=dotnet-reportgenerator-globaltool&version=5.4.13"))
                 .UseContext<BuildContext>()
                 .UseLifetime<BuildLifetime>()
                 .UseWorkingDirectory("..")
                 .Run(args);
        }
    }
}
