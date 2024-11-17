/*
 * 游戏源移动任务，当进入托盘模式时由SourceMoveTask恢复
 */

using GalgameManager.Helpers;
using GalgameManager.Models.Sources;

namespace GalgameManager.Models.BgTasks;

public class ZipSourceMoveInTask : BgTaskBase
{
    private readonly Galgame _game;
    private readonly string _targetPath;
    
    public ZipSourceMoveInTask(Galgame game, string targetPath)
    {
        _game = game;
        _targetPath = targetPath;
    }
    
    protected override Task RecoverFromJsonInternal() => Task.CompletedTask;

    protected async override Task RunInternal()
    {
        await Task.CompletedTask;
        var originPath = _game.Sources.FirstOrDefault(s => s.SourceType == GalgameSourceType.LocalZip)
            ?.GetPath(_game);
        if (originPath is null) throw new PvnException("originPath is null");
        if (Utils.IsPathContained(originPath, _targetPath))
            throw new PvnException("TargetPath is contained in originPath");
        List<string> parts = Utils.GetZipParts(originPath);
        var num = parts.Count;

        for (var i = 0; i < num; i++)
        {
            File.Copy(parts[i], Path.Combine(_targetPath, Path.GetFileName(parts[i])));
            ChangeProgress(i, num, "ZipSourceMoveTask_MoveIn_Progress".GetLocalized(Path.GetFileName(parts[i])));
        }

        ChangeProgress(num, num, "ZipSourceMoveTask_MoveIn_Success".GetLocalized(_game.Name, _targetPath));
    }

    public override string Title { get; } = "ZipSourceMoveTask_MoveIn_Title".GetLocalized();
}

public class ZipSourceMoveOutTask : BgTaskBase
{
    private readonly Galgame _game;
    private readonly GalgameSourceBase _target;

    public ZipSourceMoveOutTask(Galgame game, GalgameSourceBase target)
    {
        _game = game;
        _target = target;
    }

    protected override Task RecoverFromJsonInternal() => Task.CompletedTask;

    protected async override Task RunInternal()
    {
        await Task.CompletedTask;
        var root = _target.GetPath(_game);
        if (root is null) throw new PvnException("root is null"); //不应该发生
        
        List<string> parts = Utils.GetZipParts(root);
        var num = parts.Count;

        for (var i = 0; i < num; i++)
        {
            File.Copy(parts[i], Path.Combine(_target.Url, Path.GetFileName(parts[i])));
            ChangeProgress(i, num, "ZipSourceMoveTask_MoveOut_Progress".GetLocalized(Path.GetFileName(parts[i])));
        }

        ChangeProgress(num, num, "ZipSourceMoveTask_MoveOut_Success".GetLocalized(_game.Name, _target.Name));
        
    }

    public override string Title { get; } = "ZipSourceMoveTask_MoveOut_Title".GetLocalized();
}