using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace eBEST.OpenApi.DevCenter.Helpers
{
    public class GithupRepoHelper
    {
        public record GithubBlobInfo(string sha, string node_id, int size, string url, string content, string encoding);
        public record GithubTagInfo(string html_url, string tag_name, string name, string published_at, string body);
        public record GithubTreeInfo(string path, string type, int size, string url);
        public record GihubRepoRecursive(string sha, string url, IList<GithubTreeInfo> tree);

        private readonly string _userName;
        private readonly string _repository;
        private readonly HttpClient _client;
        public GithupRepoHelper(string userName, string repository)
        {
            _userName = userName;
            _repository = repository;
            _client = new();
            var pih = ProductInfoHeaderValue.Parse(repository);
            _client.DefaultRequestHeaders.UserAgent.Add(pih);
        }

        public async Task<IList<GithubTagInfo>?> GetTagInfosAsync()
        {
            return await _client.GetFromJsonAsync<List<GithubTagInfo>>($"https://api.github.com/repos/{_userName}/{_repository}/releases");
        }

        public async Task<GihubRepoRecursive?> GetTreeInfosAsync()
        {
            return await _client.GetFromJsonAsync<GihubRepoRecursive>($"https://api.github.com/repos/{_userName}/{_repository}/git/trees/master?recursive=1");
        }

        public async Task<string> GetBlobContentAsync(string uri)
        {
            try
            {
                var blobInfo = await _client.GetFromJsonAsync<GithubBlobInfo>(uri);
                if (blobInfo != null)
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(blobInfo.content));
                }
            }
            catch
            {
            }
            return string.Empty;
        }

        public async Task<Stream?> Download()
        {
            try
            {
                string url = $"https://api.github.com/repos/{_userName}/{_repository}/zipball/master";
                var data = await _client.GetStreamAsync(url);
                return data;
            }
            catch
            {
            }
            return null;
        }
    }
}
