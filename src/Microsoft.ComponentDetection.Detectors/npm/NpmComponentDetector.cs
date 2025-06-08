namespace Microsoft.ComponentDetection.Detectors.Npm;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using global::NuGet.Versioning;
using Microsoft.ComponentDetection.Contracts;
using Microsoft.ComponentDetection.Contracts.Internal;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Microsoft.Extensions.Logging;

public class NpmComponentDetector : FileComponentDetector
{
    private static readonly Regex SingleAuthor = new Regex(@"^(?<name>([^<(]+?)?)[ \t]*(?:<(?<email>([^>(]+?))>)?[ \t]*(?:\(([^)]+?)\)|$)", RegexOptions.Compiled);

    public NpmComponentDetector(
        IComponentStreamEnumerableFactory componentStreamEnumerableFactory,
        IObservableDirectoryWalkerFactory walkerFactory,
        ILogger<NpmComponentDetector> logger)
    {
        this.ComponentStreamEnumerableFactory = componentStreamEnumerableFactory;
        this.Scanner = walkerFactory;
        this.Logger = logger;
    }

    /// <summary>Common delegate for Package.json JsonNode processing.</summary>
    /// <param name="node">A JsonNode, usually corresponding to a package.json file.</param>
    /// <returns>Used in scenarios where one file path creates multiple JsonNodes, a false value indicates processing additional JsonNodes should be halted, proceed otherwise.</returns>
    protected delegate bool JsonNodeProcessingDelegate(JsonNode node);

    public override string Id { get; } = "Npm";

    public override IEnumerable<string> Categories => [Enum.GetName(typeof(DetectorClass), DetectorClass.Npm)];

    public override IList<string> SearchPatterns { get; } = ["package.json"];

    public override IEnumerable<ComponentType> SupportedComponentTypes { get; } = [ComponentType.Npm];

    public override int Version { get; } = 3;

    protected override async Task OnFileFoundAsync(ProcessRequest processRequest, IDictionary<string, string> detectorArgs, CancellationToken cancellationToken = default)
    {
        var singleFileComponentRecorder = processRequest.SingleFileComponentRecorder;
        var file = processRequest.ComponentStream;

        var filePath = file.Location;

        string contents;
        using (var reader = new StreamReader(file.Stream))
        {
            contents = await reader.ReadToEndAsync(cancellationToken);
        }

        await this.SafeProcessAllPackageJsonNodesAsync(filePath, contents, (node) =>
        {
            if (node["name"] == null || node["version"] == null)
            {
                this.Logger.LogInformation("{BadPackageJson} does not contain a name and/or version. These are required fields for a valid package.json file. It and its dependencies will not be registered.", filePath);
                return false;
            }

            return this.ProcessIndividualPackageJsonNodes(filePath, singleFileComponentRecorder, node);
        });
    }

    protected virtual Task ProcessAllPackageJsonNodesAsync(string contents, JsonNodeProcessingDelegate nodeProcessor)
    {
        var o = JsonNode.Parse(contents);
        nodeProcessor(o);
        return Task.CompletedTask;
    }

    protected virtual bool ProcessIndividualPackageJsonNodes(string filePath, ISingleFileComponentRecorder singleFileComponentRecorder, JsonNode packageNode)
    {
        var name = packageNode["name"]?.GetValue<string>();
        var version = packageNode["version"]?.GetValue<string>();
        var authorNode = packageNode["author"];
        var enginesNode = packageNode["engines"];

        if (!SemanticVersion.TryParse(version, out _))
        {
            this.Logger.LogWarning("Unable to parse version {NpmPackageVersion} for package {NpmPackageName} found at path {NpmPackageLocation}. This may indicate an invalid npm package component and it will not be registered.", version, name, filePath);
            singleFileComponentRecorder.RegisterPackageParseFailure($"{name} - {version}");
            return false;
        }

        var containsVsCodeEngine = false;
        if (enginesNode != null)
        {
            if (enginesNode is JsonArray arr)
            {
                var engineStrings = arr
                    .Where(t => t is JsonValue)
                    .Select(t => t.GetValue<string>())
                    .ToArray();
                if (engineStrings.Any(e => e.Contains("vscode")))
                {
                    containsVsCodeEngine = true;
                }
            }
            else if (enginesNode is JsonObject obj)
            {
                if (obj["vscode"] != null)
                {
                    containsVsCodeEngine = true;
                }
            }
        }

        if (containsVsCodeEngine)
        {
            this.Logger.LogInformation("{NpmPackageName} found at path {NpmPackageLocation} represents a built-in VS Code extension. This package will not be registered.", name, filePath);
            return false;
        }

        var npmComponent = new NpmComponent(name, version, author: this.GetAuthor(authorNode, name, filePath));

        singleFileComponentRecorder.RegisterUsage(new DetectedComponent(npmComponent));
        return true;
    }

    private async Task SafeProcessAllPackageJsonNodesAsync(string sourceFilePath, string contents, JsonNodeProcessingDelegate nodeProcessor)
    {
        try
        {
            await this.ProcessAllPackageJsonNodesAsync(contents, nodeProcessor);
        }
        catch (Exception e)
        {
            // If something went wrong, just ignore the component
            this.Logger.LogInformation(e, "Could not parse JsonNodes from file {PackageJsonFilePaths}.", sourceFilePath);
        }
    }

    private NpmAuthor GetAuthor(JsonNode authorNode, string packageName, string filePath)
    {
        var authorString = authorNode?.ToJsonString();
        if (string.IsNullOrEmpty(authorString))
        {
            return null;
        }

        string authorName = null;
        string authorEmail = null;

        if (authorNode is JsonObject obj)
        {
            authorName = obj["name"]?.GetValue<string>();
            authorEmail = obj["email"]?.GetValue<string>();
        }
        else if (authorNode is JsonValue val)
        {
            var authorStr = val.GetValue<string>();
            var authorMatch = SingleAuthor.Match(authorStr);
            if (authorMatch.Success)
            {
                authorName = authorMatch.Groups["name"].ToString().Trim();
                authorEmail = authorMatch.Groups["email"].ToString().Trim();
            }
        }

        if (string.IsNullOrEmpty(authorName))
        {
            this.Logger.LogWarning("Unable to parse author:[{NpmAuthorString}] for package:[{NpmPackageName}] found at path:[{NpmPackageLocation}]. This may indicate an invalid npm package author, and author will not be registered.", authorString, packageName, filePath);
            return null;
        }

        return new NpmAuthor(authorName, authorEmail);
    }
}
