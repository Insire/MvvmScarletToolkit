using Cake.Frosting;

namespace Build
{
    public sealed class CleanSolutionAgain : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Clean(true, true, true, false);
        }
    }
}
