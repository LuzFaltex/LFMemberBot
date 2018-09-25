using FoxBot.Core.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.Core.Services
{
    public class CodePasteService
    {
        private const string Header = @"
/*
    Written by: {0} in #{1}
    Posted on {2}
    Message ID: {3}
*/

{4}";

        private const string _ApiReferenceUrl = "https://paste.mod.gg/";
        private const string _FallbackApiReferenceUrl = "https://haste.charlesmilette.net/";
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        /// <summary>
        /// Uploads a given piece of code to the service, and returns the URL to the post
        /// </summary>
        /// <param name="code">The code to post</param>
        /// <param name="language"></param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCode(string code, string language = null)
        {
            var usingFallback = false;
            var content = FormatUtilities.ToHTMLSafeString(code);
            HttpResponseMessage response;

            try
            {
                response = await client.PostAsync($"{_ApiReferenceUrl}documents", content);
            }
            catch (TaskCanceledException)
            {
                usingFallback = true;
                response = await client.PostAsync($"{_FallbackApiReferenceUrl}documents", content);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content?.ReadAsStringAsync();
                throw new Exception($"{response.StatusCode} returned while calling {response.RequestMessage.RequestUri}. Response body: {body}");
            }

            var urlResponse = await response.Content.ReadAsStringAsync();
            var pasteKey = JObject.Parse(urlResponse)["key"].Value<string>();

            var domain = usingFallback ? _FallbackApiReferenceUrl : _ApiReferenceUrl;
            return $"{domain}{pasteKey}.{language ?? (FormatUtilities.GetCodeLanguage(code) ?? "cs")}";
        }
    }
}
