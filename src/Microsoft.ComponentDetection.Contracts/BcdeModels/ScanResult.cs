namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ScanResult
{
    public IEnumerable<ScannedComponent> ComponentsFound { get; set; } = null!;

    public IEnumerable<Detector> DetectorsInScan { get; set; } = null!;

    public IEnumerable<Detector> DetectorsNotInScan { get; set; } = null!;

    public Dictionary<int, ContainerDetails> ContainerDetailsMap { get; set; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public ProcessingResultCode ResultCode { get; set; }

    public string SourceDirectory { get; set; } = string.Empty;
}
