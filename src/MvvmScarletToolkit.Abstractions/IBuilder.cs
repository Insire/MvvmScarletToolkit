namespace MvvmScarletToolkit
{
    public interface IBuilder<out TElement>
    {
        TElement Build();
    }
}
