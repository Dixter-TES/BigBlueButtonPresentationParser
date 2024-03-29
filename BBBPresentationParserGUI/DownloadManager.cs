﻿using PuppeteerSharp;
using System;

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
        public EventHandler<byte[]>? SlideDownloaded;

        public async void InitializeDownloadAsync(string url)
        {
            IBrowser? driver = null;

            try
            {
                driver = await DriverSetup.GetSupportedDriver(true);
            }
            catch (Exception ex)
            {
                string errorMessage = "Произошла ошибка при получении драйвера: " + ex.Message;
                DownloadFailed?.Invoke(null, new DownloadResult(false, errorMessage));
                return;
            }

            try
            {
                PresentationParser parser = new PresentationParser(url, driver);
                parser.SlideParsed += (sender, args) => SlideDownloaded?.Invoke(sender, args);
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
    }
}
