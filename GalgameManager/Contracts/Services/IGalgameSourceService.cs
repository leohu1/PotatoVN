﻿using GalgameManager.Models;
using GalgameManager.Models.BgTasks;
using GalgameManager.Models.Sources;

namespace GalgameManager.Contracts.Services;

/// <summary>
/// 物理层面操作不同源游戏的接口
/// </summary>
public interface IGalgameSourceService
{
    /// <summary>
    /// 将游戏移入某个库，应该直接返回一个BgTaskBase（SourceMoveTaskBase）实例
    /// </summary>
    /// <param name="target">目标库</param>
    /// <param name="game">游戏</param>
    /// <param name="targetPath">目标路径，若为null则表示服务可自行决定路径</param>
    public BgTaskBase MoveInAsync(GalgameSourceBase target, Galgame game, string? targetPath = null);

    /// <summary>
    /// 将游戏移出某个库，应该直接返回一个BgTaskBase（SourceMoveTaskBase）实例
    /// </summary>
    /// <param name="source"></param>
    /// <param name="game">游戏</param>
    public BgTaskBase MoveOutAsync(GalgameSourceBase source, Galgame game);

    /// <summary>
    /// 在库中保存游戏的Meta
    /// </summary>
    public Task SaveMetaAsync(Galgame game);

    /// <summary>
    /// 从游戏文件夹游戏Meta，若不存在则返回null
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <exception cref="PvnException">当.PotatoVN存在但meta.json不存在时抛出</exception>
    /// <returns></returns>
    public Task<Galgame?> LoadMetaAsync(string path);
    
    /// <summary>
    /// 获取库的（总空间，已用空间）（byte），若无法获取则返回(-1,-1)
    /// </summary>
    /// <param name="source"></param>
    public Task<(long total, long used)> GetSpaceAsync(GalgameSourceBase source);

    /// <summary>
    /// 获取移入描述 <br/>
    /// 该方法只会在移动游戏对话框中被调用
    /// </summary>
    /// <param name="source">源自哪个库</param>
    /// <param name="target">移动到哪个库</param>
    /// <param name="galgame">哪个游戏</param>
    /// <param name="moveInPath">移动到目录（与具体源有关）</param>
    /// <returns></returns>
    public string GetMoveInDescription(GalgameSourceBase source, GalgameSourceBase target, Galgame galgame,
        string? moveInPath=null);

    /// <summary>
    /// 获取将游戏移出某个库的描述 <br/>
    /// 该方法只会在移动游戏对话框中被调用
    /// </summary>
    /// <param name="source">源自哪个库</param>
    /// <param name="galgame">哪个游戏</param>
    /// <returns></returns>
    public string GetMoveOutDescription(GalgameSourceBase source, Galgame galgame);

    /// <summary>
    /// 检查移动操作是否合法，若合法返回null，否则返回错误信息 <br/>
    /// 该方法只会在移动游戏对话框中被调用
    /// </summary>
    /// <param name="moveIn">要移入的库</param>
    /// <param name="moveOut">要移出的库</param>
    /// <param name="galgame">要移动的游戏</param>
    public string? CheckMoveOperateValid(GalgameSourceBase? moveIn, GalgameSourceBase? moveOut, Galgame galgame);
}