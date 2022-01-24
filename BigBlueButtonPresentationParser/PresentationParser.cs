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
            _baseUrl = baseUrl;
        }

        public string[] Parse()
        {
            EdgeOptions options = new EdgeOptions();
            options.AddArgument("--headless");

            _browser = new EdgeDriver(options);

            _browser.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
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

                    int width = _browser.FindElement(By.Id("surface1")).Size.Width;
                    int height = _browser.FindElement(By.Id("surface1")).Size.Height;
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
