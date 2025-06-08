namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

using System;

public class SpdxComponent : TypedComponent
{
    private SpdxComponent()
    {
        /* Reserved for deserialization */
        this.SpdxVersion = string.Empty;
        this.DocumentNamespace = new Uri("about:blank");
        this.Name = string.Empty;
        this.Checksum = string.Empty;
        this.RootElementId = string.Empty;
        this.Path = string.Empty;
    }

    public SpdxComponent(string spdxVersion, Uri documentNamespace, string name, string checksum, string rootElementId, string path)
    {
        this.SpdxVersion = this.ValidateRequiredInput(spdxVersion, nameof(this.SpdxVersion), nameof(ComponentType.Spdx));
        this.DocumentNamespace = this.ValidateRequiredInput(documentNamespace, nameof(this.DocumentNamespace), nameof(ComponentType.Spdx));
        this.Name = this.ValidateRequiredInput(name, nameof(this.Name), nameof(ComponentType.Spdx));
        this.Checksum = this.ValidateRequiredInput(checksum, nameof(this.Checksum), nameof(ComponentType.Spdx));
        this.RootElementId = this.ValidateRequiredInput(rootElementId, nameof(this.RootElementId), nameof(ComponentType.Spdx));
        this.Path = this.ValidateRequiredInput(path, nameof(this.Path), nameof(ComponentType.Spdx));
    }

    public override ComponentType Type => ComponentType.Spdx;

    public string RootElementId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SpdxVersion { get; set; } = string.Empty;

    public Uri DocumentNamespace { get; set; } = new Uri("about:blank");

    public string Checksum { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    protected override string ComputeId() => $"{this.Name}-{this.SpdxVersion}-{this.Checksum}";
}
