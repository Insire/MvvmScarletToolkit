using Cake.Frosting;

namespace Build.Tasks
{
    public sealed class CleanSolutionAgain : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Clean(true, true, true, false);
        }
    }
}
