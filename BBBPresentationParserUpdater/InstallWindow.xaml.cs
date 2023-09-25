using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

        private void TitleLb_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void MoveDirectoryContent(string from, string to, params string[] withoutFiles)
        {
            string[] files = Directory.GetFiles(from);
            files = files.Where(f => !withoutFiles.Contains(System.IO.Path.GetFileName(f))).ToArray();

            string[] dirs = Directory.GetDirectories(from);

            try
            {
                if (!Directory.Exists(to))
                {
                    Directory.CreateDirectory(to);
                }
                else
                {
                    Process.Start("cmd", $"/c timeout 1 & rmdir /s /q {to}");
                    Directory.CreateDirectory(to);
                }
            }
            catch { }


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
            string packageFolder = Directory.GetDirectories(Directory.GetCurrentDirectory()).First(x => !oldDirs.Contains(x));

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
            Process.Start("cmd", $"/c timeout 1 & rmdir /s /q Temp & start {MainWindow.GetStartupExe()}");
            Environment.Exit(0);
        }

        public void UpdateLoadingBar()
        {
            double val = MathExtension.InverseLerp(0, _fullLoadingValue, _loadingValue);
            val = val == 0 ? 0.001 : val;

            gr.Offset = val - 0.001;
            gr1.Offset = val;
            percentLb.Text = $"{(int)Math.Round(val * 100)}%";
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
