using BBBPresentationParser.Extensions;
using BBBPresentationParser.Utils;
using Microsoft.Win32;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            versionLb.Text += Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3) ?? "0.0.0";
        }

        private void TitleLb_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) => Close();

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = urlInputTb.Text;
            if (!ValidatorUtility.ValidatePresentationUrl(ref url))
            {
                ToolTip tooltip = new ToolTip { Content = "Введите ссылку на презентацию" };
                urlInputBorder.ToolTip = tooltip;
                tooltip.IsOpen = true;
                await Task.Delay(1500);
                tooltip.IsOpen = false;
                return;
            }
                
            var token = ChangeControlState(false);
            ImageSource defaultImage = previewImg.ImageSource;

            PresentationDownloader downloadManager = new PresentationDownloader();

            downloadManager.DownloadFailed += (sender, e) =>
            {
                AudioCenter.PlaySound(Properties.Resources.DownloadFailedSound);
                UIUtility.ShowMessage($"{e.ErrorMessage}\n\nВозможно вы ввели некорректную ссылку.");

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
                    var source = BitmapImageUtils.BitmapImageFromBytes(e);
                    previewImg.ImageSource = source;
                });
            };

            downloadManager.DownloadAsync(url);
        }

        private CancellationTokenSource? ChangeControlState(bool enabled)
        {
            downloadButton.IsEnabled = enabled;
            urlInputTb.IsEnabled = enabled;
            closeButton.IsEnabled = enabled;

            if (enabled)
                return null;

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

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
