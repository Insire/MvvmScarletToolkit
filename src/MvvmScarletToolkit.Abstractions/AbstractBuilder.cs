namespace MvvmScarletToolkit
{
    public abstract class AbstractBuilder<TElement> : IBuilder<TElement>
    {
        public static implicit operator TElement(AbstractBuilder<TElement> @this)
        {
            return @this.Build();
        }

        public abstract TElement Build();
    }
}
