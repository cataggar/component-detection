namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

using System.Collections.Generic;
using PackageUrl;

public class PodComponent : TypedComponent
{
    private PodComponent()
    {
        /* Reserved for deserialization */
        this.Name = string.Empty;
        this.Version = string.Empty;
        this.SpecRepo = string.Empty;
    }

    public PodComponent(string name, string version, string specRepo = "")
    {
        this.Name = this.ValidateRequiredInput(name, nameof(this.Name), nameof(ComponentType.Pod));
        this.Version = this.ValidateRequiredInput(version, nameof(this.Version), nameof(ComponentType.Pod));
        this.SpecRepo = specRepo;
    }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string SpecRepo { get; set; } = string.Empty;

    public override ComponentType Type => ComponentType.Pod;

    public override PackageURL PackageUrl
    {
        get
        {
            var qualifiers = new SortedDictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(this.SpecRepo))
            {
                qualifiers.Add("repository_url", this.SpecRepo);
            }

            return new PackageURL("cocoapods", null, this.Name, this.Version, qualifiers, null);
        }
    }

    protected override string ComputeId() => $"{this.Name} {this.Version} - {this.Type}";
}
