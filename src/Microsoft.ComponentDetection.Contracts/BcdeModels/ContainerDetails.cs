namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// Details for a docker container
public class ContainerDetails
{
    [JsonPropertyName("imageId")]
    public string ImageId { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("digests")]
    public IEnumerable<string> Digests { get; set; }

    [JsonPropertyName("baseImageRef")]
    public string BaseImageRef { get; set; }

    [JsonPropertyName("baseImageDigest")]
    public string BaseImageDigest { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("tags")]
    public IEnumerable<string> Tags { get; set; }

    [JsonPropertyName("layers")]
    public IEnumerable<DockerLayer> Layers { get; set; }
}
