﻿using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace BBBPresentationParser
{
    internal class PresentationParser
    {
        private readonly IBrowser _browser;
        private readonly string _baseUrl;

        private const int MaxSlidesCount = 200;
        private const string SlidesEndIndicator = "404 Not Found";

        public EventHandler<byte[]>? SlideParsed;

        public PresentationParser(string baseUrl, IBrowser driver)
        {
            _baseUrl = baseUrl;
            _browser = driver;
        }

        ~PresentationParser()
        {
            _browser?.CloseAsync();
        }

        public async Task<byte[][]?> Parse()
        {
            var page = await _browser.NewPageAsync();
            var screenshots = new List<byte[]>();

            for (var i = 1; i < MaxSlidesCount; i++)
            {
                try
                {
                    await page.GoToAsync($@"{_baseUrl}{i}");

                    string content = await page.GetContentAsync();
                    if (content.Contains(SlidesEndIndicator))
                        break;

                    var svgElement = await page.QuerySelectorAsync("svg");
                    var svgElementSize = await svgElement.BoundingBoxAsync();

                    var width = (int)(svgElementSize.Width + svgElementSize.X * 2);
                    var height = (int)(svgElementSize.Height + svgElementSize.Y);

                    await page.SetViewportAsync(new ViewPortOptions { Width = width + 10, Height = height + 50 });

                    byte[] data = await page.ScreenshotDataAsync();
                    screenshots.Add(data);
                    SlideParsed?.Invoke(null, data);
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
