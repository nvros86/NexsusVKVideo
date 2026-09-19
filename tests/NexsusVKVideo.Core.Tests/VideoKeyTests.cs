using NexsusVKVideo.Core.Models;

namespace NexsusVKVideo.Core.Tests;

public sealed class VideoKeyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenProviderIsBlank_ThrowsArgumentException(string? provider)
    {
        Assert.Throws<ArgumentException>(() => new VideoKey(provider!, "123"));
    }

    [Fact]
    public void Constructor_TrimsValuesForStableIdentity()
    {
        var key = new VideoKey(" vk ", " 123 ");

        Assert.Equal("vk", key.Provider);
        Assert.Equal("123", key.VideoId);
    }
}
