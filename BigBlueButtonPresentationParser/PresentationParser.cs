using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBlueButtonPresentationParser
{
    internal class PresentationParser
    {
        private EdgeDriver _browser;
        private readonly string _baseUrl;

        public PresentationParser(string baseUrl)
        {
            _baseUrl = baseUrl; // https://mwebinar.bsu.edu.ru/bigbluebutton/presentation/d4e277644f0d0339cff9d4f2f13362ec5927c892-1645440566435/d4e277644f0d0339cff9d4f2f13362ec5927c892-1645440566435/e86dfad4f5acd918badeb55159d4b353352ceb24-1645441712126/svg/6
        }

        public string[] Parse()
        {
            EdgeOptions options = new EdgeOptions();
            options.AddArgument("--headless");
            // options.BinaryLocation = @"EdgePortable\EdgePortable.exe";

            _browser = new EdgeDriver(options);

            _browser.Manage().Window.Size = new System.Drawing.Size(3000, 3000);
            var scrs = new List<string>();

            int count = 0;
            Program.WriteLine("Получаем количество слайдов...", ConsoleColor.Yellow);
            for (int i = 1; i < 100; i++)
            {
                try
                {
                    _browser.Url = $@"{_baseUrl}{i}";
                    if (_browser.PageSource.Contains("404 Not Found"))
                        break;

                    count++;
                }
                catch
                {
                    break;
                }
            }

            Program.WriteLine("Количество слайдов: " + count, ConsoleColor.Yellow);
            Program.WriteLine("Началось скачивание слайдов...", ConsoleColor.Yellow);
            LoadingBarVisualization vis = new LoadingBarVisualization();
            vis.FillColor = ConsoleColor.Red;
            LoadingBar downloadLoadingBar = new LoadingBar(1, count - 1, visualization: vis);
            Console.WriteLine();
            for (int i = 1; i <= count; i++)
            {
                try
                {
                    _browser.Url = $@"{_baseUrl}{i}";
                    if (_browser.PageSource.Contains("404 Not Found"))
                        break;

                    var slideBody = _browser.FindElement(By.Id("surface1"));
                    int width = slideBody.Size.Width + slideBody.Location.X * 2;
                    int height = slideBody.Size.Height + slideBody.Location.Y;
                    _browser.Manage().Window.Size = new System.Drawing.Size(width + 10, height + 50);
                    var scr = _browser.GetScreenshot();
                    string imgPath = $"{DateTime.Now.Ticks + i}.jpg";
                    scr.SaveAsFile(imgPath);
                    scrs.Add(imgPath);
                    downloadLoadingBar.Update(i);
                }
                catch (Exception ex)
                {
                    Program.WriteLine("Произошла ошибка при скачивании слайдов: " + ex.Message, ConsoleColor.Red);
                    return null;
                }
            }

            _browser.Close();
            return scrs.ToArray();
        }
    }
}
