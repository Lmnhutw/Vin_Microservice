using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Vin.BuildingBlocks.Diagnostics;

public static class ServiceTelemetry
{
    public const string SourceName = "Vin.Shop";

    public static readonly ActivitySource ActivitySource = new(SourceName);
    public static readonly Meter Meter = new(SourceName);
    public static readonly Counter<long> RequestCount = Meter.CreateCounter<long>("vin.http.server.requests");
    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>("vin.http.server.duration", "ms");
}
