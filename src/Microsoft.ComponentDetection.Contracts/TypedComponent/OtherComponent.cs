namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

using System;

public class OtherComponent : TypedComponent
{
    private OtherComponent()
    {
        /* Reserved for deserialization */
        this.Name = string.Empty;
        this.Version = string.Empty;
        this.DownloadUrl = new Uri("about:blank");
        this.Hash = string.Empty;
    }

    public OtherComponent(string name, string version, Uri downloadUrl, string hash)
    {
        this.Name = this.ValidateRequiredInput(name, nameof(this.Name), nameof(ComponentType.Other));
        this.Version = this.ValidateRequiredInput(version, nameof(this.Version), nameof(ComponentType.Other));
        this.DownloadUrl = this.ValidateRequiredInput(downloadUrl, nameof(this.DownloadUrl), nameof(ComponentType.Other));
        this.Hash = hash;
    }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public Uri DownloadUrl { get; set; } = new Uri("about:blank");

    public string Hash { get; set; } = string.Empty;

    public override ComponentType Type => ComponentType.Other;

    protected override string ComputeId() => $"{this.Name} {this.Version} {this.DownloadUrl} - {this.Type}";
}
