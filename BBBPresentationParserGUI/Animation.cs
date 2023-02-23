using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BBBPresentationParser
{
    internal class Animation
    {
        public static async void ButtonTextAnimation(Button button, string[] text, int delay, CancellationTokenSource token)
        {
            int index = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    button.Dispatcher.Invoke(() => button.Content = text[index]);
                    await Task.Delay(delay, token.Token);
                    index = index < text.Length - 1 ? index + 1 : 0;
                }
                catch { }

            }
        }
    }
}
