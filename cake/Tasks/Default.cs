using Cake.Frosting;

namespace Build
{
    [IsDependentOn(typeof(CleanSolution))]
    [IsDependentOn(typeof(UpdateAssemblyInfo))]
    [IsDependentOn(typeof(TestAndUploadReport))]
    [IsDependentOn(typeof(CleanSolutionAgain))]
    [IsDependentOn(typeof(BuildAndPack))]
    [IsDependentOn(typeof(PushNuget))]
    [IsDependentOn(typeof(PushGithub))]
    [IsDependentOn(typeof(PushLocally))]
    public sealed class Default : FrostingTask<BuildContext>;
}
