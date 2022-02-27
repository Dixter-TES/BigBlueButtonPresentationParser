using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BBBPresentationParserUpdater
{
    /// <summary>
    /// Логика взаимодействия для InstallWindow.xaml
    /// </summary>
    public partial class InstallWindow : Window
    {
        private double _fullLoadingValue = 3;
        private double _loadingValue = 0;

        public InstallWindow()
        {
            InitializeComponent();
            UpdateLoadingBar();
            Install();
        }

        private void MoveDirectoryContent(string from, string to, params string[] withoutFiles)
        {
            string[] files = Directory.GetFiles(from);
            files = files.Where(f => !withoutFiles.Contains(System.IO.Path.GetFileName(f))).ToArray();

            string[] dirs = Directory.GetDirectories(from);

            if(!Directory.Exists(to))
                Directory.CreateDirectory(to);

            string tempDir = System.IO.Path.Combine(from, to);

            foreach (string file in files)
                File.Move(file, System.IO.Path.Combine(tempDir, System.IO.Path.GetFileName(file)), true);

            foreach (string dir in dirs)
                Directory.Move(dir, System.IO.Path.Combine(tempDir, System.IO.Path.GetFileName(dir)!));
        }

        private void Unpack(string package)
        {
            string[] oldDirs = Directory.GetDirectories(Directory.GetCurrentDirectory());
            ZipFile.ExtractToDirectory(package, Directory.GetCurrentDirectory());
            string packageFolder = Directory.GetDirectories(Directory.GetCurrentDirectory()).Where(x => !oldDirs.Contains(x)).First();

            MoveDirectoryContent(packageFolder, Directory.GetCurrentDirectory());
            Directory.Delete(packageFolder);
            File.Delete(package);                                                 
        }

        public async void Install()
        {
            statusTb.Text = $"Скачивание пакета...";
            Github.OnDownloadProgressChanged += (s, e) =>
            {
                _loadingValue = e;
                UpdateLoadingBar();
                Trace.WriteLine("1");
            };

            string? package = await Github.DownloadLatestRelease("Dixter-TES", "BigBlueButtonPresentationParser");
            installButton.IsEnabled = false;

            if (package == null)
            {
                statusTb.Text = $"Скачивание пакета: ошибка!";
                return;
            }

            statusTb.Text = $"Создание резервной копии...";
            MoveDirectoryContent(Directory.GetCurrentDirectory(), "Temp", package);
            _loadingValue += 1;
            UpdateLoadingBar();

            statusTb.Text = $"Распаковка пакета...";
            Unpack(package);
            _loadingValue += 1;
            UpdateLoadingBar();

            statusTb.Text = $"Удаление резервной копии...";
            MessageBox.Show("Установка завершена!");
            Process.Start("cmd", $"/c timeout 1 & rmdir /s /q Temp");
            Environment.Exit(0);
        }

        public void UpdateLoadingBar()
        {
            double val = MathExtension.InverseLerp(0, _fullLoadingValue, _loadingValue);
            val = val == 0 ? 0.001 : val;

            gr.Offset = val - 0.001;
            gr1.Offset = val;
            percentLb.Content = $"{(int)Math.Round(val * 100)}%";
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            string? exe = MainWindow.GetStartupExe();
            if (exe is not null)
                Process.Start(exe);

            Close();
        }
    }
}
