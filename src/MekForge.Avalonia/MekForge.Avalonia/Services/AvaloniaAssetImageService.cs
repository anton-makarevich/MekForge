using System;
using System.Collections.Concurrent;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Sanet.MekForge.Core.Services;

namespace Sanet.MekForge.Avalonia.Services;

public class AvaloniaAssetImageService : IImageService<Bitmap>
{
    private readonly ConcurrentDictionary<string, Bitmap?> _cache = new();
    private const string AssetsBasePath = "avares://Sanet.MekForge.Avalonia/Assets";

    public Bitmap? GetImage(string assetType, string assetName)
    {
        var path = $"{AssetsBasePath}/{assetType.ToLower()}/{assetName.ToLower()}.png";
        return _cache.GetOrAdd(path, LoadImage);
    }

    private static Bitmap? LoadImage(string path)
    {
        try
        {
            var uri = new Uri(path);
            var asset =AssetLoader.Open(uri);
            return new Bitmap(asset);
        }
        catch (Exception e)
        {
            var t = e;
            return null;
        }
    }

    object? IImageService.GetImage(string assetType, string assetName)
    {
        return GetImage(assetType, assetName);
    }
}
