namespace Microsoft.ComponentDetection.Detectors.Npm;

using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.ComponentDetection.Contracts;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Microsoft.Extensions.Logging;

public class NpmLockfile3Detector : NpmLockfileDetectorBase
{
    private static readonly string NodeModules = NpmComponentUtilities.NodeModules;

    public NpmLockfile3Detector(
        IComponentStreamEnumerableFactory componentStreamEnumerableFactory,
        IObservableDirectoryWalkerFactory walkerFactory,
        IPathUtilityService pathUtilityService,
        ILogger<NpmLockfile3Detector> logger)
        : base(
            componentStreamEnumerableFactory,
            walkerFactory,
            pathUtilityService,
            logger)
    {
    }

    public NpmLockfile3Detector(IPathUtilityService pathUtilityService)
        : base(pathUtilityService)
    {
    }

    public override string Id => "NpmLockfile3";

    public override int Version => 2;

    protected override bool IsSupportedLockfileVersion(int lockfileVersion) => lockfileVersion == 3;

    protected override JsonNode ResolveDependencyObject(JsonNode packageLockNode) => packageLockNode?["packages"];

    protected override bool TryEnqueueFirstLevelDependencies(
        Queue<(JsonNode DependencyNode, TypedComponent ParentComponent)> queue,
        JsonNode dependencies,
        IDictionary<string, JsonNode> dependencyLookup,
        TypedComponent parentComponent = null,
        bool skipValidation = false)
    {
        if (dependencies == null)
        {
            return true;
        }

        var isValid = true;

        foreach (var dependency in dependencies.AsObject())
        {
            if (dependency.Key == null)
            {
                continue;
            }

            var inLock = dependencyLookup.TryGetValue($"{NodeModules}/{dependency.Key}", out var dependencyNode);
            if (inLock)
            {
                queue.Enqueue((dependencyNode, parentComponent));
            }
            else if (!skipValidation)
            {
                isValid = false;
            }
        }

        return isValid;
    }

    protected override void EnqueueAllDependencies(
        IDictionary<string, JsonNode> dependencyLookup,
        ISingleFileComponentRecorder singleFileComponentRecorder,
        Queue<(JsonNode CurrentSubDependency, TypedComponent ParentComponent)> subQueue,
        JsonNode currentDependency,
        TypedComponent typedComponent)
    {
        this.TryEnqueueFirstLevelDependenciesLockfile3(
            subQueue,
            currentDependency?["dependencies"],
            dependencyLookup,
            singleFileComponentRecorder,
            parentComponent: typedComponent);
    }

    private void TryEnqueueFirstLevelDependenciesLockfile3(
        Queue<(JsonNode DependencyNode, TypedComponent ParentComponent)> queue,
        JsonNode dependencies,
        IDictionary<string, JsonNode> dependencyLookup,
        ISingleFileComponentRecorder componentRecorder,
        TypedComponent parentComponent)
    {
        if (dependencies == null)
        {
            return;
        }

        foreach (var dependency in dependencies.AsObject())
        {
            if (dependency.Key == null)
            {
                continue;
            }

            var inLock = dependencyLookup.TryGetValue($"{NodeModules}/{dependency.Key}", out var dependencyNode);
            if (inLock)
            {
                queue.Enqueue((dependencyNode, parentComponent));
            }
            else
            {
                this.Logger.LogWarning("Could not find dependency {Dependency} in lockfile", dependency.Key);
            }
        }
    }
}
