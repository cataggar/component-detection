namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

public class DockerImageComponent : TypedComponent
{
    private DockerImageComponent()
    {
        /* Reserved for deserialization */
        this.Name = string.Empty;
        this.Digest = string.Empty;
        this.Tag = string.Empty;
    }

    public DockerImageComponent(string hash, string? name = null, string? tag = null)
    {
        this.Digest = this.ValidateRequiredInput(hash, nameof(this.Digest), nameof(ComponentType.DockerImage));
        this.Name = name ?? string.Empty;
        this.Tag = tag ?? string.Empty;
    }

    public string? Name { get; set; } = string.Empty;
    public string Digest { get; set; } = string.Empty;
    public string? Tag { get; set; } = string.Empty;

    public override ComponentType Type => ComponentType.DockerImage;

    protected override string ComputeId() => $"{this.Name} {this.Tag} {this.Digest}";
}
