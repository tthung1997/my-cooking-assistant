using Google.Apis.Customsearch.v1;
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

        private readonly string loggingSource;
        private readonly ILogger logger;

        public GoogleApiImageClient(string apiKey, string searchEngineId, ILogger logger, string loggingSource = "GoogleApiImageClient")
        {
            service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = apiKey
            });
            this.searchEngineId = searchEngineId;
            this.logger = logger;
            this.loggingSource = loggingSource;
        }

        public async Task<IEnumerable<DownloadedImage>> GetImages(string query, int count)
        {
            await logger.LogInfo(loggingSource, $"Generating request for {count} image{(count > 1 ? "s" : "")} of {query}");
            var request = GetRequest(query, count);

            var images = new List<DownloadedImage>();
            try
            {
                await logger.LogInfo(loggingSource, $"Executing request for {count} image{(count > 1 ? "s" : "")} of {query}");
                var response = await request.ExecuteAsync();
                await logger.LogInfo(loggingSource, $"Response received. Generating results for {response.Items.Count} image{(response.Items.Count > 1 ? "s" : "")}");

                foreach (var item in response.Items)
                {
                    var image = new DownloadedImage
                    {
                        Id = Guid.NewGuid(),
                        FileFormat = item.Mime,
                        Link = new Uri(item.Link)
                    };

                    using (var webClient = new WebClient())
                    {
                        image.Content = webClient.DownloadData(image.Link);
                    }

                    images.Add(image);
                    await logger.LogInfo(loggingSource, image.ToString());
                }
            }
            catch (Exception e)
            {
                await logger.LogError(loggingSource, e);
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
    }
}
