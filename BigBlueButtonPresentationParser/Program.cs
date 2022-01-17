using iTextSharp.text;
using iTextSharp.text.pdf;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BigBlueButtonPresentationParser
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("Введите базовую ссылку: ");
            string url = Console.ReadLine();

            WriteLine("Запускаем виртуальный браузер...", ConsoleColor.Yellow);
            Console.CursorVisible = false;

            string tempPdfFile = ParseAndGeneratePresentation(url);
            WriteLine("Выберите место для сохранения PDF-файла", ConsoleColor.Yellow);
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = tempPdfFile;
            saveDialog.Filter = "PDF-Файлы|*.pdf";
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MoveFileAsync(tempPdfFile, saveDialog.FileName);
                WriteLine("PDF-файл сохранён", ConsoleColor.Yellow);
            }
            else
            {
                WriteLine("Сохранение файла отменено", ConsoleColor.Yellow);
            }

            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
            Environment.Exit(0);
            Process.GetCurrentProcess().Kill();
        }

        private static async void MoveFileAsync(string source, string destination)
        {
            await Task.Run(() => File.Move(source, destination));
        }

        private static string ParseAndGeneratePresentation(string url)
        {
            EdgeOptions otis = new EdgeOptions();
            otis.AddArgument("--headless");
            var browser = new EdgeDriver(otis);

            browser.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
            var scrs = new List<string>();

            int count = 0;
            WriteLine("Получаем количество слайдов...", ConsoleColor.Yellow);
            for (int i = 1; i < 100; i++)
            {
                try
                {
                    browser.Url = $@"{url}{i}";
                    if (browser.PageSource.Contains("404 Not Found"))
                        break;

                    count++;
                }
                catch
                {
                    break;
                }
            }

            WriteLine("Количество слайдов: " + count, ConsoleColor.Yellow);
            WriteLine("Началось скачивание слайдов...", ConsoleColor.Yellow);
            LoadingBar downloadLoadingBar = new LoadingBar(1, count - 1, 50, ConsoleColor.Red);
            Console.WriteLine();
            for (int i = 1; i < count; i++)
            {
                try
                {
                    browser.Url = $@"{url}{i}";
                    if (browser.PageSource.Contains("404 Not Found"))
                        break;

                    int width = browser.FindElement(By.Id("surface1")).Size.Width;
                    int height = browser.FindElement(By.Id("surface1")).Size.Height;
                    browser.Manage().Window.Size = new System.Drawing.Size(width + 10, height + 50);
                    var scr = browser.GetScreenshot();
                    string imgPath = $"{DateTime.Now.Ticks + i}.jpg";
                    scr.SaveAsFile(imgPath);
                    scrs.Add(imgPath);
                    downloadLoadingBar.Update(i);
                }
                catch
                {
                    break;
                }
            }

            browser.Close();
            Thread.Sleep(1);

            if (scrs.Count() == 0)
                return null;

            WriteLine("Объединяем слайды в PDF-файл...", ConsoleColor.Yellow);
            LoadingBar savingLoadingBar = new LoadingBar(1, count, 50, ConsoleColor.Cyan);
            Console.WriteLine();

            Image im = Image.GetInstance(scrs[0]);
            var doc = new Document(new Rectangle(im));
            string filename = $"{DateTime.Now.Ticks}.pdf";
            PdfWriter.GetInstance(doc, new FileStream(filename, FileMode.Create));
            doc.Open();

            Thread.Sleep(1);
            foreach (string img in scrs)
            {
                Image i = Image.GetInstance(img);
                doc.SetPageSize(new Rectangle(i));
                i.Alignment = Element.ALIGN_CENTER;
                i.ScaleToFit(doc.PageSize.Width, doc.PageSize.Height);
                doc.Add(i);
                doc.NewPage();
                File.Delete(img);
                savingLoadingBar.Update(savingLoadingBar.GetValue() + 1);
            }
            doc.Close();
            return filename;
        }

        private static void WriteLine(string text, ConsoleColor color)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = original;
        }
    }
}
