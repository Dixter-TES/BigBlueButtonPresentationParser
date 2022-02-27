using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace BBBPresentationParserGUI
{
    internal class DriverSetup
    {
        public static async Task<WebDriver?> GetSupportedDriver(bool headless)
        {
            var result = CheckCopatibility();

            await Task.Delay(1000);

            if (result.copatibility)
                return null;

            ChromiumOptions chromiumOptions = new ChromeOptions();
            FirefoxOptions firefoxOptions = new FirefoxOptions();

            if (headless)
            {
                chromiumOptions.AddArgument("--headless");
                firefoxOptions.AddArgument("--headless");
            }

            if (result.driver == typeof(EdgeDriver))
            {
                var driverService = EdgeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                return new EdgeDriver(driverService, (EdgeOptions)chromiumOptions);
            }
            else if (result.driver == typeof(ChromeDriver))
            {
                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                return new ChromeDriver(driverService, (ChromeOptions)chromiumOptions);
            }
            else if (result.driver == typeof(FirefoxDriver))
            {
                var driverService = FirefoxDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                return new FirefoxDriver(driverService, firefoxOptions);
            }
            else
            {
                return null;
            }
               
        }

        public static (bool copatibility, Type? driver) CheckCopatibility()
        {
            string?[] browsers = GetBrowsers();
            foreach (var browser in browsers)
            {
                if (browser == null)
                    continue;

                string lowerBrowser = browser.ToLower();
                DriverManager manager = new DriverManager();

                if (lowerBrowser.Contains("edge"))
                {
                    manager.SetUpDriver(new EdgeConfig());
                    return (true, typeof(EdgeDriver));
                }
                else if (lowerBrowser.Contains("chrome"))
                {
                    manager.SetUpDriver(new ChromeConfig());
                    return (true, typeof(EdgeDriver));
                }
                else if (lowerBrowser.Contains("firefox"))
                {
                    manager.SetUpDriver(new FirefoxConfig());
                    return (true, typeof(FirefoxDriver));
                }
                else
                {
                    return (false, null);
                }
            }

            return (false, null);
        }

        private static string?[] GetBrowsers()
        {
            string browsersRegistryKeyPath = @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet";
            List<string?> browsers = new List<string?>();

            using (RegistryKey browsersKey = Registry.LocalMachine.OpenSubKey(browsersRegistryKeyPath)!)
            {
                foreach (string browserKeyName in browsersKey!.GetSubKeyNames())
                {
                    using (RegistryKey browserKey = browsersKey?.OpenSubKey(browserKeyName)!)
                    {
                        browsers.Add(browserKey?.GetValue(null)?.ToString());
                    }
                }
            }

            return browsers.ToArray();
        }
    }


}
