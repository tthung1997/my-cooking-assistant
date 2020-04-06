﻿using Google.Apis.Customsearch.v1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Services;
using static Google.Apis.Customsearch.v1.CseResource;
using System.Net;
using System.Globalization;
using System.IO;
using Common;

namespace Core
{
    public interface IImageClient
    {
        Task<IEnumerable<DownloadedImage>> GetImages(string query, int count);
    }

    public class GoogleApiImageClient : IImageClient
    {
        private readonly CustomsearchService service;
        private readonly string searchEngineId;

        private readonly ILogger logger;

        public GoogleApiImageClient(string apiKey, string searchEngineId, ILogger logger)
        {
            service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = apiKey
            });
            this.searchEngineId = searchEngineId;
            this.logger = logger;
        }

        public async Task<IEnumerable<DownloadedImage>> GetImages(string query, int count)
        {
            await logger.LogInfo($"Generating request for {count} image{(count > 1 ? "s" : "")} of {query}");
            var request = GetRequest(query, count);

            var images = new List<DownloadedImage>();
            try
            {
                await logger.LogInfo($"Executing request for {count} image{(count > 1 ? "s" : "")} of {query}");
                var response = await request.ExecuteAsync();
                await logger.LogInfo($"Response received. Generating results for {response.Items.Count} image{(response.Items.Count > 1 ? "s" : "")}");

                var order = 0;
                foreach (var item in response.Items)
                {
                    var image = new DownloadedImage
                    {
                        Name = GetImageName(query, ++order),
                        FileFormat = item.Mime,
                        Link = new Uri(item.Link)
                    };

                    using (var webClient = new WebClient())
                    {
                        image.Content = webClient.DownloadData(image.Link);
                    }

                    images.Add(image);
                    await logger.LogInfo(image.ToString());
                }
            }
            catch (Exception e)
            {
                await logger.LogError(e);
            }

            return images;
        }

        private ListRequest GetRequest(string query, int count)
        {
            var request = service.Cse.List(query);
            request.Cx = searchEngineId;
            request.Num = count;
            request.SearchType = ListRequest.SearchTypeEnum.Image;
            request.Safe = ListRequest.SafeEnum.Active;
            return request;
        }

        private string GetImageName(string query, int order)
        {
            var imgName = query;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                imgName.Replace($"{c}", "");
            }
            imgName = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(query.ToLower()).Replace(" ", "")}-{order}";
            return imgName;
        }
    }
}
