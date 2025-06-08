namespace Microsoft.ComponentDetection.Detectors.Npm;

using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.ComponentDetection.Contracts;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Microsoft.Extensions.Logging;

public class NpmComponentDetectorWithRoots : NpmLockfileDetectorBase
{
    public NpmComponentDetectorWithRoots(
        IComponentStreamEnumerableFactory componentStreamEnumerableFactory,
        IObservableDirectoryWalkerFactory walkerFactory,
        IPathUtilityService pathUtilityService,
        ILogger<NpmComponentDetectorWithRoots> logger)
        : base(
            componentStreamEnumerableFactory,
            walkerFactory,
            pathUtilityService,
            logger)
    {
    }

    public NpmComponentDetectorWithRoots(IPathUtilityService pathUtilityService)
        : base(pathUtilityService)
    {
    }

    public override string Id => "NpmWithRoots";

    public override int Version => 3;

    protected override bool IsSupportedLockfileVersion(int lockfileVersion) => lockfileVersion != 3;

    protected override JsonNode ResolveDependencyObject(JsonNode packageLockNode) => packageLockNode?["dependencies"];

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

            var inLock = dependencyLookup.TryGetValue(dependency.Key, out var dependencyNode);
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
        this.EnqueueDependencies(subQueue, currentDependency?["dependencies"], parentComponent: typedComponent);
        this.TryEnqueueFirstLevelDependencies(subQueue, currentDependency?["requires"], dependencyLookup, parentComponent: typedComponent);
    }

    private void EnqueueDependencies(Queue<(JsonNode Dependency, TypedComponent ParentComponent)> queue, JsonNode dependencies, TypedComponent parentComponent)
    {
        if (dependencies == null)
        {
            return;
        }

        foreach (var dependency in dependencies.AsObject())
        {
            queue.Enqueue((dependency.Value, parentComponent));
        }
    }
}
