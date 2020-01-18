namespace MvvmScarletToolkit.Abstractions
{
    public interface IBuilder<out TElement>
    {
        TElement Build();
    }
}
