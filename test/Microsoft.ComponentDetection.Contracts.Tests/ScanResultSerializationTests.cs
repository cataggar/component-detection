namespace Microsoft.ComponentDetection.Contracts.Tests;

using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.ComponentDetection.Contracts.BcdeModels;
using Microsoft.ComponentDetection.Contracts.TypedComponent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestCategory("Governance/All")]
[TestCategory("Governance/ComponentDetection")]
public class ScanResultSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), new TypedComponentConverter() },
    };

    private ScanResult scanResultUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        this.scanResultUnderTest = new ScanResult
        {
            ResultCode = ProcessingResultCode.PartialSuccess,
            ComponentsFound =
            [
                new ScannedComponent
                {
                    Component = new NpmComponent("SampleNpmComponent", "1.2.3"),
                    DetectorId = "NpmDetectorId",
                    IsDevelopmentDependency = true,
                    DependencyScope = DependencyScope.MavenCompile,
                    LocationsFoundAt =
                    [
                        "some/location",
                    ],
                    TopLevelReferrers =
                    [
                        new NpmComponent("RootNpmComponent", "4.5.6"),
                    ],
                },
            ],
            DetectorsInScan =
            [
                new Detector
                {
                    DetectorId = "NpmDetectorId",
                    IsExperimental = true,
                    SupportedComponentTypes =
                    [
                        ComponentType.Npm,
                    ],
                    Version = 2,
                },
            ],
            SourceDirectory = "D:\\test\\directory",
        };
    }

    [TestMethod]
    public void ScanResultSerialization_HappyPath()
    {
        var serializedResult = JsonSerializer.Serialize(this.scanResultUnderTest, JsonOptions);
        var actual = JsonSerializer.Deserialize<ScanResult>(serializedResult, JsonOptions);

        actual.ResultCode.Should().Be(ProcessingResultCode.PartialSuccess);
        actual.SourceDirectory.Should().Be("D:\\test\\directory");
        actual.ComponentsFound.Should().ContainSingle();
        var actualDetectedComponent = actual.ComponentsFound.First();
        actualDetectedComponent.DetectorId.Should().Be("NpmDetectorId");
        actualDetectedComponent.IsDevelopmentDependency.Should().Be(true);
        actualDetectedComponent.DependencyScope.Should().Be(DependencyScope.MavenCompile);
        actualDetectedComponent.LocationsFoundAt.Contains("some/location").Should().Be(true);

        var npmComponent = actualDetectedComponent.Component as NpmComponent;
        npmComponent.Should().NotBeNull();
        npmComponent.Name.Should().Be("SampleNpmComponent");
        npmComponent.Version.Should().Be("1.2.3");

        var rootNpmComponent = actualDetectedComponent.TopLevelReferrers.First() as NpmComponent;
        rootNpmComponent.Should().NotBeNull();
        rootNpmComponent.Name.Should().Be("RootNpmComponent");
        rootNpmComponent.Version.Should().Be("4.5.6");

        var actualDetector = actual.DetectorsInScan.First();
        actualDetector.DetectorId.Should().Be("NpmDetectorId");
        actualDetector.IsExperimental.Should().Be(true);
        actualDetector.Version.Should().Be(2);
        actualDetector.SupportedComponentTypes.Single().Should().Be(ComponentType.Npm);
    }

    [TestMethod]
    public void ScanResultSerialization_ExpectedJsonFormat()
    {
        var serializedResult = JsonSerializer.Serialize(this.scanResultUnderTest, JsonOptions);
        using var jsonDoc = JsonDocument.Parse(serializedResult);
        var json = jsonDoc.RootElement;

        json.GetProperty("resultCode").GetString().Should().Be("PartialSuccess");
        json.GetProperty("sourceDirectory").GetString().Should().Be("D:\\test\\directory");
        var foundComponent = json.GetProperty("componentsFound")[0];

        foundComponent.GetProperty("detectorId").GetString().Should().Be("NpmDetectorId");
        foundComponent.GetProperty("isDevelopmentDependency").GetBoolean().Should().Be(true);
        foundComponent.GetProperty("dependencyScope").GetString().Should().Be("MavenCompile");
        foundComponent.GetProperty("locationsFoundAt")[0].GetString().Should().Be("some/location");
        var component = foundComponent.GetProperty("component");
        component.GetProperty("type").GetString().Should().Be("Npm");
        component.GetProperty("name").GetString().Should().Be("SampleNpmComponent");
        component.GetProperty("version").GetString().Should().Be("1.2.3");

        var rootComponent = foundComponent.GetProperty("topLevelReferrers")[0];
        rootComponent.GetProperty("type").GetString().Should().Be("Npm");
        rootComponent.GetProperty("name").GetString().Should().Be("RootNpmComponent");
        rootComponent.GetProperty("version").GetString().Should().Be("4.5.6");

        var detector = json.GetProperty("detectorsInScan")[0];
        detector.GetProperty("detectorId").GetString().Should().Be("NpmDetectorId");
        detector.GetProperty("version").GetInt32().Should().Be(2);
        detector.GetProperty("isExperimental").GetBoolean().Should().Be(true);
        detector.GetProperty("supportedComponentTypes")[0].GetString().Should().Be("Npm");
    }
}
