using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MvvmScarletToolkit.Test.Tracing.Otel.Fixtures
{
    public abstract class OpenTelemetryFixtureBase
    {
        /// <summary>
        /// Tracing is turned off on CI/CD agents, since there is no collector listening on localhost there,
        /// and when <c>OTEL_SDK_DISABLED</c> is set.
        /// </summary>
        public static bool IsEnabled { get; } = !IsSet("OTEL_SDK_DISABLED") && !IsRunningOnBuildServer();

        protected static TracerProvider? CreateTracerProvider(ResourceBuilder builder, string tracerName)
        {
            if (!IsEnabled)
            {
                return null;
            }

            return Sdk.CreateTracerProviderBuilder()
                 .SetResourceBuilder(builder)
                 .AddSource(tracerName)
                 .AddHttpClientInstrumentation()
                 .AddConsoleExporter()
                     .AddOtlpExporter(o =>
                     {
                         o.Endpoint = new Uri("http://localhost:4317");
                         o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                     })
                 .Build();
        }

        private static bool IsRunningOnBuildServer()
        {
            // CI: GitHub Actions, GitLab, AppVeyor and most others; TF_BUILD: Azure Pipelines
            return IsSet("CI") || IsSet("TF_BUILD") || IsSet("GITHUB_ACTIONS");
        }

        private static bool IsSet(string variable)
        {
            var value = Environment.GetEnvironmentVariable(variable);

            return !string.IsNullOrWhiteSpace(value)
                && !value.Equals("false", StringComparison.OrdinalIgnoreCase)
                && value != "0";
        }
    }
}
