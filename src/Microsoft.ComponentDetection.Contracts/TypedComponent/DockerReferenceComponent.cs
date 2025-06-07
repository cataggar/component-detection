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
        this.Reference = reference;
        this.Repository = reference switch
        {
            CanonicalReference c => c.Repository,
            RepositoryReference r => r.Repository,
            TaggedReference t => t.Repository,
            DualReference d => d.Repository,
            _ => string.Empty,
        };
        this.Domain = reference switch
        {
            CanonicalReference c => c.Domain,
            RepositoryReference r => r.Domain,
            TaggedReference t => t.Domain,
            DualReference d => d.Domain,
            _ => string.Empty,
        };
        this.Tag = reference switch
        {
            TaggedReference t => t.Tag,
            DualReference d => d.Tag,
            _ => string.Empty,
        };
        this.Digest = reference switch
        {
            CanonicalReference c => c.Digest,
            DigestReference d => d.Digest,
            DualReference du => du.Digest,
            _ => string.Empty,
        };
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
    public DockerReference Reference { get; set; } = default!;

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
