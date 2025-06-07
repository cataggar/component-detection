namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Represents a detector with its properties.
/// </summary>
[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Detector
{
    /// <summary>
    /// Gets or sets the unique identifier for the detector.
    /// </summary>
    public string DetectorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the detector is experimental.
    /// </summary>
    public bool IsExperimental { get; set; }

    /// <summary>
    /// Gets or sets the version of the detector.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the supported component types for the detector.
    /// </summary>
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public IEnumerable<ComponentType> SupportedComponentTypes { get; set; } = new List<ComponentType>();
}
