using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace UmbracoAI.Web.Seeding.Content;

/// <summary>
/// Imports the images from <c>SeedData/images</c> into the media library (folder "Starter").
/// Optional: without images the starter content is created without pictures.
/// </summary>
public sealed class MediaSeeder(
    IMediaService mediaService,
    MediaFileManager mediaFileManager,
    MediaUrlGeneratorCollection mediaUrlGenerators,
    IShortStringHelper shortStringHelper,
    IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
    IWebHostEnvironment hostingEnvironment,
    ILogger<MediaSeeder> logger)
{
    public const string FolderName = "Starter";

    /// <summary>Returns the imported images keyed by file name without extension (e.g. "hero").</summary>
    public IReadOnlyDictionary<string, IMedia> ImportAll()
    {
        string sourceDirectory = Path.Combine(hostingEnvironment.ContentRootPath, "SeedData", "images");
        string[] files = Directory.Exists(sourceDirectory)
            ? Directory.EnumerateFiles(sourceDirectory).Where(IsImage).Order().ToArray()
            : [];
        if (files.Length == 0)
        {
            logger.LogInformation("Seed: no images in {Directory} – starter content without pictures", sourceDirectory);
            return new Dictionary<string, IMedia>();
        }

        IMedia folder = mediaService.CreateMedia(FolderName, Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
        mediaService.Save(folder);
        return files.ToDictionary(file => Path.GetFileNameWithoutExtension(file), file => ImportImage(file, folder.Id));
    }

    private IMedia ImportImage(string filePath, int folderId)
    {
        string fileName = Path.GetFileName(filePath);
        IMedia media = mediaService.CreateMedia(Path.GetFileNameWithoutExtension(fileName), folderId, Constants.Conventions.MediaTypes.Image);

        using (FileStream stream = System.IO.File.OpenRead(filePath))
        {
            media.SetValue(mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider,
                Constants.Conventions.Media.File, fileName, stream);
        }

        var result = mediaService.Save(media);
        SeedGuard.Ensure(result.Success, $"saving media '{fileName}'", result.Result?.Result);
        return media;
    }

    private static bool IsImage(string path) =>
        Path.GetExtension(path).ToLowerInvariant() is ".jpg" or ".jpeg" or ".png" or ".webp";
}
