namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ScannedComponent
{
    /// <summary>
    /// Gets or sets the locations where the component was found.
    /// </summary>
    public IEnumerable<string> LocationsFoundAt { get; set; } = [];

    /// <summary>
    /// Gets or sets the typed component.
    /// </summary>
    public TypedComponent.TypedComponent Component { get; set; } = null!;

    /// <summary>
    /// Gets or sets the detector ID.
    /// </summary>
    public string DetectorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the component is a development dependency.
    /// </summary>
    public bool? IsDevelopmentDependency { get; set; }

    /// <summary>
    /// Gets or sets the dependency scope.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public DependencyScope? DependencyScope { get; set; }

    /// <summary>
    /// Gets or sets the top-level referrers.
    /// </summary>
    public IEnumerable<TypedComponent.TypedComponent> TopLevelReferrers { get; set; } = [];

    /// <summary>
    /// Gets or sets the ancestral referrers.
    /// </summary>
    public IEnumerable<TypedComponent.TypedComponent> AncestralReferrers { get; set; } = [];

    /// <summary>
    /// Gets or sets the container detail IDs.
    /// </summary>
    public IEnumerable<int> ContainerDetailIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the container layer IDs.
    /// </summary>
    public IDictionary<int, IEnumerable<int>> ContainerLayerIds { get; set; } = new Dictionary<int, IEnumerable<int>>();

    /// <summary>
    /// Gets or sets the target frameworks.
    /// </summary>
    public ISet<string> TargetFrameworks { get; set; } = new HashSet<string>();
}
