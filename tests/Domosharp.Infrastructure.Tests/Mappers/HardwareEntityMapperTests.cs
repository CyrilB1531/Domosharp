using Domosharp.Infrastructure.Mappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domosharp.Infrastructure.Tests.Mappers
{
  public class HardwareEntityMapperTests
  {
    [Fact]
    public void Mapper_WithNullHardware_ReturnsNull()
    {
      // Act
      var result = IHardwareExtensions.MapHardwareToEntity(null);
      // Assert
      Assert.Null(result);
    }
  }
}
