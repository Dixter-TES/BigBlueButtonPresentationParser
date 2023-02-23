using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Reflection;
using System.Threading;

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
            versionLb.Content += Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3) ?? "0.0.0";
        }

        private void TitleLb_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) => Close();

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = urlInputTb.Text;
            if (!ValidatorUtility.ValidatePresentationUrl(ref url))
                return;

            var token = ChangeControlState(false);
            ImageSource defaultImage = previewImg.ImageSource;

            DownloadManager downloadManager = new DownloadManager();

            downloadManager.DownloadFailed += (sender, e) =>
            {
                AudioCenter.PlaySound(Properties.Resources.DownloadFailedSound);
                MessageBox.Show(e.ErrorMessage);
                token?.Cancel();
                Thread.Sleep(200);
                ChangeControlState(true);
                previewImg.ImageSource = defaultImage;
            };

            downloadManager.DownloadCompleted += (sender, e) =>
            {
                AudioCenter.PlaySound(Properties.Resources.DownloadCompletedSound);
                previewImg.ImageSource = defaultImage;

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
                    Thread.Sleep(200);
                    ChangeControlState(true);
                    previewImg.ImageSource = defaultImage;
                }
            };

            downloadManager.SlideDownloaded += (s, e) =>
            {
                previewImg.Dispatcher.Invoke(() =>
                {
                    var source = BitmapImageUtils.BitmapImageFromData(e);
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

            Animation.ButtonTextAnimation(downloadButton, new[]
            {
                "Идёт скачивание",
                "Идёт скачивание.",
                "Идёт скачивание..",
                "Идёт скачивание..."
            }, 500, cancelTokenSource);

            return cancelTokenSource;
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
