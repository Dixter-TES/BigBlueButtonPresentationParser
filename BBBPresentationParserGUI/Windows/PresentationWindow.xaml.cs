using BBBPresentationParser.PresentationUtils;
using BBBPresentationParser.Utils;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BBBPresentationParser.Extensions;

namespace BBBPresentationParser.Windows
{
    /// <summary>
    /// Логика взаимодействия для PresentationWindow.xaml
    /// </summary>
    public partial class PresentationWindow : Window
    {
        private string _url;
        private PresentationViewer? _viewer;
        private int _currentSlide = 1;

        public PresentationWindow(string url)
        {
            InitializeComponent();
            _url = url;
            InitializePresentation();
        }

        private void TitleLb_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) => Close();

        private async void InitializePresentation()
        {
            IBrowser? driver = await TryInitializeBrowser();

            if (driver is null)
                return;

            try
            {
                _viewer = new PresentationViewer(_url, driver);
                _viewer.SlideParsed += (s, e) =>
                {
                    slideImg.Source = BitmapImageUtils.BitmapImageFromBytes(e);
                };

                await _viewer.ParseSlideAsync(1);
            }
            catch (Exception ex)
            {
                UIUtility.ShowMessage("Произошла ошибка: " + ex.Message);
                Close();
            }
        }

        private async Task<IBrowser?> TryInitializeBrowser()
        {
            try
            {
                return await DriverSetup.GetSupportedDriver(true);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Произошла ошибка при получении драйвера: {ex.Message}";
                UIUtility.ShowMessage(errorMessage);
            }

            return null;
        }


        private async void NextSlide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewer is null)
                return;

            _currentSlide++;

            var slide = await _viewer.ParseSlideAsync(_currentSlide);

            if (slide is null)
            {
                _currentSlide--;
                return;
            }
        }

        private async void PreviousSlide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewer is null)
                return;

            if(_currentSlide > 1)
                _currentSlide--;

            var slide = await _viewer.ParseSlideAsync(_currentSlide);

            if (slide is null)
            {
                _currentSlide++;
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewer is not null)
                _viewer.Dispose();
        }
    }
}
