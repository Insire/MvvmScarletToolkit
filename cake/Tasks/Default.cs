using Cake.Frosting;

[Dependency(typeof(CleanSolution))]
[Dependency(typeof(UpdateAssemblyInfo))]
[Dependency(typeof(TestAndUploadReport))]
[Dependency(typeof(CleanSolutionAgain))]
[Dependency(typeof(BuildAndPack))]
[Dependency(typeof(PushRemote))]
[Dependency(typeof(PushLocally))]
public sealed class Default : FrostingTask<Context>
{
}
