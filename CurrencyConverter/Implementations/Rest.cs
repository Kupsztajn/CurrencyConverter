namespace Lab1NBP.Implementations;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Lab1NBP.Interfaces;

public class Rest : IRemoteRepository
{
    private readonly HttpClient _httpClient;

    public Rest()
    {
        _httpClient = new HttpClient();
    }

    public async Task<byte[]> Get(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching data from {url}", ex);
        }
    }
}