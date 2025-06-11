using Cake.Common.IO;
using Cake.Common.Tools.ReportGenerator;
using Cake.Frosting;

namespace Build
{
    [IsDependentOn(typeof(ConvertCoverage))]
    public sealed class CoberturaReport : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.MergeReports("./results/coverage/**/*.xml", ReportGeneratorReportType.Cobertura, "cobertura");
        }

        public override bool ShouldRun(BuildContext context)
        {
            var files = context.GetFiles("./results/coverage/**/*.xml");
            return files.Count > 0;
        }
    }
}
