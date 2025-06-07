namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

using System;

public class GitComponent : TypedComponent
{
    public GitComponent(Uri repositoryUrl, string commitHash)
    {
        this.RepositoryUrl = this.ValidateRequiredInput(repositoryUrl, nameof(this.RepositoryUrl), nameof(ComponentType.Git));
        this.CommitHash = this.ValidateRequiredInput(commitHash, nameof(this.CommitHash), nameof(ComponentType.Git));
        this.Tag = string.Empty;
    }

    public GitComponent(Uri repositoryUrl, string commitHash, string tag)
        : this(repositoryUrl, commitHash) => this.Tag = tag;

    private GitComponent()
    {
        /* Reserved for deserialization */
        this.RepositoryUrl = new Uri("about:blank");
        this.CommitHash = string.Empty;
        this.Tag = string.Empty;
    }

    public Uri RepositoryUrl { get; set; } = new Uri("about:blank");

    public string CommitHash { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;

    public override ComponentType Type => ComponentType.Git;

    protected override string ComputeId() => $"{this.RepositoryUrl} : {this.CommitHash} - {this.Type}";
}
