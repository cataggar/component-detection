namespace Microsoft.ComponentDetection.Contracts.BcdeModels;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.ComponentDetection.Contracts.TypedComponent;

public class TypedComponentConverter : JsonConverter<TypedComponent>
{
    private readonly Dictionary<ComponentType, Type> componentTypesToTypes = new()
    {
        { ComponentType.Other, typeof(OtherComponent) },
        { ComponentType.NuGet, typeof(NuGetComponent) },
        { ComponentType.Npm, typeof(NpmComponent) },
        { ComponentType.Maven, typeof(MavenComponent) },
        { ComponentType.Git, typeof(GitComponent) },
        { ComponentType.RubyGems, typeof(RubyGemsComponent) },
        { ComponentType.Cargo, typeof(CargoComponent) },
        { ComponentType.Conan, typeof(ConanComponent) },
        { ComponentType.Pip, typeof(PipComponent) },
        { ComponentType.Go, typeof(GoComponent) },
        { ComponentType.DockerImage, typeof(DockerImageComponent) },
        { ComponentType.Pod, typeof(PodComponent) },
        { ComponentType.Linux, typeof(LinuxComponent) },
        { ComponentType.Conda, typeof(CondaComponent) },
        { ComponentType.DockerReference, typeof(DockerReferenceComponent) },
        { ComponentType.Vcpkg, typeof(VcpkgComponent) },
        { ComponentType.Spdx, typeof(SpdxComponent) },
        { ComponentType.DotNet, typeof(DotNetComponent) },
    };

    public override TypedComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        var typeString = root.GetProperty("type").GetString();
        var value = (ComponentType)Enum.Parse(typeof(ComponentType), typeString);
        var targetType = this.componentTypesToTypes[value];
        var json = root.GetRawText();
        return (TypedComponent)JsonSerializer.Deserialize(json, targetType, options);
    }

    public override void Write(Utf8JsonWriter writer, TypedComponent value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
