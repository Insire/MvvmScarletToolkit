using Cake.Frosting;

namespace Build.Tasks
{
    [IsDependentOn(typeof(HtmlReport))]
    [IsDependentOn(typeof(UploadCodecovReport))]
    public sealed class TestAndUploadReport : FrostingTask<BuildContext>;
}
