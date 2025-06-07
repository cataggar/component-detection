namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System.Collections.Generic;
using Microsoft.ComponentDetection.Contracts.TypedComponent;

public class LayerMappedLinuxComponents
{
    public IEnumerable<LinuxComponent> LinuxComponents { get; set; } = new List<LinuxComponent>();
    public DockerLayer DockerLayer { get; set; } = null!;
}
