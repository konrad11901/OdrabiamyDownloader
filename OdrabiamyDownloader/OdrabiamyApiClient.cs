using OdrabiamyDownloader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OdrabiamyDownloader
{
    public class OdrabiamyApiClient
    {
        public string WorkingDirectory { get; set; } = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public int SleepTime { get; set; } = 10000;


        private readonly string authCookie;
        private readonly HttpClient apiClient;
        private readonly HttpClient browserClient;

        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public OdrabiamyApiClient(string authCookie)
        {
            this.authCookie = authCookie;
            apiClient = new HttpClient
            {
                BaseAddress = new Uri("https://odrabiamy.pl/api/v1.3")
            };
            browserClient = new HttpClient
            {
                BaseAddress = new Uri("https://odrabiamy.pl")
            };

            apiClient.DefaultRequestHeaders.Add("authority", "odrabiamy.pl");
            apiClient.DefaultRequestHeaders.Add("browser-identifier", "e68e0081");
            apiClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36");
            apiClient.DefaultRequestHeaders.Add("accept", "*/*");
            apiClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            apiClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            apiClient.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9,pl-PL;q=0.8,pl;q=0.7,pt;q=0.6");
            apiClient.DefaultRequestHeaders.Add("cookie", authCookie);

            browserClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36");
            browserClient.DefaultRequestHeaders.Add("cookie", authCookie);
        }

        public async Task<int[]> InitializeAndGetPages(string subject, int bookId)
        {
            var response = await browserClient.GetAsync($"/{subject}/ksiazka-{bookId}");
            var responseString = await response.Content.ReadAsStringAsync();

            var versionBeginIndex = responseString.IndexOf("appVersion=") + 12;
            var versionEndIndex = responseString.IndexOf('\"', versionBeginIndex);
            var appVersion = responseString[versionBeginIndex..versionEndIndex];
            apiClient.DefaultRequestHeaders.Add("app-version", appVersion);
            Debug.WriteLine("appVersion: " + appVersion);

            var pagesBeginIndex = responseString.IndexOf("\"pages\"") + 9;
            var pagesEndIndex = responseString.IndexOf(']', pagesBeginIndex);
            Debug.WriteLine("Pages: " + responseString[pagesBeginIndex..pagesEndIndex]);
            return responseString[pagesBeginIndex..pagesEndIndex].Split(',').Select(int.Parse).ToArray();
        }

        public async Task DownloadPageSolutions(string subject, int bookId, int pageNumber, string bookName)
        {
            var savePath = Path.Combine(WorkingDirectory, bookName, pageNumber.ToString());
            Directory.CreateDirectory(savePath);
            await browserClient.GetAsync("https://odrabiamy.pl/{subject}/ksiazka-{bookId}/strona-{pageNumber}");
            var premiumResponse = await GetPremiumReponse(subject, bookId, pageNumber);

            foreach (var solution in premiumResponse.Solutions)
            {
                var fileName = "Zadanie " + solution.Number + ".html";
                int index = solution.Solution.IndexOf("img src=");
                while (index != -1)
                {
                    if (solution.Solution[index + 9] == '.')
                    {
                        solution.Solution = solution.Solution.Remove(index + 9, 2);
                        index--;
                    }
                    else if (solution.Solution[index + 9] == '/' && solution.Solution[index + 10] == '.')
                    {
                        solution.Solution = solution.Solution.Remove(index + 9, 3);
                        index--;
                    }
                    else if (solution.Solution[index + 9] != 'h')
                        solution.Solution = solution.Solution.Insert(index + 9, "https://odrabiamy.pl");
                    index = solution.Solution.IndexOf("img src=", index + 1);
                }
                await File.WriteAllTextAsync(Path.Combine(savePath, fileName), solution.Solution);
            }
        }

        public async Task Sleep()
        {
            await Task.Delay(SleepTime);
        }

        private async Task<PremiumResponse> GetPremiumReponse(string subject, int bookId, int pageNumber)
        {
            PremiumResponse premiumResponse = new PremiumResponse();
            apiClient.DefaultRequestHeaders.Referrer = new Uri($"https://odrabiamy.pl/{subject}/ksiazka-{bookId}/strona-{pageNumber}");
            var response = await apiClient.GetAsync($"https://odrabiamy.pl/api/v1.3/ksiazki/{bookId}/zadania/strona/{pageNumber}/premium");
            var responseStream = await response.Content.ReadAsStreamAsync();
            premiumResponse.Solutions = await JsonSerializer.DeserializeAsync<List<PremiumSolution>>(responseStream, serializerOptions);
            return premiumResponse;
        }
    }
}
