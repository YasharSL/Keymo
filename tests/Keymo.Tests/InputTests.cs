namespace Keymo.Tests;

public class InputTests
{
    [Theory]
    [InlineData(1366)]
    [InlineData(1920)]
    [InlineData(2560)]
    [InlineData(5760)] // three monitors side by side
    public void Absolute_coordinate_scales_back_to_the_same_pixel(int desktopSize)
    {
        const int AbsoluteRange = 65536;

        for (int pixel = 0; pixel < desktopSize; pixel++)
        {
            long scaledBack = (long)Input.ToAbsolute(pixel, desktopSize) * desktopSize / AbsoluteRange;

            Assert.Equal(pixel, scaledBack);
        }
    }
}
