using Cake.Frosting;

namespace Build.Tasks
{
    /// <summary>
    /// The packing half of <see cref="Default"/>, for the Pack job of the CI pipeline. Runs in parallel to <see cref="RunTests"/>,
    /// the pipeline only pushes the packages with <see cref="PushPackages"/> once both succeeded.
    /// </summary>
    [IsDependentOn(typeof(CleanSolution))]
    [IsDependentOn(typeof(UpdateAssemblyInfo))]
    [IsDependentOn(typeof(BuildAndPack))]
    [IsDependentOn(typeof(ValidateSymbols))]
    public sealed class CreatePackages : FrostingTask<BuildContext>;
}
