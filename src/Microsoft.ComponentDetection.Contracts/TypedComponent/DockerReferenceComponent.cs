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
            CanonicalReference c => c.Repository ?? string.Empty,
            RepositoryReference r => r.Repository ?? string.Empty,
            TaggedReference t => t.Repository ?? string.Empty,
            DualReference d => d.Repository ?? string.Empty,
            _ => string.Empty,
        };

        this.Domain = reference switch
        {
            CanonicalReference c => c.Domain ?? string.Empty,
            RepositoryReference r => r.Domain ?? string.Empty,
            TaggedReference t => t.Domain ?? string.Empty,
            DualReference d => d.Domain ?? string.Empty,
            _ => string.Empty,
        };

        this.Tag = reference switch
        {
            TaggedReference t => t.Tag ?? string.Empty,
            DualReference d => d.Tag ?? string.Empty,
            _ => string.Empty,
        };

        this.Digest = reference switch
        {
            CanonicalReference c => c.Digest ?? string.Empty,
            DigestReference d => d.Digest ?? string.Empty,
            DualReference du => du.Digest ?? string.Empty,
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
            return DockerReference.CreateDockerReference(this.Repository ?? string.Empty, this.Domain ?? string.Empty, this.Digest ?? string.Empty, this.Tag ?? string.Empty);
        }
    }

    protected override string ComputeId() => $"{this.Repository} {this.Tag} {this.Digest}";
}
