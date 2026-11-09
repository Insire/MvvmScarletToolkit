using Cake.Frosting;

namespace Build.Tasks
{
    /// <summary>
    /// The test half of <see cref="Default"/>, for the Test job of the CI pipeline: runs the tests, publishes their results and the coverage.
    /// </summary>
    [IsDependentOn(typeof(CleanSolution))]
    [IsDependentOn(typeof(UpdateAssemblyInfo))]
    [IsDependentOn(typeof(TestAndUploadReport))]
    public sealed class RunTests : FrostingTask<BuildContext>;
}
