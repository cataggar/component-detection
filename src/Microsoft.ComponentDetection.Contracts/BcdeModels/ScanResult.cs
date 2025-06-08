namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ScanResult
{
    public IEnumerable<ScannedComponent> ComponentsFound { get; set; }

    public IEnumerable<Detector> DetectorsInScan { get; set; }

    public IEnumerable<Detector> DetectorsNotInScan { get; set; }

    public Dictionary<int, ContainerDetails> ContainerDetailsMap { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProcessingResultCode ResultCode { get; set; }

    public string SourceDirectory { get; set; }
}
