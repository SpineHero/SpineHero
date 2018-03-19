using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpineHero.Utils.CloudStorage
{
    public class SpineHeroWebApi : ICloudStorage
    {
        public readonly string ApiToken;
        public readonly string ApiUrl;

        public SpineHeroWebApi(string apiUrl, string apiToken)
        {
            ApiUrl = apiUrl;
            ApiToken = apiToken;
        }

        // API token is secret and should NOT be placed in config file. Advanced user will be capable to see it
        // even here with decompilation. Way to go is to use good obfuscation tool, that will encrypt strings. Still,
        // a hacker can break the obfuscation but it will be more complicated.
        public SpineHeroWebApi(string apiUrl) : this(apiUrl, "removed")
        { }

        public SpineHeroWebApi() : this(Properties.Settings.Default.SpineHeroApiUrl)
        { }

        public async Task<int> SaveData(string type, string data)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(ApiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", ApiToken);
                    var content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, object>()
                    {
                        {"type", type},
                        {"data", data}
                    }),
                        Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("logs/", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        throw new CloudStorageException(
                            $"Unable to send data. Status: {(int)response.StatusCode}, Content: {responseString}");
                    }
                }
                catch (Exception ex)
                {
                    throw new CloudStorageException("Exception caught: " + ex.Message);
                }
            }
            return 0;   // added only for tests :(
        }
    }
}