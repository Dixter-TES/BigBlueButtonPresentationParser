using PuppeteerSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BBBPresentationParser
{
    internal class DriverSetup
    {
        public static async Task<IBrowser> GetSupportedDriver(bool headless)
        {
            using var browserFetcher = new BrowserFetcher();

            bool haveInstalledRevisions = browserFetcher.LocalRevisions().Any();
            if(!haveInstalledRevisions)
            {
                MessageBox.Show("Производится первоначальная настройка программы.\nЭто может занять пару минут.\n\nОжидайте...");
            }

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = headless
            });

        }
    }
}
