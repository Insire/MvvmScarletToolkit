using Cake.Frosting;

namespace Build.Tasks
{
    public sealed class CleanSolution : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Clean(true, true, true, true);
        }
    }
}
