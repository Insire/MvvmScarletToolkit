using Build;
using Cake.Common.IO;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

[Dependency(typeof(ConvertCoverage))]
public sealed class CoberturaReport : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.MergeReports("./Results/coverage/**/*.xml", ReportGeneratorReportType.Cobertura, "cobertura");
    }

    public override bool ShouldRun(Context context)
    {
        return base.ShouldRun(context)
            && context.GetFiles("./Results/coverage/**/*.xml").Count > 0;
    }
}
