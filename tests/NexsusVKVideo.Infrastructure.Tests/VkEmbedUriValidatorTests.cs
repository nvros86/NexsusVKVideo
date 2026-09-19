using NexsusVKVideo.Infrastructure.Vk;

namespace NexsusVKVideo.Infrastructure.Tests;

public sealed class VkEmbedUriValidatorTests
{
    [Fact]
    public void IsAllowedEmbed_AcceptsTheOfficialWidgetShape()
    {
        var uri = new Uri("https://vk.ru/video_ext.php?oid=-22822305&id=456241864&hd=2");

        Assert.True(VkEmbedUriValidator.IsAllowedEmbed(uri));
    }

    [Theory]
    [InlineData("http://vk.ru/video_ext.php?oid=-1&id=2")]
    [InlineData("https://vk.ru.evil.example/video_ext.php?oid=-1&id=2")]
    [InlineData("https://vk.ru/video-22822305_456241864")]
    [InlineData("https://vk.ru/video_ext.php?oid=-1")]
    public void IsAllowedEmbed_RejectsAnythingOutsideTheAllowlist(string value)
    {
        Assert.False(VkEmbedUriValidator.IsAllowedEmbed(new Uri(value)));
    }

    [Fact]
    public void IsAllowedVideoPage_AcceptsTheOfficialVideoPageShape()
    {
        Assert.True(VkEmbedUriValidator.IsAllowedVideoPage(new Uri("https://vk.ru/video-22822305_456241864")));
    }

    [Theory]
    [InlineData("https://vkvideo.ru/")]
    [InlineData("https://id.vk.ru/auth")]
    [InlineData("https://oauth.vk.com/authorize")]
    public void IsAllowedVkSite_AcceptsHttpsVkOwnedHosts(string value)
    {
        Assert.True(VkEmbedUriValidator.IsAllowedVkSite(new Uri(value)));
    }

    [Theory]
    [InlineData("http://vkvideo.ru/")]
    [InlineData("https://vkvideo.ru.evil.example/")]
    [InlineData("https://example.com/")]
    public void IsAllowedVkSite_RejectsOtherHostsAndSchemes(string value)
    {
        Assert.False(VkEmbedUriValidator.IsAllowedVkSite(new Uri(value)));
    }
}
