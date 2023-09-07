using System.Windows;
using System.Windows.Input;

namespace BBBPresentationParser
{
    /// <summary>
    /// Логика взаимодействия для MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow(string message)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            messageTb.Text = message;
        }

        private void TitleLb_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void okButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
