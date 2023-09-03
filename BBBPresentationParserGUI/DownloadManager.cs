using PuppeteerSharp;
using System;
using System.Threading.Tasks;

namespace BBBPresentationParser
{
    internal class PresentationDownloader
    {
        public EventHandler<DownloadResult>? DownloadCompleted;
        public EventHandler<DownloadResult>? DownloadFailed;
        public EventHandler<byte[]>? SlideDownloaded;

        public async void DownloadAsync(string url)
        {
            IBrowser? driver = await TryInitializeBrowser();

            if (driver is null)
                return;

            try
            {
                var parser = InitializePresentationParser(url, driver);
                byte[][]? slides = await parser.Parse();

                if (slides is null)
                {
                    DownloadFailed?.Invoke(null, new DownloadResult(false, "Не удалось скачать ни одного слайда!"));
                    return;
                }

                Presentation presentation = new Presentation();
                presentation.AddSlides(slides);

                DownloadCompleted?.Invoke(null, new DownloadResult(true, string.Empty, presentation));
            }
            catch (Exception ex)
            {
                DownloadFailed?.Invoke(null, new DownloadResult(false, "Произошла ошибка: " + ex.Message));
            }
            finally
            {
                driver?.CloseAsync();
            }
        }

        private async Task<IBrowser?> TryInitializeBrowser()
        {
            try
            {
                return await DriverSetup.GetSupportedDriver(true);
            }
            catch (Exception ex)
            {
                string errorMessage = "Произошла ошибка при получении драйвера: " + ex.Message;
                DownloadFailed?.Invoke(null, new DownloadResult(false, errorMessage));
            }

            return null;
        }

        private PresentationParser InitializePresentationParser(string url, IBrowser driver)
        {
            PresentationParser parser = new PresentationParser(url, driver);

            parser.SlideParsed += (sender, args) => SlideDownloaded?.Invoke(sender, args);
            return parser;
        }
    }

    internal struct DownloadResult
    {
        public readonly bool Status;
        public readonly string ErrorMessage;
        public Presentation? Content;

        public DownloadResult(bool status, string errorMessage)
        {
            Status = status;
            ErrorMessage = errorMessage;
            Content = null;
        }

        public DownloadResult(bool status, string errorMessage, Presentation content)
        {
            Status = status;
            ErrorMessage = errorMessage;
            Content = content;
        }
    }
}
