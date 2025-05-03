using Cake.Frosting;

namespace Build
{
    [IsDependentOn(typeof(HtmlReport))]
    [IsDependentOn(typeof(UploadCodecovReport))]
    public sealed class TestAndUploadReport : FrostingTask<BuildContext>;
}
