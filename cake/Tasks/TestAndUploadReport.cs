using Cake.Frosting;

[Dependency(typeof(HtmlReport))]
[Dependency(typeof(UploadCodecovReport))]
public sealed class TestAndUploadReport : FrostingTask<BuildContext>
{
}
