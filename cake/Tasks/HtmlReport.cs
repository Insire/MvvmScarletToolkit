using Cake.Common.IO;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

[Dependency(typeof(ConvertCoverage))]
public sealed class HtmlReport : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.MergeReports("./Results/coverage/**/*.xml", ReportGeneratorReportType.Html, "html");
    }

    public override bool ShouldRun(BuildContext context)
    {
        return base.ShouldRun(context)
            && context.GetFiles("./Results/coverage/**/*.xml").Count > 0;
    }
}
