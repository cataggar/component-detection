namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ScanResult
{
    /// <summary>
    /// Gets or sets the components found during the scan.
    /// </summary>
    public IEnumerable<ScannedComponent> ComponentsFound { get; set; } = new List<ScannedComponent>();

    /// <summary>
    /// Gets or sets the detectors that were part of the scan.
    /// </summary>
    public IEnumerable<Detector> DetectorsInScan { get; set; } = new List<Detector>();

    /// <summary>
    /// Gets or sets the detectors that were not part of the scan.
    /// </summary>
    public IEnumerable<Detector> DetectorsNotInScan { get; set; } = new List<Detector>();

    /// <summary>
    /// Gets or sets the mapping of container details.
    /// </summary>
    public Dictionary<int, ContainerDetails> ContainerDetailsMap { get; set; } = new Dictionary<int, ContainerDetails>();

    /// <summary>
    /// Gets or sets the result code of the processing.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public ProcessingResultCode ResultCode { get; set; }

    /// <summary>
    /// Gets or sets the source directory of the scan.
    /// </summary>
    public string SourceDirectory { get; set; } = string.Empty;
}
