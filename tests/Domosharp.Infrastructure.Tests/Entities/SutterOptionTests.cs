using Domosharp.Infrastructure.Entities;

namespace Domosharp.Infrastructure.Tests.Entities;

public class ShutterOptionTests
{
  [Theory]
  [InlineData(0, false, false, false, false, false)]
  [InlineData(1, true, false, false, false, false)]
  [InlineData(2, false, true, false, false, false)]
  [InlineData(3, true, true, false, false, false)]
  [InlineData(4, false, false, true, false, false)]
  [InlineData(5, true, false, true, false, false)]
  [InlineData(6, false, true, true, false, false)]
  [InlineData(7, true, true, true, false, false)]
  [InlineData(8, false, false, false, true, false)]
  [InlineData(9, true, false, false, true, false)]
  [InlineData(10, false, true, false, true, false)]
  [InlineData(11, true, true, false, true, false)]
  [InlineData(12, false, false, true, true, false)]
  [InlineData(13, true, false, true, true, false)]
  [InlineData(14, false, true, true, true, false)]
  [InlineData(15, true, true, true, true, false)]
  [InlineData(16, false, false, false, false, true)]
  [InlineData(17, true, false, false, false, true)]
  [InlineData(18, false, true, false, false, true)]
  [InlineData(19, true, true, false, false, true)]
  [InlineData(20, false, false, true, false, true)]
  [InlineData(21, true, false, true, false, true)]
  [InlineData(22, false, true, true, false, true)]
  [InlineData(23, true, true, true, false, true)]
  [InlineData(24, false, false, false, true, true)]
  [InlineData(25, true, false, false, true, true)]
  [InlineData(26, false, true, false, true, true)]
  [InlineData(27, true, true, false, true, true)]
  [InlineData(28, false, false, true, true, true)]
  [InlineData(29, true, false, true, true, true)]
  [InlineData(30, false, true, true, true, true)]
  [InlineData(31, true, true, true, true, true)]
  public void ShutterOption0_ReturnsEveryThingToGoodValue(byte option, bool invert, bool locked, bool extraEndStop, bool invertWebButtons, bool extraStopRelay)
  {
    // Act
    var result = new ShutterOption(option);

    // Assert
    Assert.Equal(extraStopRelay, result.ExtraStopRelay);
    Assert.Equal(extraEndStop, result.ExtraEndStop);
    Assert.Equal(invert, result.Invert);
    Assert.Equal(invertWebButtons, result.InvertWebButtons);
    Assert.Equal(locked, result.Lock);
    Assert.Equal(option, result.GetValue());
  }
}
