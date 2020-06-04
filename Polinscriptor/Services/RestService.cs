using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polinscriptor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Polinscriptor.Services
{
    class RestService
    {
        private HttpWebRequest CreateRequest(string method, string url, IDictionary<string,string> requestHeaders)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = method;
            if (requestHeaders != null)
                foreach (KeyValuePair<string, string> kv in requestHeaders)
                    req.Headers.Add(kv.Key, kv.Value);
            
            return req;
        }

        private async Task<HttpWebResponse> GetResponseAsync(HttpWebRequest req)
        {
            try
            {
                return await req.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                return ex.Response as HttpWebResponse;
            }
        }

        private async Task WriteContentAsync(HttpWebRequest req, string content, string contentType)
        {
            if (content == null)
                return;

            req.ContentType = contentType;
            req.Headers.Add(HttpRequestHeader.ContentEncoding, Encoding.UTF8.HeaderName);
            using (BufferedStream w = new BufferedStream(await req.GetRequestStreamAsync()))
            {
                byte[] data = Encoding.UTF8.GetBytes(content);
                await w.WriteAsync(data, 0, data.Length);
            }
        }

        private async Task<string> ReadContentAsync(HttpWebResponse response)
        {
            using (StreamReader r = new StreamReader(response.GetResponseStream()))
            {
                return await r.ReadToEndAsync();
            }
        }


        public async Task<APIAnswer> PostRequest(string query, string jsonContent, IDictionary<string,string> requestHeaders = null)
        {
            try
            {
                HttpWebRequest req = CreateRequest(HttpMethod.Post.Method, query, requestHeaders);
                await WriteContentAsync(req, jsonContent, "application/json");
                var response = await GetResponseAsync(req);
                var resContent = await ReadContentAsync(response);
                try
                {
                    JObject jsonResponse = JObject.Parse(resContent);
                    return new APIAnswer(response.StatusCode, jsonResponse);

                } catch(JsonReaderException e)
                {
                    return new APIAnswer(response.StatusCode, null);
                }
            } catch(Exception e)
            {
                throw new Exception("Errore sconosciuto");
            }
        }
        
    }
}
