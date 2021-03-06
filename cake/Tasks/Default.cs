using Cake.Frosting;

[Dependency(typeof(CleanSolution))]
[Dependency(typeof(UpdateAssemblyInfo))]
[Dependency(typeof(BuildAndPack))]
[Dependency(typeof(TestAndUploadReport))]
[Dependency(typeof(PushLocally))]
public sealed class Default : FrostingTask<Context>
{
}
