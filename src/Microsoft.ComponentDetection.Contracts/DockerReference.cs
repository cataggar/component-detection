namespace Microsoft.ComponentDetection.Contracts;

public enum DockerReferenceKind
{
    Canonical = 0,
    Repository = 1,
    Tagged = 2,
    Dual = 3,
    Digest = 4,
}

#pragma warning disable SA1402
public class DockerReference
{
    public virtual DockerReferenceKind Kind { get; }

    public static DockerReference CreateDockerReference(string repository, string domain, string digest, string tag)
    {
        if (!string.IsNullOrEmpty(repository) && string.IsNullOrEmpty(domain))
        {
            if (!string.IsNullOrEmpty(digest))
            {
                return new DigestReference
                {
                    Digest = digest,
                };
            }
            else
            {
                throw new System.InvalidOperationException("Repository name must have at least one component");
            }
        }
        else if (string.IsNullOrEmpty(tag))
        {
            if (!string.IsNullOrEmpty(digest))
            {
                return new CanonicalReference
                {
                    Domain = domain,
                    Repository = repository,
                    Digest = digest,
                };
            }
            else
            {
                return new RepositoryReference
                {
                    Domain = domain,
                    Repository = repository,
                };
            }
        }
        else if (string.IsNullOrEmpty(digest))
        {
            return new TaggedReference
            {
                Domain = domain,
                Repository = repository,
                Tag = tag,
            };
        }
        else
        {
            return new DualReference
            {
                Domain = domain,
                Repository = repository,
                Tag = tag,
                Digest = digest,
            };
        }
    }

    public virtual TypedComponent.DockerReferenceComponent ToTypedDockerReferenceComponent()
    {
        throw new System.NotImplementedException();
    }
}

public class Reference
{
    public string Tag { get; set; } = string.Empty;

    public string Digest { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;
}

// sha256:abc123...
public class DigestReference : DockerReference
{
    public DigestReference()
    {
        this.Digest = string.Empty;
    }

    public string Digest { get; set; }

    public override DockerReferenceKind Kind { get; } = DockerReferenceKind.Digest;

    public override string ToString()
    {
        return $"{this.Digest}";
    }

    public override TypedComponent.DockerReferenceComponent ToTypedDockerReferenceComponent()
    {
        return new TypedComponent.DockerReferenceComponent(this)
        {
            Digest = this.Digest,
        };
    }
}

// docker.io/library/ubuntu@sha256:abc123...
public class CanonicalReference : DockerReference
{
    public CanonicalReference()
    {
        this.Domain = string.Empty;
        this.Repository = string.Empty;
        this.Digest = string.Empty;
    }

    public string Domain { get; set; }

    public string Repository { get; set; }

    public string Digest { get; set; }

    public override DockerReferenceKind Kind { get; } = DockerReferenceKind.Canonical;

    public override string ToString()
    {
        return $"{this.Domain}/{this.Repository}@${this.Digest}";
    }

    public override TypedComponent.DockerReferenceComponent ToTypedDockerReferenceComponent()
    {
        return new TypedComponent.DockerReferenceComponent(this)
        {
            Domain = this.Domain,
            Digest = this.Digest,
            Repository = this.Repository,
        };
    }
}

// docker.io/library/ubuntu
public class RepositoryReference : DockerReference
{
    public RepositoryReference()
    {
        this.Domain = string.Empty;
        this.Repository = string.Empty;
    }

    public string Domain { get; set; }

    public string Repository { get; set; }

    public override DockerReferenceKind Kind { get; } = DockerReferenceKind.Repository;

    public override string ToString()
    {
        return $"{this.Repository}";
    }

    public override TypedComponent.DockerReferenceComponent ToTypedDockerReferenceComponent()
    {
        return new TypedComponent.DockerReferenceComponent(this)
        {
            Domain = this.Domain,
            Repository = this.Repository,
        };
    }
}

// docker.io/library/ubuntu:latest
public class TaggedReference : DockerReference
{
    public TaggedReference()
    {
        this.Domain = string.Empty;
        this.Repository = string.Empty;
        this.Tag = string.Empty;
    }

    public string Domain { get; set; }

    public string Repository { get; set; }

    public string Tag { get; set; }

    public override DockerReferenceKind Kind { get; } = DockerReferenceKind.Tagged;

    public override string ToString()
    {
        return $"{this.Domain}/{this.Repository}:${this.Tag}";
    }

    public override TypedComponent.DockerReferenceComponent ToTypedDockerReferenceComponent()
    {
        return new TypedComponent.DockerReferenceComponent(this)
        {
            Domain = this.Domain,
            Tag = this.Tag,
            Repository = this.Repository,
        };
    }
}

// docker.io/library/ubuntu:latest@sha256:abc123...
public class DualReference : DockerReference
{
    public DualReference()
    {
        this.Domain = string.Empty;
        this.Repository = string.Empty;
        this.Tag = string.Empty;
        this.Digest = string.Empty;
    }

    public string Domain { get; set; }

    public string Repository { get; set; }

    public string Tag { get; set; }

    public string Digest { get; set; }

    public override DockerReferenceKind Kind { get; } = DockerReferenceKind.Dual;

    public override string ToString()
    {
        return $"{this.Domain}/{this.Repository}:${this.Tag}@${this.Digest}";
    }

    public override TypedComponent.DockerReferenceComponent ToTypedDockerReferenceComponent()
    {
        return new TypedComponent.DockerReferenceComponent(this)
        {
            Domain = this.Domain,
            Digest = this.Digest,
            Tag = this.Tag,
            Repository = this.Repository,
        };
    }
}
