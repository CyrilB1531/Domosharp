using Domosharp.Infrastructure.Mappers;

namespace Domosharp.Infrastructure.Tests.Mappers
{
  public class HardwareEntityMapperTests
  {
    [Fact]
    public void Mapper_WithNullHardware_ReturnsNull()
    {
      // Act
      var result = IHardwareExtensions.MapToEntity(null, 1, DateTime.Now);
      // Assert
      Assert.Null(result);
    }
  }
}
