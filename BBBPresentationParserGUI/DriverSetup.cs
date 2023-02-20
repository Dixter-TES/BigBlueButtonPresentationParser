using PuppeteerSharp;
using System.Threading.Tasks;

namespace BBBPresentationParser
{
    internal class DriverSetup
    {
        public static async Task<IBrowser?> GetSupportedDriver(bool headless)
        {
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
        }
    }
}
