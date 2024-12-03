using System;
using System.Collections.Concurrent;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Sanet.MekForge.Avalonia.Services;

public class AvaloniaAssetImageService : IImageService
{
    private readonly ConcurrentDictionary<string, Bitmap?> _cache = new();
    private const string AssetsBasePath = "avares://Sanet.MekForge.Avalonia/Assets";

    public Bitmap? GetImage(string assetType, string assetName)
    {
        var path = $"{AssetsBasePath}/{assetType.ToLower()}/{assetName.ToLower()}.gif";
        return _cache.GetOrAdd(path, LoadImage);
    }

    public void PreloadImages()
    {
        // TODO: Implement preloading by scanning the Assets directory
        // For now, we'll use lazy loading
    }

    private static Bitmap? LoadImage(string path)
    {
        try
        {
            var uri = new Uri(path);
            var asset =AssetLoader.Open(uri);
            return new Bitmap(asset);
        }
        catch
        {
            return null;
        }
    }
}
