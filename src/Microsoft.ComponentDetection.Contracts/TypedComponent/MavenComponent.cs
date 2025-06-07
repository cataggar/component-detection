namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

using PackageUrl;

public class MavenComponent : TypedComponent
{
    public MavenComponent(string groupId, string artifactId, string version)
    {
        this.GroupId = this.ValidateRequiredInput(groupId, nameof(this.GroupId), nameof(ComponentType.Maven));
        this.ArtifactId = this.ValidateRequiredInput(artifactId, nameof(this.ArtifactId), nameof(ComponentType.Maven));
        this.Version = this.ValidateRequiredInput(version, nameof(this.Version), nameof(ComponentType.Maven));
    }

    private MavenComponent()
    {
        /* Reserved for deserialization */
        this.GroupId = string.Empty;
        this.ArtifactId = string.Empty;
        this.Version = string.Empty;
    }

    public string GroupId { get; set; } = string.Empty;

    public string ArtifactId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public override ComponentType Type => ComponentType.Maven;

    public override PackageURL PackageUrl => new PackageURL("maven", this.GroupId, this.ArtifactId, this.Version, null, null);

    protected override string ComputeId() => $"{this.GroupId} {this.ArtifactId} {this.Version} - {this.Type}";
}
