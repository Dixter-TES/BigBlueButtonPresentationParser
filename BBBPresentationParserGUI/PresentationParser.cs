using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace BBBPresentationParser
{
    internal class PresentationParser
    {
        private IBrowser _browser;
        private readonly string _baseUrl;

        public EventHandler<string>? SlideParsed;

        public PresentationParser(string baseUrl, IBrowser driver)
        {
            _baseUrl = baseUrl;
            _browser = driver;

            try
            {
                foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.jpg"))
                    File.Delete(file);

                foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.pdf"))
                    File.Delete(file);
            }
            catch { }
        }

        ~PresentationParser()
        {
            try
            {
                _browser?.CloseAsync();
                _browser?.Dispose();
            }
            catch { }
        }

        public async Task<string[]?> Parse()
        {
            var page = await _browser.NewPageAsync();
            var screenshots = new List<string>();

            for (int i = 1; i < 200; i++)
            {
                try
                {
                    await page.GoToAsync($@"{_baseUrl}{i}");

                    string content = await page.GetContentAsync();
                    if (content.Contains("404 Not Found"))
                        break;

                    var svgElement = await page.QuerySelectorAsync("svg");
                    var svgElementSize = await svgElement.BoundingBoxAsync();

                    int width = (int)(svgElementSize.Width + svgElementSize.X * 2);
                    int height = (int)(svgElementSize.Height + svgElementSize.Y);

                    await page.SetViewportAsync(new ViewPortOptions { Width = width + 10, Height = height + 50 });

                    string imgPath = $"{DateTime.Now.Ticks + i}.jpg";
                    await page.ScreenshotAsync(imgPath);
                    screenshots.Add(imgPath);
                    SlideParsed?.Invoke(null, imgPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка: " + ex.Message);
                    return null;
                }
            }

            await _browser.CloseAsync();
            return screenshots.ToArray();
        }
    }
}
