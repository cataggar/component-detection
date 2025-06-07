namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ScanResult
{
    public IEnumerable<ScannedComponent> ComponentsFound { get; set; } = new List<ScannedComponent>();
    public IEnumerable<Detector> DetectorsInScan { get; set; } = new List<Detector>();
    public IEnumerable<Detector> DetectorsNotInScan { get; set; } = new List<Detector>();
    public Dictionary<int, ContainerDetails> ContainerDetailsMap { get; set; } = new Dictionary<int, ContainerDetails>();
    [JsonConverter(typeof(StringEnumConverter))]
    public ProcessingResultCode ResultCode { get; set; }
    public string SourceDirectory { get; set; } = string.Empty;
}
