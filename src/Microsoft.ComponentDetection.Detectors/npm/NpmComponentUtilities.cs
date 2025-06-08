namespace Microsoft.ComponentDetection.Detectors.Npm;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using global::NuGet.Versioning;
using Microsoft.ComponentDetection.Contracts;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Microsoft.Extensions.Logging;

public static class NpmComponentUtilities
{
    private static readonly Regex UnsafeCharactersRegex = new Regex(
        @"[?<>#%{}|`'^\\~\[\]""\s\x7f]|[\x00-\x1f]|[\x80-\xff]",
        RegexOptions.Compiled);

    public static readonly string NodeModules = "node_modules";
    public static readonly string LockFile3EnvFlag = "CD_LOCKFILE_V3_ENABLED";

    public static void TraverseAndRecordComponents(JsonNode currentDependency, ISingleFileComponentRecorder singleFileComponentRecorder, TypedComponent component, TypedComponent explicitReferencedDependency, string parentComponentId = null)
    {
        var isDevDependency = currentDependency?["dev"]?.GetValue<bool?>() ?? false;
        AddOrUpdateDetectedComponent(singleFileComponentRecorder, component, isDevDependency, parentComponentId, isExplicitReferencedDependency: string.Equals(component.Id, explicitReferencedDependency.Id));
    }

    public static DetectedComponent AddOrUpdateDetectedComponent(
        ISingleFileComponentRecorder singleFileComponentRecorder,
        TypedComponent component,
        bool isDevDependency,
        string parentComponentId = null,
        bool isExplicitReferencedDependency = false)
    {
        var newComponent = new DetectedComponent(component);
        singleFileComponentRecorder.RegisterUsage(newComponent, isExplicitReferencedDependency, parentComponentId, isDevDependency);
        return singleFileComponentRecorder.GetComponent(component.Id);
    }

    public static TypedComponent GetTypedComponent(JsonNode node, string npmRegistryHost, ILogger logger)
    {
        if (node is null)
        {
            return null;
        }

        var name = node["name"]?.GetValue<string>() ?? string.Empty;
        var version = node["version"]?.GetValue<string>();
        var hash = node["integrity"]?.GetValue<string>();

        if (!IsPackageNameValid(name))
        {
            logger.LogInformation("The package name {PackageName} is invalid or unsupported and a component will not be recorded.", name);
            return null;
        }

        if (!SemanticVersion.TryParse(version, out var result) && !TryParseNpmVersion(npmRegistryHost, name, version, out result))
        {
            logger.LogInformation("Version string {ComponentVersion} for component {ComponentName} is invalid or unsupported and a component will not be recorded.", version, name);
            return null;
        }

        version = result.ToString();
        var component = new NpmComponent(name, version, hash);
        return component;
    }

    public static bool TryParseNpmVersion(string npmRegistryHost, string packageName, string versionString, out SemanticVersion version)
    {
        if (Uri.TryCreate(versionString, UriKind.Absolute, out var parsedUri))
        {
            if (string.Equals(npmRegistryHost, parsedUri.Host, StringComparison.OrdinalIgnoreCase))
            {
                return TryParseNpmRegistryVersion(packageName, parsedUri, out version);
            }
        }

        version = null;
        return false;
    }

    public static bool TryParseNpmRegistryVersion(string packageName, Uri versionString, out SemanticVersion version)
    {
        try
        {
            var file = Path.GetFileNameWithoutExtension(versionString.LocalPath);
            var potentialVersion = file.Replace($"{packageName}-", string.Empty);

            return SemanticVersion.TryParse(potentialVersion, out version);
        }
        catch (Exception)
        {
            version = null;
            return false;
        }
    }

    public static IDictionary<string, IDictionary<string, bool>> TryGetAllPackageJsonDependencies(Stream stream, out IList<string> yarnWorkspaces)
    {
        yarnWorkspaces = [];
        using var file = new StreamReader(stream);
        var jsonString = file.ReadToEnd();
        var node = JsonNode.Parse(jsonString);

        var dependencies = PullDependenciesFromJsonNode(node, "dependencies")
            .Concat(PullDependenciesFromJsonNode(node, "optionalDependencies"))
            .ToDictionary(x => x.Key, x => x.Value);
        var devDependencies = PullDependenciesFromJsonNode(node, "devDependencies");

        if (node?["private"]?.GetValue<bool>() == true && node["workspaces"] != null)
        {
            if (node["workspaces"] is JsonArray arr)
            {
                yarnWorkspaces = arr.Select(x => x.GetValue<string>()).ToList();
            }
            else if (node["workspaces"] is JsonObject obj && obj["packages"] is JsonArray arr2)
            {
                yarnWorkspaces = arr2.Select(x => x.GetValue<string>()).ToList();
            }
        }

        return AttachDevInformationToDependencies(dependencies, false)
            .Concat(AttachDevInformationToDependencies(devDependencies, true))
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.First().Value);
    }

    /// <summary>
    /// Gets the module name, stripping off the "node_modules/" prefix if it exists.
    /// </summary>
    /// <param name="name">The name of the module.</param>
    /// <returns>The module name, stripped of the "node_modules/" prefix if it exists.</returns>
    public static string GetModuleName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var index = name.LastIndexOf($"{NodeModules}/", StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            name = name[(index + $"{NodeModules}/".Length)..];
        }

        return name;
    }

    private static bool IsPackageNameValid(string name)
    {
        if (Uri.TryCreate(name, UriKind.Absolute, out _))
        {
            return false;
        }

        return !(name.Length >= 214
                 || name.StartsWith('.')
                 || name.StartsWith('_')
                 || UnsafeCharactersRegex.IsMatch(name));
    }

    private static IDictionary<string, IDictionary<string, bool>> AttachDevInformationToDependencies(IDictionary<string, string> dependencies, bool isDev)
    {
        var returnedDependencies = new Dictionary<string, IDictionary<string, bool>>();
        foreach (var item in dependencies)
        {
            returnedDependencies[item.Key] = new Dictionary<string, bool> { { item.Value, isDev } };
        }

        return returnedDependencies;
    }

    private static IDictionary<string, string> PullDependenciesFromJsonNode(JsonNode node, string dependencyType)
    {
        var result = new Dictionary<string, string>();
        if (node?[dependencyType] is JsonObject depObj)
        {
            foreach (var kv in depObj)
            {
                result[kv.Key] = kv.Value?.GetValue<string>();
            }
        }

        return result;
    }
}
