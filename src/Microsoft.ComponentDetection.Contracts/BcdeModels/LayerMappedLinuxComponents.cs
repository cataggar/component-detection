namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Microsoft.ComponentDetection.Contracts.TypedComponent;

public class LayerMappedLinuxComponents
{
    /// <summary>
    /// Gets or sets the collection of Linux components.
    /// </summary>
    public IEnumerable<LinuxComponent> LinuxComponents { get; set; } = [];

    /// <summary>
    /// Gets or sets the Docker layer.
    /// </summary>
    public DockerLayer DockerLayer { get; set; } = null!;
}
