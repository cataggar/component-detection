namespace Microsoft.ComponentDetection.Contracts.TypedComponent;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ComponentDetection.Contracts.BcdeModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PackageUrl;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
[JsonConverter(typeof(TypedComponentConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class TypedComponent
{
    [JsonIgnore]
    private string? id;

    internal TypedComponent()
    {
        // Reserved for deserialization.
    }

    /// <summary>Gets the type of the component, must be well known.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public abstract ComponentType Type { get; }

    /// <summary>Gets the id of the component.</summary>
    public string Id => this.id ??= this.ComputeId();

    public virtual PackageURL PackageUrl { get; } = null!;

    [JsonIgnore]
    internal string DebuggerDisplay => $"{this.Id}";

    protected string ValidateRequiredInput(string input, string fieldName, string componentType)
    {
        return string.IsNullOrWhiteSpace(input)
            ? throw new ArgumentNullException(fieldName, this.NullPropertyExceptionMessage(fieldName, componentType))
            : input;
    }

    protected T ValidateRequiredInput<T>(T input, string fieldName, string componentType)
    {
        return EqualityComparer<T>.Default.Equals(input, default!) ? throw new ArgumentNullException(fieldName, this.NullPropertyExceptionMessage(fieldName, componentType)) : input;
    }

    protected string NullPropertyExceptionMessage(string propertyName, string componentType)
    {
        return $"Property {propertyName} of component type {componentType} is required";
    }

    protected abstract string ComputeId();
}
