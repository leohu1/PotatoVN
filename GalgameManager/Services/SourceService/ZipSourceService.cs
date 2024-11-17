using Windows.Storage;
using GalgameManager.Contracts.Services;
using GalgameManager.Core.Contracts.Services;
using GalgameManager.Helpers;
using GalgameManager.Models;
using GalgameManager.Models.BgTasks;
using GalgameManager.Models.Sources;

namespace GalgameManager.Services;

public class ZipSourceService : IGalgameSourceService
{
    private readonly IInfoService _infoService;
    private readonly IFileService _fileService;

    public ZipSourceService(IInfoService infoService, IFileService fileService)
    {
        _infoService = infoService;
        _fileService = fileService;
    }

    public BgTaskBase MoveInAsync(GalgameSourceBase target, Galgame game, string? targetPath = null)
    {
        if (targetPath is null) throw new PvnException("targetPath is null");
        if (target is not GalgameFolderSource && target is not GalgameZipSource ) 
            throw new ArgumentException("target is not GalgameFolderSource");
        if (target is GalgameZipSource)
        {
            return new ZipSourceMoveInTask(game, targetPath);
        }

        if (target is GalgameFolderSource folderSource)
        {
            return new PackGameTask(folderSource.Path, targetPath);
        }
        throw new PvnException("source is not supported");
    }

    public BgTaskBase MoveOutAsync(GalgameSourceBase source, Galgame game)
    {
        return new ZipSourceMoveOutTask(game, source);
    }

    public async Task SaveMetaAsync(Galgame game)
    {
        foreach (GalgameZipSource source in game.Sources.OfType<GalgameZipSource>())
        {
            var gamePath = source.GetPath(game)!;
            var folderPath = Directory.GetParent(gamePath)!.FullName;
            var metaPath = Path.Combine(folderPath, ".PotatoVN", Path.GetFileNameWithoutExtension(gamePath));
            if (!Directory.Exists(metaPath)) Directory.CreateDirectory(metaPath);
            Galgame meta = game.GetMetaCopy(metaPath);
            var destImagePath = Path.Combine(metaPath, meta.ImagePath.Value!);
            _fileService.Save(metaPath, "meta.json", meta);
            // 备份图片
            CopyImg(game.ImagePath.Value, destImagePath);
            foreach (GalgameCharacter character in game.Characters)
            {
                var destCharPreviewImagePath = Path.Combine(metaPath, Path.GetFileName(character.PreviewImagePath));
                var destCharImagePath = Path.Combine(metaPath, Path.GetFileName(character.ImagePath));
                CopyImg(character.PreviewImagePath, destCharPreviewImagePath);
                CopyImg(character.ImagePath, destCharImagePath);
            }
        }

        await Task.CompletedTask;
    }

    public async Task<Galgame?> LoadMetaAsync(string path)
    {
        await Task.CompletedTask;
        var folderPath = Directory.GetParent(path)!.FullName;
        var metaPath = Path.Combine(folderPath, ".PotatoVN", Path.GetFileNameWithoutExtension(path));
        if (!Directory.Exists(metaPath)) return null; // 不存在备份文件夹
        Galgame meta = _fileService.Read<Galgame>(metaPath, "meta.json")!;
        if (meta is null) throw new PvnException("meta.json not exist");
        if (meta.Path.EndsWith('\\')) meta.Path = meta.Path[..^1];
        meta.ImagePath.ForceSet(LoadImg(meta.ImagePath.Value, metaPath));
        foreach (GalgameCharacter character in meta.Characters)
        {
            character.ImagePath = LoadImg(character.ImagePath, metaPath)!;
            character.PreviewImagePath = LoadImg(character.PreviewImagePath, metaPath)!;
        }
        meta.UpdateIdFromMixed();
        meta.ExePath = LoadImg(meta.ExePath, metaPath, defaultReturn: null);
        meta.SavePath = Directory.Exists(meta.SavePath) ? meta.SavePath : null; //检查存档路径是否存在并设置SavePosition字段
        meta.FindSaveInPath();
        return meta;
    }

    public async Task<(long total, long used)> GetSpaceAsync(GalgameSourceBase source)
    {
        await Task.CompletedTask;
        try
        {
            DriveInfo? info = GetDriveInfo(source.Path);
            if (info is null) return (-1, -1);
            return (info.TotalSize, info.TotalSize - info.AvailableFreeSpace);
        }
        catch (Exception e)
        {
            _infoService.DeveloperEvent(msg: $"failed to get drive info with exception: {e}");
            return (-1, -1);
        }
    }

    public string GetMoveInDescription(GalgameSourceBase source, GalgameSourceBase target, Galgame galgame,
        string? moveInPath)
    {
        return "ZipSourceService_MoveInDescription".GetLocalized();
    }

    public string GetMoveOutDescription(GalgameSourceBase source, Galgame galgame)
    {
        var path = source.GetPath(galgame) ?? string.Empty;
        return "ZipSourceService_MoveOutDescription".GetLocalized(path);
    }

    public string? CheckMoveOperateValid(GalgameSourceBase? moveIn, GalgameSourceBase? moveOut, Galgame galgame)
    {
        if (moveIn?.SourceType == GalgameSourceType.LocalZip)
            return moveOut?.SourceType is GalgameSourceType.LocalFolder or GalgameSourceType.LocalZip
                ? null
                : "ZipSourceService_MoveOutError".GetLocalized();
        return null;
    }

    private static void CopyImg(string? src, string target)
    {
        if (src is null or Galgame.DefaultImagePath) return;
        if (!File.Exists(src)) return;
        if (File.Exists(target) && new FileInfo(target).Length == new FileInfo(src).Length) return; //文件已存在且大小相同就不复制
        File.Copy(src, target, true);
    }

    private static string? LoadImg(string? target, string path, string defaultTarget = Galgame.DefaultImagePath, 
        string? defaultReturn = Galgame.DefaultImagePath)
    {
        if (string.IsNullOrEmpty(target) || target == defaultTarget) return defaultReturn;
        var targetPath = Path.GetFullPath(Path.Combine(path, target));
        return File.Exists(targetPath) ? targetPath : defaultReturn;
    }
    
    private static DriveInfo? GetDriveInfo(string path)
    {
        var root = Path.GetPathRoot(path);
        return root is null ? null : new DriveInfo(root);
    }
}