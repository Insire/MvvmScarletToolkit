namespace MvvmScarletToolkit.Wpf
{
    internal sealed class HtmlTagTree : HtmlTagNode
    {
        public HtmlTagTree(IParamParser paramParser)
            : base(true, new HtmlTag(paramParser, "root", string.Empty))
        {
        }

        public override bool CanAdd(HtmlTag tag)
        {
            return true;
        }
    }
}
