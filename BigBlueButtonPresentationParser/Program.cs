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
        private static void Main(string[] args)
        {
            Console.Write("Введите базовую ссылку: ");
            string url = Console.ReadLine();
            string[] parts = url.Split('/');
            url = string.Join("/", parts.Take(parts.Length - 1).ToArray());

            WriteLine("Запускаем виртуальный браузер...", ConsoleColor.Yellow);
            Console.CursorVisible = false;

            string tempPdfFile = MakePresentation(url + "/");

            if(tempPdfFile == null)
            {
                WriteLine("Не удалось получить ни одного слайда", ConsoleColor.Red);
                Exit();
                return;
            }

            WriteLine("Выберите место для сохранения PDF-файла", ConsoleColor.Yellow);
            TrySavePresentation(tempPdfFile);

            Exit();
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = original;
        }

        private static void Exit()
        {
            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
            Process.GetCurrentProcess().CloseMainWindow();
        }

        private static void TrySavePresentation(string filename)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = filename;
            saveDialog.Filter = "PDF-Файлы|*.pdf";
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MoveFileAsync(filename, saveDialog.FileName);
                WriteLine("PDF-файл сохранён", ConsoleColor.Yellow);
            }
            else
            {
                WriteLine("Сохранение файла отменено", ConsoleColor.Yellow);
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

        private static string MakePresentation(string url)
        {
            PresentationParser parser = new PresentationParser(url);
            string[] scrs = parser.Parse();
            Thread.Sleep(1);

            if (scrs.Length == 0)
                return null;

            WriteLine("Объединяем слайды в PDF-файл...", ConsoleColor.Yellow);
            var document = GeneratePresentationDocument(scrs);
            return document;
        }

        private static string GeneratePresentationDocument(string[] images)
        {
            LoadingBarVisualization vis = new LoadingBarVisualization();
            vis.FillColor = ConsoleColor.Cyan;

            LoadingBar savingLoadingBar = new LoadingBar(1, images.Length, visualization: vis);
            Console.WriteLine();

            Image im = Image.GetInstance(images[0]);

            var doc = new Document(new Rectangle(im));
            string filename = $"{DateTime.Now.Ticks}.pdf";

            PdfWriter.GetInstance(doc, new FileStream(filename, FileMode.Create));
            doc.Open();

            Thread.Sleep(1);
            foreach (string img in images)
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
    }
}
