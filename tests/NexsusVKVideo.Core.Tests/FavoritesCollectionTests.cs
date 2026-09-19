using NexsusVKVideo.Core.Models;
using NexsusVKVideo.Core.Services;

namespace NexsusVKVideo.Core.Tests;

public sealed class FavoritesCollectionTests
{
    [Fact]
    public void TryAdd_WhenVideoKeyAlreadyExists_DoesNotAddDuplicate()
    {
        var favorites = new FavoritesCollection();
        var original = new VideoSummary(new VideoKey("vk", "123"), "Original title");
        var duplicate = new VideoSummary(new VideoKey("vk", "123"), "Updated title");

        var firstAdded = favorites.TryAdd(original);
        var duplicateAdded = favorites.TryAdd(duplicate);

        Assert.True(firstAdded);
        Assert.False(duplicateAdded);
        var savedVideo = Assert.Single(favorites.Videos);
        Assert.Equal("Original title", savedVideo.Title);
    }

    [Fact]
    public void TryAdd_WhenProviderDiffers_AllowsSameVideoId()
    {
        var favorites = new FavoritesCollection();

        var firstAdded = favorites.TryAdd(new VideoSummary(new VideoKey("vk", "123"), "VK video"));
        var secondAdded = favorites.TryAdd(new VideoSummary(new VideoKey("demo", "123"), "Demo video"));

        Assert.True(firstAdded);
        Assert.True(secondAdded);
        Assert.Equal(2, favorites.Videos.Count);
    }

    [Fact]
    public void Remove_WhenVideoExists_RemovesItFromTheCollection()
    {
        var favorites = new FavoritesCollection();
        var key = new VideoKey("vk", "123");
        favorites.TryAdd(new VideoSummary(key, "Video"));

        var removed = favorites.Remove(key);

        Assert.True(removed);
        Assert.Empty(favorites.Videos);
    }
}
