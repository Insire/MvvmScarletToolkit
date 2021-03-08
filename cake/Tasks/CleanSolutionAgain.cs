using Cake.Frosting;
using Build;

public sealed class CleanSolutionAgain : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.Clean(true, true, true, false);
    }
}
