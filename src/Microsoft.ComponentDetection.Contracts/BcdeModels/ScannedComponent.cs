namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ScannedComponent
{
    public IEnumerable<string> LocationsFoundAt { get; set; }

    public TypedComponent.TypedComponent Component { get; set; }

    public string DetectorId { get; set; }

    public bool? IsDevelopmentDependency { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DependencyScope? DependencyScope { get; set; }

    public IEnumerable<TypedComponent.TypedComponent> TopLevelReferrers { get; set; }

    public IEnumerable<TypedComponent.TypedComponent> AncestralReferrers { get; set; }

    public IEnumerable<int> ContainerDetailIds { get; set; }

    public IDictionary<int, IEnumerable<int>> ContainerLayerIds { get; set; }

    public ISet<string> TargetFrameworks { get; set; }
}
