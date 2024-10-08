﻿using Domosharp.Business.Contracts.Models;
using Domosharp.Infrastructure.Mappers;

namespace Domosharp.Infrastructure.Tests.Mappers;

public class TasmotaDeviceMapperTests
{
  [Fact]
  public void MapTasmotaDevice_WithoutConfiguration_ReturnsNull()
  {
    // Arrange
    var device = new Device()
    {
      Id = 1,
      Name = "test",

    };

    // Act
    var result = device.MapToTasmotaDevice();

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public void MapTasmotaDevice_WithoutTasmotaDiscoveryPayloadConfiguration_ReturnsNull()
  {
    // Arrange
    var device = new Device()
    {
      Id = 1,
      Name = "test",
      SpecificParameters = "{\"test\":1}"
    };

    // Act
    var result = device.MapToTasmotaDevice();

    // Assert
    Assert.Null(result);
  }


}
