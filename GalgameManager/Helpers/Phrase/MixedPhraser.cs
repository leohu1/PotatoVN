﻿using System.Reflection;
using GalgameManager.Contracts.Phrase;
using GalgameManager.Enums;
using GalgameManager.Models;
using Newtonsoft.Json.Linq;

namespace GalgameManager.Helpers.Phrase;

public class MixedPhraser : IGalInfoPhraser
{
    private readonly BgmPhraser _bgmPhraser;
    private readonly VndbPhraser _vndbPhraser;
    private readonly List<string> _developerList = new();
    private bool _init;
    private const string ProducerFile = @"Assets\Data\producers.json";

    
    private async Task InitAsync()
    {
        _init = true;
        Assembly assembly = Assembly.GetExecutingAssembly();
        var file = Path.Combine(Path.GetDirectoryName(assembly.Location)!, ProducerFile);
        if (!File.Exists(file)) return;

        JToken json = JToken.Parse(await File.ReadAllTextAsync(file));
        List<JToken>? producers = json.ToObject<List<JToken>>();
        producers!.ForEach(dev =>
        {
            if (!string.IsNullOrEmpty(dev["name"]!.ToString()))
                _developerList.Add(dev["name"]!.ToString());
            if (!string.IsNullOrEmpty(dev["latin"]!.ToString()))
                _developerList.Add(dev["latin"]!.ToString());
            if (!string.IsNullOrEmpty(dev["alias"]!.ToString()))
            {
                var tmp = dev["alias"]!.ToString();
                _developerList.AddRange(tmp.Split("\n"));
            }
        });
    }
    
    private string? GetDeveloperFromTags(Galgame galgame)
    {
        string? result = null;
        foreach (var tag in galgame.Tags.Value!)
        {
            double maxSimilarity = 0;
            foreach(var dev in _developerList)
                if (IGalInfoPhraser.Similarity(dev, tag) > maxSimilarity)
                {
                    maxSimilarity = IGalInfoPhraser.Similarity(dev, tag);
                    result = dev;
                }

            if (result != null && maxSimilarity > 0.75) // magic number: 一个tag和开发商的相似度大于0.75就认为是开发商
                break;
        }
        return result;
    }
    
    public MixedPhraser(BgmPhraser bgmPhraser, VndbPhraser vndbPhraser)
    {
        _bgmPhraser = bgmPhraser;
        _vndbPhraser = vndbPhraser;
    }
    
    public async Task<Galgame?> GetGalgameInfo(Galgame galgame)
    {
        if (_init == false)
            await InitAsync();
        Galgame? bgm = new(), vndb = new();
        bgm.Name = galgame.Name;
        vndb.Name = galgame.Name;
        // 试图从Id中获取bgmId和vndbId
        try
        {
            (string? bgmId, string ? vndbId) tmp = TryGetId(galgame.Id);
            if (tmp.bgmId != null)
            {
                bgm.RssType = RssType.Bangumi;
                bgm.Id = tmp.bgmId;
            }
            if (tmp.vndbId != null)
            {
                vndb.RssType = RssType.Vndb;
                vndb.Id = tmp.vndbId;
            }
        }
        catch (Exception)
        {
            // ignored
        }
        // 从bgm和vndb中获取信息
        bgm = await _bgmPhraser.GetGalgameInfo(bgm);
        vndb = await _vndbPhraser.GetGalgameInfo(vndb);
        if(bgm == null && vndb == null)
            return null;
        
        // 合并信息
        Galgame result = new()
        {
            RssType = RssType.Mixed,
            Id = $"bgm:{(bgm == null ? "null" : bgm.Id)},vndb:{(vndb == null ? "null" : vndb.Id)}",
            // name
            Name = vndb != null ? vndb.Name : bgm!.Name
        };

        // Chinese name
        if (bgm != null && !string.IsNullOrEmpty(bgm.CnName))result.CnName =  bgm.CnName;
        else if (vndb != null && !string.IsNullOrEmpty(vndb.CnName)) result.CnName = vndb.CnName;
        else result.CnName = "";

        // description
        result.Description = bgm != null ? bgm.Description : vndb!.Description;
        
        // developer
        if (bgm != null && bgm.Developer != Galgame.DefaultString)result.Developer = bgm.Developer;
        else if (vndb != null && vndb.Developer != Galgame.DefaultString)result.Developer = vndb.Developer;
        
        // expectedPlayTime
        if(vndb != null)result.ExpectedPlayTime = vndb.ExpectedPlayTime;
        // rating
        result.Rating = bgm != null ? bgm.Rating : vndb!.Rating;
        // imageUrl
        result.ImageUrl = vndb != null ? vndb.ImageUrl : bgm!.ImageUrl;
        // tags
        //todo: mix Bgm's and Vndb's tag 
        result.Tags = bgm != null ? bgm.Tags : vndb!.Tags;
        
        // developer from tag
        if (result.Developer == Galgame.DefaultString)
        {
            var tmp = GetDeveloperFromTags(result);
            if (tmp != null)
                result.Developer = tmp;
        }
        return result;
    }

    public static (string? bgmId, string? vndbId) TryGetId(string? id)  //id: bgm:xxx,vndb:xxx
    {
        if (id == null || id.Contains("bgm:") == false || id.Contains(",vndb:") == false)
            return (null, null);
        id = id.Replace("bgm:", "").Replace("vndb:", "").Replace(" ","");
        id = id.Replace("，", ","); //替换中文逗号为英文逗号
        var tmp = id.Split(",").ToArray();
        string? bgmId = null, vndbId = null;
        if (tmp[0] != "null") bgmId = tmp[0];
        if (tmp[1] != "null") vndbId = tmp[1];
        return (bgmId, vndbId);
    }

    public RssType GetPhraseType() => RssType.Mixed;
}

public class MixedPhraserData : IGalInfoPhraserData
{
}