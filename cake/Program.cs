using Cake.Frosting;

public class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
             .UseContext<Context>()
             .UseWorkingDirectory("..")
             .Run(args);
    }
}
