namespace MvvmScarletToolkit.Commands
{
    public interface IBuilder<out TElement>
    {
        TElement Build();
    }
}
