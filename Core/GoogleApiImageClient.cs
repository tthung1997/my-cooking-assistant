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
using System.Collections.Specialized;

namespace Core
{
    public class GoogleApiImageClient : IImageClient
    {
        private readonly CustomsearchService service;
        private readonly string searchEngineId;

        private readonly string loggingSource;
        private readonly ILogger logger;

        public GoogleApiImageClient(string apiKey, string searchEngineId, ILogger logger, string loggingSource = "GoogleApiImageClient")
        {
            (logger == null).Throws(new ArgumentNullException(nameof(logger)));
            this.logger = logger;
            this.loggingSource = loggingSource;

            string.IsNullOrWhiteSpace(apiKey).Throws(new ArgumentNullException(nameof(apiKey)), this.logger, this.loggingSource);
            string.IsNullOrWhiteSpace(searchEngineId).Throws(new ArgumentNullException(nameof(searchEngineId)), this.logger, this.loggingSource);
            service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = apiKey
            });
            this.searchEngineId = searchEngineId;
        }

        public async Task<IEnumerable<DownloadedImage>> GetImages(string query, int count)
        {
            string.IsNullOrWhiteSpace(query).Throws(new ArgumentNullException(nameof(query)), logger, loggingSource);
            (count < 0).Throws(new ArgumentException(nameof(count)), logger, loggingSource);

            await logger.LogInfo(loggingSource, $"Generating request for {count} {"image".ToPlural(count > 1)} of {query}");
            var request = GetRequest(query, count);

            var images = new List<DownloadedImage>();
            if (count > 0)
            {
                try
                {
                    await logger.LogInfo(loggingSource, $"Executing request for {count} {"image".ToPlural(count > 1)} of {query}");
                    var response = await request.ExecuteAsync();
                    await logger.LogInfo(loggingSource, $"Response received. Generating results for {response.Items.Count} {"image".ToPlural(response.Items.Count > 1)}");

                    foreach (var item in response.Items)
                    {
                        var image = new DownloadedImage
                        {
                            Id = Guid.NewGuid(),
                            FileFormat = item.Mime,
                            Link = new Uri(item.Link),
                            DownloadedDateTime = DateTime.Now
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
