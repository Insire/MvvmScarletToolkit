namespace MvvmScarletToolkit
{
    /// <summary>
    /// A column has one function. <see cref="DataGridSummaryBar.FunctionsProperty"/> combines them, like "Sum, Maximum".
    /// </summary>
    [Flags]
    public enum DataGridSummaryFunction
    {
        None = 0,
        Sum = 1,
        Average = 2,
        Minimum = 4,
        Maximum = 8,
        Count = 16,
    }
}
