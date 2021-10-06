using Cake.Frosting;

namespace Build
{
    [Dependency(typeof(HtmlReport))]
    [Dependency(typeof(UploadCodecovReport))]
    public sealed class TestAndUploadReport : FrostingTask<BuildContext>
    {
    }
}
