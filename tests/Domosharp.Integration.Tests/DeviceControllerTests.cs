using System.Net;

namespace Domosharp.Integration.Tests;

public class DeviceControllerTests
{
  [Fact]
  [Trait("Category", "Integration")]
  public async Task GetAllDevices_WithUnknownHardware_ReturnsEmptyArray()
  {
    // Arrange
    var server = new DomosharpWebApplication();
    var client = server.CreateClient();

    // Act
    var response = await client.GetAsync("/api/v1/device/hardware/1");

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    response.EnsureSuccessStatusCode();
    client.Dispose();
    await server.DisposeAsync();
  }

}