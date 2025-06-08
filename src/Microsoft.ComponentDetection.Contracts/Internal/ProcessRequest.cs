namespace Microsoft.ComponentDetection.Contracts.Internal;

public class ProcessRequest
{
    public IComponentStream ComponentStream { get; set; } = null!;

    public ISingleFileComponentRecorder SingleFileComponentRecorder { get; set; } = null!;
}
