namespace MvvmScarletToolkit.Test.Tracing
{
    /// <summary>
    /// Replaces the base class of MvvmScarletToolkit.Test.Tracing for the linked test files. Without the TraceTest attribute,
    /// its OpenTelemetry tracing is built on xunit.v3.extensibility.core, which has no Native AOT version.
    /// </summary>
    public abstract class TraceTestBase;
}
