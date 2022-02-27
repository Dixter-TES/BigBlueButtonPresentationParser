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

namespace BBBPresentationParserGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // https://mwebinar.bsu.edu.ru/bigbluebutton/presentation/d4e277644f0d0339cff9d4f2f13362ec5927c892-1645440566435/d4e277644f0d0339cff9d4f2f13362ec5927c892-1645440566435/e86dfad4f5acd918badeb55159d4b353352ceb24-1645441712126/svg/1

        public MainWindow()
        {
            InitializeComponent();
            versionLb.Content += Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3);
        }

        private void TitleLb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(urlInputTb.Text) || !urlInputTb.Text.StartsWith("https://") || !urlInputTb.Text.Contains("webinar.bsu.edu.ru/"))
                return;

            ChangeControlState(false);

            WebDriver? driver = null;

            try
            {
                driver = await Task.Run(() => DriverSetup.GetSupportedDriver(true));
            }
            catch(Exception ex)
            {
                MessageBox.Show("Произошла ошибка при проверка совместимости: " + ex.Message);
            }
            

            if (driver == null)
            {
                MessageBox.Show("Не найдено ни одного поддерживаемого браузера!");
                ChangeControlState(true);
                return;
            }


            string url = urlInputTb.Text;
            string[] parts = url.Split('/');
            url = string.Join("/", parts.Take(parts.Length - 1).ToArray()) + "/";

            try
            {
                PresentationParser parser = new PresentationParser(url, driver);
                ImageSource def = previewImg.ImageSource;
                parser.SlideParsed += (s, e) =>
                {
                    previewImg.Dispatcher.Invoke(() =>
                    {
                        var source = new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), e)));
                        previewImg.ImageSource = source;
                    });
                };

                string[]? images = await parser.Parse();
                previewImg.ImageSource = def;

                await Task.Delay(1000);

                if (images is null)
                {
                    ChangeControlState(true);
                    return;
                }

                string tempPdfFile = GeneratePresentationDocument(images);

                if (tempPdfFile is null)
                {
                    ChangeControlState(true);
                    return;
                }

                TrySavePresentation(tempPdfFile);
            }
            catch (Exception ex)
            {
                driver?.Quit();
                MessageBox.Show("Произошла ошибка: " + ex.Message);
            }
            finally
            {
                ChangeControlState(true);
            }
        }

        private void ChangeControlState(bool enabled)
        {
            downloadButton.Content = enabled ? "Скачать" : "Идёт скачивание...";
            downloadButton.IsEnabled = enabled;
            urlInputTb.IsEnabled = enabled;
            closeButton.IsEnabled = enabled;
        }

        private static void TrySavePresentation(string filename)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = filename;
            saveDialog.Filter = "PDF-Файлы|*.pdf";
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if ((bool)saveDialog.ShowDialog()!)
                MoveFileAsync(filename, saveDialog.FileName);
            else
            {
                try
                {
                    File.Delete(filename);
                }
                catch { }
            }
                
        }

        private static async void MoveFileAsync(string source, string destination)
        {
            await Task.Run(() => {
                var bytes = File.ReadAllBytes(source);
                File.WriteAllBytes(destination, bytes);
                File.Delete(source);
            });
        }

        private static string GeneratePresentationDocument(string[] images)
        {
            var im = Image.GetInstance(images[0]);

            var doc = new Document(new Rectangle(im));
            string filename = $"Презентация {DateTime.Now:dd.MM.yy HH mm}.pdf";

            PdfWriter.GetInstance(doc, new FileStream(filename, FileMode.Create));
            doc.Open();

            foreach (string img in images)
            {
                Image i = Image.GetInstance(img);
                doc.SetPageSize(new Rectangle(i));
                i.Alignment = Element.ALIGN_CENTER;
                i.ScaleToFit(doc.PageSize.Width, doc.PageSize.Height);
                doc.Add(i);
                doc.NewPage();
                File.Delete(img);
            }
            doc.Close();
            return filename;
        }
    }
}
