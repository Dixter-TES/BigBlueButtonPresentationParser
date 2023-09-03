using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BBBPresentationParserUpdater
{
    internal class Github
    {
        public static EventHandler<double>? OnDownloadProgressChanged;

        public static async Task<Release?> GetLatestRelease(string owner, string name)
        {
            IReadOnlyList<Release>? releases = await GetAllReleases(owner, name);
            return releases[0];
        }

        public static async Task<Release[]> GetAllReleases(string owner, string name)
        {
            var client = new GitHubClient(new ProductHeaderValue(nameof(BBBPresentationParserUpdater)));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(owner, name);
            return releases.ToArray();
        }

        [Obsolete]
        public static async Task<string?> DownloadLatestRelease(string owner, string name)
        {
            Release? release = await GetLatestRelease(owner, name);

            if (release == null)
                return null;

            string downloadPath = Path.GetFileName(release.Assets[0].BrowserDownloadUrl);
            WebClient client = new WebClient();
            client.DownloadProgressChanged += (sender, e) =>
            {
                OnDownloadProgressChanged?.Invoke(null, e.ProgressPercentage / 100d);
            };
            await client.DownloadFileTaskAsync(new Uri(release.Assets[0].BrowserDownloadUrl), downloadPath);

            return downloadPath;
        }
    }
}
