using Cake.Frosting;

namespace Build.Tasks
{
    /// <summary>
    /// Pushes the packages that <see cref="CreatePackages"/> wrote to <see cref="BuildContext.PackagePath"/>, for the Publish job of the CI pipeline.
    /// Doesn't clean or build anything, the packages come from the Pack job.
    /// </summary>
    [IsDependentOn(typeof(PushNuget))]
    [IsDependentOn(typeof(PushGithub))]
    public sealed class PushPackages : FrostingTask<BuildContext>;
}
