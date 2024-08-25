using Domosharp.Api.Models;
using Newtonsoft.Json;
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
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    response.EnsureSuccessStatusCode();
    var responseString = await response.Content.ReadAsStringAsync();
    Assert.NotNull(responseString);
    Assert.NotEmpty(responseString);
    var devices = JsonConvert.DeserializeObject<IEnumerable<DeviceResponse>>(responseString);
    Assert.NotNull(devices);
    client.Dispose();
    await server.DisposeAsync();

    Assert.Empty(devices);
  }

}