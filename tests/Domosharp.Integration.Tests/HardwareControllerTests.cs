using Domosharp.Api.Models;

using System.Net;
using System.Text.Json;

namespace Domosharp.Integration.Tests;

public class HardwareControllerTests
{
  [Fact]
  [Trait("Category", "Integration")]
  public async Task GetAllHardwares_ReturnsEmptyArray()
  {
    // Arrange
    var server = new DomosharpWebApplication();
    var client = server.CreateClient();

    // Act
    var response = await client.GetAsync("/api/v1/hardware");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    response.EnsureSuccessStatusCode();
    var responseString = await response.Content.ReadAsStringAsync();
    Assert.NotNull(responseString);
    Assert.NotEmpty(responseString);
    var hardwares = JsonSerializer.Deserialize<IEnumerable<HardwareResponse>>(responseString);
    Assert.NotNull(hardwares);
    client.Dispose();
    await server.DisposeAsync();

    Assert.Empty(hardwares);
  }

}