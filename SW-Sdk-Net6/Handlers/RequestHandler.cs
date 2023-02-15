﻿using SW.Entities;
using SW.Helpers;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace SW.Handlers
{
    internal class RequestHandler<T> where T : Response, new()
    {
        private readonly ResponseHandler<T> _handler;
        internal RequestHandler()
        {
            _handler = new ResponseHandler<T>();
        }
        private async Task<T> PostResponseAsync(string url, string path, Dictionary<string, string> headers, HttpClientHandler proxy,
            HttpContent? content = null)
        {
            HttpResponseMessage result;
            try
            {
                using (HttpClient client = new(proxy, false))
                {
                    client.BaseAddress = new Uri(url);
                    client.Timeout = TimeSpan.FromSeconds(180);
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                    result = await client.PostAsync(path, content);
                }
                return await _handler.TryGetResponseAsync(result);
            }
            catch (HttpRequestException ex)
            {
                return _handler.GetExceptionResponse(ex);
            }
        }
        private async Task<T> GetResponseAsync(string url, string path, Dictionary<string, string> headers, HttpClientHandler proxy)
        {
            HttpResponseMessage result;
            try
            {
                using (HttpClient client = new(proxy, false))
                {
                    client.BaseAddress = new Uri(url);
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                    result = await client.GetAsync(path);
                }
                return await _handler.TryGetResponseAsync(result);
            }
            catch (HttpRequestException ex)
            {
                return _handler.GetExceptionResponse(ex);
            }
        }
        /// <summary>
        /// POST No Body.
        /// </summary>
        /// <param name="url">Base Url.</param>
        /// <param name="path">Path.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="proxy">Proxy settings.</param>
        /// <returns></returns>
        internal async Task<T> PostAsync(string url, string path, Request request)
        {
            return await PostResponseAsync(url, path, request.Headers, request.Proxy);
        }
        /// <summary>
        /// POST JSON, accepts custom Content-Type.
        /// </summary>
        /// <param name="url">Base Url.</param>
        /// <param name="path">Path.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="proxy">Proxy settings.</param>
        /// <param name="content">Json String.</param>
        /// <param name="contentType">Custom Content-Type, default: application/json</param>
        /// <returns></returns>
        internal async Task<T> PostAsync(string url, string path, Request request, string content, string contentType = null)
        {
            var setContent = new StringContent(content, Encoding.UTF8, contentType ?? Application.Json);
            return await PostResponseAsync(url, path, request.Headers, request.Proxy, setContent);
        }
        /// <summary>
        /// POST Multipart Form.
        /// </summary>
        /// <param name="url">Base Url.</param>
        /// <param name="path">Path.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="proxy">Proxy settings.</param>
        /// <param name="content">File Bytes.</param>
        /// <returns></returns>
        internal async Task<T> PostAsync(string url, string path, Request request, byte[] content)
        {
            var setContent = new MultipartFormDataContent();
            var data = new ByteArrayContent(content);
            setContent.Add(data, "xml", "xml");
            return await PostResponseAsync(url, path, request.Headers, request.Proxy, setContent);
        }
        /// <summary>
        /// GET
        /// </summary>
        /// <param name="url">Base Url.</param>
        /// <param name="path">Path.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="proxy">Proxy settings.</param>
        /// <returns></returns>
        internal async Task<T> GetAsync(string url, string path, Request request)
        {
            return await GetResponseAsync(url, path, request.Headers, request.Proxy);
        }
        internal T HandleException(Exception ex)
        {
            return _handler.GetExceptionResponse(ex);
        }
    }
}