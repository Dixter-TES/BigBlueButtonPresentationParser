using BBBPresentationParser.Utils;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBBPresentationParser.PresentationUtils
{
    internal class PresentationViewer : IDisposable
    {
        private IBrowser _browser;
        private readonly string _baseUrl;

        private const string SlidesEndIndicator = "404 Not Found";

        public EventHandler<byte[]>? SlideParsed;

        private int _slidesCount = -1;

        public PresentationViewer(string baseUrl, IBrowser driver)
        {
            _baseUrl = baseUrl;
            _browser = driver;
        }

        ~PresentationViewer()
        {
            _browser?.CloseAsync();
        }

        public async Task<byte[]?> ParseSlideAsync(int slideId)
        {
            if (slideId >= _slidesCount && _slidesCount != -1)
                return null;

            var page = await _browser.NewPageAsync();

            await page.GoToAsync($@"{_baseUrl}{slideId}");
            
            string content = await page.GetContentAsync();
            if (content.Contains(SlidesEndIndicator))
            {
                _slidesCount = slideId;
                return null;
            }

            var svgElement = await page.QuerySelectorAsync("svg");
            var svgElementSize = await svgElement.BoundingBoxAsync();

            int width = (int)(svgElementSize.Width + svgElementSize.X * 2);
            int height = (int)(svgElementSize.Height + svgElementSize.Y);

            await page.SetViewportAsync(new ViewPortOptions { Width = width + 10, Height = height + 50 });

            var screenshot = await page.ScreenshotDataAsync();
            SlideParsed?.Invoke(null, screenshot);

            return screenshot;
        }

        public void Dispose()
        {
            _browser?.Dispose();
        }
    }
}
