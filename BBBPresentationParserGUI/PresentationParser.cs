using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace BBBPresentationParserGUI
{
    internal class PresentationParser
    {
        private WebDriver? _browser;
        private readonly string _baseUrl;

        public EventHandler<string>? SlideParsed;

        public PresentationParser(string baseUrl, WebDriver driver)
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
                _browser?.Quit();
                _browser?.Dispose();
            }
            catch { }
        }

        public async Task<string[]?> Parse()
        {
            return await Task.Run(() =>
            {
                if (_browser == null)
                    return null;

                _browser.Manage().Window.Size = new System.Drawing.Size(3000, 3000);
                var scrs = new List<string>();

                int count = 0;
                for (int i = 1; i < 200; i++)
                {
                    try
                    {
                        _browser.Url = $@"{_baseUrl}{i}";
                        if (_browser.PageSource.Contains("404 Not Found"))
                            break;

                        count++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Произошла ошибка: " + ex.Message);
                        break;
                    }
                }

                for (int i = 1; i <= count; i++)
                {
                    try
                    {
                        _browser.Url = $@"{_baseUrl}{i}";
                        if (_browser.PageSource.Contains("404 Not Found"))
                            break;

                        var slideBody = _browser.FindElement(By.TagName("svg"));
                        int width = slideBody.Size.Width + slideBody.Location.X * 2;
                        int height = slideBody.Size.Height + slideBody.Location.Y;

                        _browser.Manage().Window.Size = new System.Drawing.Size(width + 10, height + 50);

                        var scr = _browser.GetScreenshot();
                        string imgPath = $"{DateTime.Now.Ticks + i}.jpg";
                        scr.SaveAsFile(imgPath);
                        scrs.Add(imgPath);
                        SlideParsed?.Invoke(null, imgPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Произошла ошибка: " + ex.Message);
                        return null;
                    }
                }

                _browser.Close();
                return scrs.ToArray();
            });
        }
    }
}
