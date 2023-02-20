using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BBBPresentationParser
{
    internal class DownloadManager
    {
        public struct DownloadResult
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

        public EventHandler<DownloadResult>? DownloadCompleted;
        public EventHandler<DownloadResult>? DownloadFailed;
        public EventHandler<string>? SlideDownloaded;

        public async void InitializeDownloadAsync(string url)
        {
            WebDriver? driver = null;

            try
            {
                driver = await Task.Run(() => DriverSetup.GetSupportedDriver(true));
            }
            catch (Exception ex)
            {
                driver?.Quit();
                driver?.Dispose();

                string errorMessage = "Произошла ошибка при проверка совместимости: " + ex.Message;
                DownloadFailed?.Invoke(null, new DownloadResult(false, errorMessage));
            }


            if (driver == null)
            {
                DownloadFailed?.Invoke(null, new DownloadResult(false, "Не найдено ни одного поддерживаемого браузера!"));
            }

            try
            {
                PresentationParser parser = new PresentationParser(url, driver);
                parser.SlideParsed += (sender, args) => SlideDownloaded?.Invoke(sender, args);
                string[]? slides = await parser.Parse();

                await Task.Delay(1000);

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
                driver?.Quit();
                driver?.Dispose();
            }
        }
    }
}
