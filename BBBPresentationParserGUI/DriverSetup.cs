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
using System.Windows;
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

            if (!result.copatibility)
                return null;

            ChromeOptions chromeOptions = new ChromeOptions();
            FirefoxOptions firefoxOptions = new FirefoxOptions();
            EdgeOptions edgeOptions = new EdgeOptions();

            if (headless)
            {
                chromeOptions.AddArgument("--headless");
                firefoxOptions.AddArgument("--headless");
                edgeOptions.AddArgument("--headless");
            }

            if (result.driver == typeof(EdgeDriver))
            {
                var driverService = EdgeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                return new EdgeDriver(driverService, edgeOptions);
            }
            else if (result.driver == typeof(ChromeDriver))
            {
                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                return new ChromeDriver(driverService, chromeOptions);
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
            string[]? browsers = GetBrowsers();

            if (browsers is null)
                return (false, null);

            foreach (var browser in browsers)
            {
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
                    return (true, typeof(ChromeDriver));
                }
                else if (lowerBrowser.Contains("firefox"))
                {
                    manager.SetUpDriver(new FirefoxConfig());
                    return (true, typeof(FirefoxDriver));
                }
            }

            return (false, null);
        }

        private static string[]? GetBrowsers()
        {
            var osVer = Environment.OSVersion.Version;
            string browsersRegistryKeyPath = @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet";

            if (osVer.Major == 6 && osVer.Minor == 1)
                browsersRegistryKeyPath = @"SOFTWARE\Wow6432Node\Clients\StartMenuInternet";

            List<string> browsers = new List<string>();

            using (RegistryKey browsersKey = Registry.LocalMachine.OpenSubKey(browsersRegistryKeyPath)!)
            {
                foreach(var browser in browsersKey.GetSubKeyNames())
                {
                    using (RegistryKey browserKey = browsersKey.OpenSubKey(browser + @"\shell\open\command")!)
                    {
                        if (!string.IsNullOrWhiteSpace(browserKey.GetValue(null)?.ToString()))
                            browsers.Add(browser);
                    }
                }
            }

            return browsers.ToArray();
        }
    }


}
