using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading;
using Image = iTextSharp.text.Image;
using OpenQA.Selenium;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace BBBPresentationParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Presentation pres = new Presentation();
            //try
            //{
            //    pres.Save(@"C:\Users\artem\Desktop\p1.bbbp");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            versionLb.Content += Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3) ?? "0.0.0";
        }

        private void TitleLb_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) => Close();

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = urlInputTb.Text;
            if (!ValidateUrl(ref url))
                return;

            var token = ChangeControlState(false);
            ImageSource defaultImage = previewImg.ImageSource;

            DownloadManager downloadManager = new DownloadManager();

            downloadManager.DownloadFailed += (sender, e) =>
            {
                AudioCenter.PlaySound(Properties.Resources.DownloadFailedSound);
                MessageBox.Show(e.ErrorMessage);
                token?.Cancel();
                ChangeControlState(true);
                previewImg.ImageSource = defaultImage;
            };

            downloadManager.DownloadCompleted += (sender, e) =>
            {
                AudioCenter.PlaySound(Properties.Resources.DownloadCompletedSound);

                try
                {
                    if (e.Content != null)
                        SavePresentationDialog(e.Content);
                    else
                        throw new ArgumentNullException($"{nameof(e.Content)} равен NULL!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка при сохранении: " + ex.Message);
                }
                finally
                {
                    token?.Cancel();
                    ChangeControlState(true);
                    previewImg.ImageSource = defaultImage;
                }
            };

            downloadManager.SlideDownloaded += (s, e) =>
            {
                previewImg.Dispatcher.Invoke(() =>
                {
                    var source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), e)));
                    previewImg.ImageSource = source;
                });
            };

            downloadManager.InitializeDownloadAsync(url);
        }

        private CancellationTokenSource? ChangeControlState(bool enabled)
        {
            downloadButton.Content = enabled ? "Скачать" : "Идёт скачивание...";
            downloadButton.IsEnabled = enabled;
            urlInputTb.IsEnabled = enabled;
            closeButton.IsEnabled = enabled;

            if (enabled)
                return null;

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            Animation.ButtonTextAnimation(downloadButton, new[]
            {
                "Идёт скачивание",
                "Идёт скачивание.",
                "Идёт скачивание..",
                "Идёт скачивание..."
            }, 500, token);

            return cancelTokenSource;
        }

        private static bool ValidateUrl(ref string url)
        {
            if (string.IsNullOrEmpty(url) ||
                !url.StartsWith("https://") ||
                (!url.Contains("webinar.bsu.edu.ru/") && !url.Contains("webinar.bsu-eis.ru/")))
                return false;

            string[] parts = url.Split('/');
            url = string.Join("/", parts.Take(parts.Length - 1).ToArray()) + "/";
            return true;
        }

        private static void SavePresentationDialog(Presentation presentation)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF-Файлы|*.pdf";
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if ((bool)saveDialog.ShowDialog()!)
            {
                presentation.SavePdf(saveDialog.FileName);
            }
        }
    }
}
