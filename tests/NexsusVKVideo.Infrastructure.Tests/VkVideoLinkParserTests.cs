using NexsusVKVideo.Infrastructure.Vk;

namespace NexsusVKVideo.Infrastructure.Tests;

public sealed class VkVideoLinkParserTests
{
    private readonly VkVideoLinkParser _parser = new();

    [Fact]
    public void TryParse_ConvertsOfficialPublicVideoUrlToWidgetUrl()
    {
        var parsed = _parser.TryParse(
            new Uri("https://vk.ru/video-22822305_456241864?list=example"),
            out var videoLink);

        Assert.True(parsed);
        Assert.NotNull(videoLink);
        Assert.Equal("vk", videoLink.Key.Provider);
        Assert.Equal("-22822305_456241864", videoLink.Key.VideoId);
        Assert.Equal("https://vk.ru/video-22822305_456241864", videoLink.VideoPageUri.AbsoluteUri);
        Assert.Equal("https://vk.ru/video_ext.php?oid=-22822305&id=456241864", videoLink.EmbedUri.AbsoluteUri);
    }

    [Theory]
    [InlineData("http://vk.ru/video-22822305_456241864")]
    [InlineData("https://vk.ru.evil.example/video-22822305_456241864")]
    [InlineData("https://vk.ru/video-22822305_456241864/extra")]
    [InlineData("https://vk.ru/video-22822305_not-a-video")]
    [InlineData("https://vk.ru/video_ext.php?oid=-22822305&id=456241864")]
    public void TryParse_RejectsAnythingOutsidePublicVideoUrlShape(string value)
    {
        Assert.False(_parser.TryParse(new Uri(value), out _));
    }
}
