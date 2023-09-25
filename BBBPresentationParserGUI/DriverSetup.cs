using BBBPresentationParser.Utils;
using PuppeteerSharp;
using System.Threading.Tasks;

namespace BBBPresentationParser
{
    internal class DriverSetup
    {
        public static async Task<IBrowser> GetSupportedDriver(bool headless)
        {
            using var browserFetcher = new BrowserFetcher();
            browserFetcher.DownloadProgressChanged += (s, e) =>
            {
                if (e.ProgressPercentage == 1)
                {
                    UIUtility.ShowMessage("Производится первоначальная настройка программы.\nЭто может занять некоторое время.\n\nОжидайте...");
                }
            };

            await browserFetcher.DownloadAsync();

            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = headless
            });
        }
    }
}
