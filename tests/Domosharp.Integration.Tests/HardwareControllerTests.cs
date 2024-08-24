using Domosharp.Api.Models;

using Newtonsoft.Json;
using System.Net;

namespace Domosharp.Integration.Tests;

public class HardwareControllerTests
{
  [Fact]
  [Trait("Category", "Integration")]
  public async Task ShouldGetAllHardwaresReturnsEmptyArray()
  {
    var server = new DomosharpWebApplication();
    var client = server.CreateClient();

    var response = await client.GetAsync("/api/v1/hardware");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    response.EnsureSuccessStatusCode();
    var responseString = await response.Content.ReadAsStringAsync();
    Assert.NotNull(responseString);
    Assert.NotEmpty(responseString);
    var devices = JsonConvert.DeserializeObject<IEnumerable<HardwareResponse>>(responseString);
    Assert.NotNull(devices);
    client.Dispose();
    await server.DisposeAsync();

    Assert.Empty(devices);
  }

}