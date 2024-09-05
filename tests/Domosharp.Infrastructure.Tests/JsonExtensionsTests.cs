using Domosharp.Infrastructure.Entities;

using System.Text.Json;

namespace Domosharp.Infrastructure.Tests;

public class JsonExtensionsTests
{

  [Fact]
  public void Json_WithTesting_ReturnsWellFormattedString()
  {
    // Arrange
    var obj = new TestingJson() { TrueValue = true, ShutterOption = new ShutterOption(255), FalseValue = false };

    // Act
    var result = JsonSerializer.Serialize(obj, JsonExtensions.FullObjectOnDeserializing);

    // Assert
    Assert.Equal("{\"TrueValue\":1,\"FalseValue\":0,\"ShutterOption\":31}", result);
  }

  private class TestingJson
  {
    public bool TrueValue { get; set; }
    public bool FalseValue { get; set; }

    public ShutterOption ShutterOption { get; set; } = new ShutterOption(0);

  }
}