namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ScannedComponent
{
    public IEnumerable<string> LocationsFoundAt { get; set; } = null!;

    public TypedComponent.TypedComponent Component { get; set; } = null!;

    public string DetectorId { get; set; } = string.Empty;

    public bool? IsDevelopmentDependency { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public DependencyScope? DependencyScope { get; set; }

    public IEnumerable<TypedComponent.TypedComponent> TopLevelReferrers { get; set; } = null!;

    public IEnumerable<TypedComponent.TypedComponent> AncestralReferrers { get; set; } = null!;

    public IEnumerable<int> ContainerDetailIds { get; set; } = null!;

    public IDictionary<int, IEnumerable<int>> ContainerLayerIds { get; set; } = null!;

    public ISet<string> TargetFrameworks { get; set; } = null!;
}
