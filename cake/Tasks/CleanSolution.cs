using Cake.Frosting;

namespace Build
{
    public sealed class CleanSolution : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Clean(true, true, true, true);
        }
    }
}
