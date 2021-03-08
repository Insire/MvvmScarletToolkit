using Build;
using Cake.Frosting;

public sealed class CleanSolution : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.Clean(true, true, true, true);
    }
}
