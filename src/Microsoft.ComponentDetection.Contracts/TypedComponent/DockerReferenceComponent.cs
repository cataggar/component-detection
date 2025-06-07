namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

public class DockerReferenceComponent : TypedComponent
{
    public DockerReferenceComponent(string hash, string? repository = null, string? tag = null)
    {
        this.Digest = this.ValidateRequiredInput(hash, nameof(this.Digest), nameof(ComponentType.DockerReference));
        this.Repository = repository;
        this.Tag = tag;
    }

    public DockerReferenceComponent(DockerReference reference)
    {
        // TODO: Implement initialization from DockerReference if needed
        this.Repository = string.Empty;
        this.Digest = string.Empty;
        this.Tag = string.Empty;
        this.Domain = string.Empty;
    }

    private DockerReferenceComponent()
    {
        /* Reserved for deserialization */
        this.Repository = string.Empty;
        this.Digest = string.Empty;
        this.Tag = string.Empty;
        this.Domain = string.Empty;
    }

    public string? Repository { get; set; }
    public string Digest { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public string? Domain { get; set; }

    public override ComponentType Type => ComponentType.DockerReference;

    public DockerReference FullReference
    {
        get
        {
            return DockerReference.CreateDockerReference(this.Repository, this.Domain, this.Digest, this.Tag);
        }
    }

    protected override string ComputeId() => $"{this.Repository} {this.Tag} {this.Digest}";
}
