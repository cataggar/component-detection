namespace Microsoft.ComponentDetection.Detectors.Npm;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ComponentDetection.Common;
using Microsoft.ComponentDetection.Contracts;
using Microsoft.ComponentDetection.Contracts.Internal;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Microsoft.Extensions.Logging;

public abstract class NpmLockfileDetectorBase : FileComponentDetector
{
    private const string NpmRegistryHost = "registry.npmjs.org";

    private const string LernaSearchPattern = "lerna.json";

    private readonly object lernaFilesLock = new object();

    private readonly IPathUtilityService pathUtilityService;

    protected NpmLockfileDetectorBase(
        IComponentStreamEnumerableFactory componentStreamEnumerableFactory,
        IObservableDirectoryWalkerFactory walkerFactory,
        IPathUtilityService pathUtilityService,
        ILogger logger)
    {
        this.ComponentStreamEnumerableFactory = componentStreamEnumerableFactory;
        this.Scanner = walkerFactory;
        this.pathUtilityService = pathUtilityService;
        this.Logger = logger;
    }

    protected NpmLockfileDetectorBase(IPathUtilityService pathUtilityService) => this.pathUtilityService = pathUtilityService;

    protected delegate bool JsonNodeProcessingDelegate(JsonNode node);

    public override IEnumerable<string> Categories => [Enum.GetName(typeof(DetectorClass), DetectorClass.Npm)];

    public override IList<string> SearchPatterns { get; } = ["package-lock.json", "npm-shrinkwrap.json", LernaSearchPattern];

    public override IEnumerable<ComponentType> SupportedComponentTypes { get; } = [ComponentType.Npm];

    private List<ProcessRequest> LernaFiles { get; } = [];

    protected override IList<string> SkippedFolders => ["node_modules", "pnpm-store"];

    protected abstract bool IsSupportedLockfileVersion(int lockfileVersion);

    protected abstract JsonNode ResolveDependencyObject(JsonNode packageLockNode);

    protected abstract void EnqueueAllDependencies(
        IDictionary<string, JsonNode> dependencyLookup,
        ISingleFileComponentRecorder singleFileComponentRecorder,
        Queue<(JsonNode CurrentSubDependency, TypedComponent ParentComponent)> subQueue,
        JsonNode currentDependency,
        TypedComponent typedComponent);

    protected abstract bool TryEnqueueFirstLevelDependencies(
        Queue<(JsonNode DependencyNode, TypedComponent ParentComponent)> queue,
        JsonNode dependencies,
        IDictionary<string, JsonNode> dependencyLookup,
        TypedComponent parentComponent = null,
        bool skipValidation = false);

    protected override Task<IObservable<ProcessRequest>> OnPrepareDetectionAsync(
        IObservable<ProcessRequest> processRequests,
        IDictionary<string, string> detectorArgs,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(this.RemoveNodeModuleNestedFiles(processRequests)
            .Where(pr =>
            {
                if (!pr.ComponentStream.Pattern.Equals(LernaSearchPattern))
                {
                    return true;
                }

                // Lock LernaFiles so we don't add while we are enumerating in processFiles
                lock (this.lernaFilesLock)
                {
                    this.LernaFiles.Add(pr);
                    return false;
                }
            }));

    protected override async Task OnFileFoundAsync(ProcessRequest processRequest, IDictionary<string, string> detectorArgs, CancellationToken cancellationToken = default)
    {
        IEnumerable<string> packageJsonPattern = ["package.json"];
        var singleFileComponentRecorder = processRequest.SingleFileComponentRecorder;
        var file = processRequest.ComponentStream;

        var packageJsonComponentStream = this.ComponentStreamEnumerableFactory.GetComponentStreams(new FileInfo(file.Location).Directory, packageJsonPattern, (name, directoryName) => false, false);

        IList<ProcessRequest> lernaFilesClone;

        // ToList LernaFiles to generate a copy we can act on without holding the lock for a long time
        lock (this.lernaFilesLock)
        {
            lernaFilesClone = this.LernaFiles.ToList();
        }

        var foundUnderLerna = lernaFilesClone.Select(lernaProcessRequest => lernaProcessRequest.ComponentStream)
            .Any(lernaFile => this.pathUtilityService.IsFileBelowAnother(
                lernaFile.Location,
                file.Location));

        await this.SafeProcessAllPackageJsonNodesAsync(file, (node) =>
        {
            if (!foundUnderLerna &&
                (node["name"] == null ||
                 node["version"] == null ||
                 string.IsNullOrWhiteSpace(node["name"].GetValue<string>()) ||
                 string.IsNullOrWhiteSpace(node["version"].GetValue<string>())))
            {
                this.Logger.LogInformation("{PackageLogJsonFile} does not contain a valid name and/or version. These are required fields for a valid package-lock.json file. It and its dependencies will not be registered.", file.Location);
                return false;
            }

            this.ProcessIndividualPackageJsonNodes(singleFileComponentRecorder, node, packageJsonComponentStream, skipValidation: foundUnderLerna);
            return true;
        });
    }

    protected async Task ProcessAllPackageJsonNodesAsync(IComponentStream componentStream, JsonNodeProcessingDelegate jsonNodeProcessor)
    {
        try
        {
            if (!componentStream.Stream.CanRead)
            {
                componentStream.Stream.ReadByte();
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogInformation(ex, "Could not read {ComponentStreamFile} file.", componentStream.Location);
            return;
        }

        using var file = new StreamReader(componentStream.Stream);
        var jsonString = await file.ReadToEndAsync();
        var node = JsonNode.Parse(jsonString);
        jsonNodeProcessor(node);
        return;
    }

    private void ProcessIndividualPackageJsonNodes(ISingleFileComponentRecorder singleFileComponentRecorder, JsonNode packageLockJsonNode, IEnumerable<IComponentStream> packageJsonComponentStream, bool skipValidation = false)
    {
        var lockfileVersion = packageLockJsonNode["lockfileVersion"].GetValue<int>();
        this.RecordLockfileVersion(lockfileVersion);

        if (!this.IsSupportedLockfileVersion(lockfileVersion))
        {
            return;
        }

        var dependencies = this.ResolveDependencyObject(packageLockJsonNode);
        var topLevelDependencies = new Queue<(JsonNode, TypedComponent)>();

        var dependencyLookup = dependencies.AsObject().ToDictionary(dependency => dependency.Key, dependency => dependency.Value);

        foreach (var stream in packageJsonComponentStream)
        {
            using var file = new StreamReader(stream.Stream);
            var jsonString = file.ReadToEnd();
            var packageJsonNode = JsonNode.Parse(jsonString);
            var enqueued = this.TryEnqueueFirstLevelDependencies(topLevelDependencies, packageJsonNode["dependencies"], dependencyLookup, skipValidation: skipValidation);
            enqueued = enqueued && this.TryEnqueueFirstLevelDependencies(topLevelDependencies, packageJsonNode["devDependencies"], dependencyLookup, skipValidation: skipValidation);
            enqueued = enqueued && this.TryEnqueueFirstLevelDependencies(topLevelDependencies, packageJsonNode["optionalDependencies"], dependencyLookup, skipValidation: skipValidation);
            if (!enqueued)
            {
                // This represents a mismatch between lock file and package.json, break out and do not register anything for these files
                throw new InvalidOperationException(string.Format("InvalidPackageJson -- There was a mismatch between the components in the package.json and the lock file at '{0}'", singleFileComponentRecorder.ManifestFileLocation));
            }
        }

        if (!packageJsonComponentStream.Any())
        {
            throw new InvalidOperationException(string.Format("InvalidPackageJson -- There must be a package.json file at '{0}' for components to be registered", singleFileComponentRecorder.ManifestFileLocation));
        }

        this.TraverseRequirementAndDependencyTree(topLevelDependencies, dependencyLookup, singleFileComponentRecorder);
    }

    private IObservable<ProcessRequest> RemoveNodeModuleNestedFiles(IObservable<ProcessRequest> componentStreams)
    {
        var directoryItemFacades = new List<DirectoryItemFacade>();
        var directoryItemFacadesByPath = new Dictionary<string, DirectoryItemFacade>();

        return Observable.Create<ProcessRequest>(s =>
        {
            return componentStreams.Subscribe(
                processRequest =>
                {
                    var item = processRequest.ComponentStream;
                    var currentDir = item.Location;
                    DirectoryItemFacade last = null;
                    do
                    {
                        currentDir = this.pathUtilityService.GetParentDirectory(currentDir);

                        // We've reached the top / root
                        if (currentDir == null)
                        {
                            // If our last directory isn't in our list of top level nodes, it should be added. This happens for the first processed item and then subsequent times we have a new root (edge cases with multiple hard drives, for example)
                            if (!directoryItemFacades.Contains(last))
                            {
                                directoryItemFacades.Add(last);
                            }

                            var skippedFolder = this.SkippedFolders.FirstOrDefault(folder => item.Location.Contains(folder));

                            // When node_modules is part of the path down to a given item, we skip the item. Otherwise, we yield the item.
                            if (string.IsNullOrEmpty(skippedFolder))
                            {
                                s.OnNext(processRequest);
                            }
                            else
                            {
                                this.Logger.LogDebug("Ignoring package-lock.json at {PackageLockJsonLocation}, as it is inside a {SkippedFolder} folder.", item.Location, skippedFolder);
                            }

                            break;
                        }

                        var directoryExisted = directoryItemFacadesByPath.TryGetValue(currentDir, out var current);
                        if (!directoryExisted)
                        {
                            directoryItemFacadesByPath[currentDir] = current = new DirectoryItemFacade
                            {
                                Name = currentDir,
                                Files = [],
                                Directories = [],
                            };
                        }

                        // If we came from a directory, we add it to our graph.
                        if (last != null)
                        {
                            current.Directories.Add(last);
                        }

                        // If we didn't come from a directory, it's because we're just getting started. Our current directory should include the file that led to it showing up in the graph.
                        else
                        {
                            current.Files.Add(item);
                        }

                        last = current;
                    }

                    // Go all the way up
                    while (currentDir != null);
                },
                s.OnCompleted);
        });
    }

    private async Task SafeProcessAllPackageJsonNodesAsync(IComponentStream componentStream, JsonNodeProcessingDelegate jsonNodeProcessor)
    {
        try
        {
            await this.ProcessAllPackageJsonNodesAsync(componentStream, jsonNodeProcessor);
        }
        catch (Exception e)
        {
            // If something went wrong, just ignore the component
            this.Logger.LogInformation(e, "Could not parse JsonNodes from {ComponentLocation} file.", componentStream.Location);
        }
    }

    private void TraverseRequirementAndDependencyTree(
        IEnumerable<(JsonNode Dependency, TypedComponent ParentComponent)> topLevelDependencies,
        IDictionary<string, JsonNode> dependencyLookup,
        ISingleFileComponentRecorder singleFileComponentRecorder)
    {
        foreach (var (currentDependency, _) in topLevelDependencies)
        {
            var typedComponent = NpmComponentUtilities.GetTypedComponent(currentDependency, NpmRegistryHost, this.Logger);
            if (typedComponent == null)
            {
                continue;
            }

            var previouslyAddedComponents = new HashSet<string> { typedComponent.Id };
            var subQueue = new Queue<(JsonNode, TypedComponent)>();

            NpmComponentUtilities.TraverseAndRecordComponents(currentDependency, singleFileComponentRecorder, typedComponent, explicitReferencedDependency: typedComponent);

            this.EnqueueAllDependencies(dependencyLookup, singleFileComponentRecorder, subQueue, currentDependency, typedComponent);

            while (subQueue.Count != 0)
            {
                var (currentSubDependency, parentComponent) = subQueue.Dequeue();

                var typedSubComponent = NpmComponentUtilities.GetTypedComponent(currentSubDependency, NpmRegistryHost, this.Logger);

                if (typedSubComponent == null || previouslyAddedComponents.Contains(typedSubComponent.Id))
                {
                    continue;
                }

                previouslyAddedComponents.Add(typedSubComponent.Id);

                NpmComponentUtilities.TraverseAndRecordComponents(currentSubDependency, singleFileComponentRecorder, typedSubComponent, explicitReferencedDependency: typedComponent, parentComponent.Id);

                this.EnqueueAllDependencies(dependencyLookup, singleFileComponentRecorder, subQueue, currentSubDependency, typedSubComponent);
            }
        }
    }
}
