using Cake.Common.IO;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build
{
    [Dependency(typeof(ConvertCoverage))]
    public sealed class CoberturaReport : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.MergeReports("./Results/coverage/**/*.xml", ReportGeneratorReportType.Cobertura, "cobertura");
        }

        public override bool ShouldRun(BuildContext context)
        {
            return base.ShouldRun(context)
                && context.GetFiles("./Results/coverage/**/*.xml").Count > 0;
        }
    }
}
