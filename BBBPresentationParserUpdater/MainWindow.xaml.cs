using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BBBPresentationParserUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            CheckUpdateAsync();
        }

        public async void CheckUpdateAsync()
        {
            var rel = await Github.GetLatestRelease("Dixter-TES", "BigBlueButtonPresentationParser");

            if (rel == null)
                return;

            var eqResult = EqualsTag(rel.TagName);
            if (!eqResult.equals)
            {
                descTb.Text = ParseDescription(rel.Body);
                Visibility = Visibility.Visible;
            }
            else
            {
                if(eqResult.startupExe is not null)
                    Process.Start(eqResult.startupExe);

                Close();
                return;
            }
        }

        private (bool equals, string? startupExe) EqualsTag(string tag)
        {
            if (!File.Exists("config"))
                return (false, null);

            string[] lines = File.ReadAllLines("config");

            if(lines.Length > 1 && lines[0] == tag)
                return (true, lines[1]);

            return (false, null);
        }

        public static string? GetStartupExe()
        {
            if (!File.Exists("config"))
                return null;

            string[] lines = File.ReadAllLines("config");

            if (lines.Length > 1)
                return lines[1];
            else
                return null;
        }

        private string ParseDescription(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            int startIndex = text.IndexOf("#==") + 3;
            int endIndex = text.IndexOf("==#");

            if (startIndex == -1 || endIndex == -1)
                return string.Empty;

            return text.Substring(startIndex, endIndex - startIndex);
        }

        private void installButton_Click(object sender, RoutedEventArgs e)
        {
            InstallWindow window = new InstallWindow();
            window.Show();
            Close();
        }

        private void laterButton_Click(object sender, RoutedEventArgs e)
        {
            string? exe = GetStartupExe();
            if (exe is not null)
                Process.Start(exe);

            Close();
        }
    }
}
