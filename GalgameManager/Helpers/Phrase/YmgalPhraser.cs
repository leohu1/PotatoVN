﻿using GalgameManager.Contracts.Phrase;
using GalgameManager.Enums;
using GalgameManager.Models;
using Newtonsoft.Json;

namespace GalgameManager.Helpers.Phrase;

public class YmgalPhraser: IGalInfoPhraser
{
    private HttpClient _httpClient;
    public static string BaseUrl = "https://www.ymgal.games/";
    public static string PublicClientId = "ymgal";
    public static string PublicClientSecret = "luna0327";
    

    public YmgalPhraser()
    {
        _httpClient = new HttpClient();
        GetHttpClient();
    }

    public async Task<Galgame?> GetGalgameInfo(Galgame galgame)
    {
        var name = galgame.Name.Value ?? "";
        var url = "";
        int? id;
        try
        {
            if (galgame.RssType != RssType.Ymgal) throw new Exception();
            id = Convert.ToInt32(galgame.Id ?? "");
            url = BaseUrl + $"open/archive/?gid={id}";
        }
        catch (Exception)
        {
            url = BaseUrl + 
                  $"open/archive/search-game?mode=accurate&keyword={name}&similarity=50";
        }
        
    }

    public RssType GetPhraseType() => RssType.Ymgal;

    public async Task<string> OauthGet()
    {
        HttpResponseMessage request = await _httpClient.GetAsync(
            BaseUrl +
            $"oauth/token?grant_type=client_credentials&client_id={PublicClientId}&client_secret={PublicClientSecret}&scope=public");
        request.EnsureSuccessStatusCode();
        return JsonConvert.
            DeserializeObject<OauthRequest>
                (await request.Content.ReadAsStringAsync())?.access_token ?? "";
    }
    
    private void GetHttpClient()
    {
        var access_token = OauthGet().Result;
        _httpClient = Utils.GetDefaultHttpClient().WithApplicationJson();
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);
    }
}

public class OauthRequest
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
}

public class ApiResponse<T>
{
    public bool success { get; set; }
    public int code { get; set; }
    public string msg { get; set; }
    public T data { get; set; }
}



