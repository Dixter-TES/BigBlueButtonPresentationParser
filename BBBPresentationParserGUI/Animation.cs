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
        public static async void ButtonTextAnimation(Button button, string[] text, int delay, CancellationToken token)
        {
            await Task.Run(() =>
            {
                int index = 0;
                while(!token.IsCancellationRequested)
                {
                    try
                    {
                        button.Dispatcher.Invoke(() => button.Content = text[index]);
                        Task.Delay(delay, token).Wait();
                        index = index < text.Length - 1 ? index + 1 : 0;
                    }
                    catch { }
                    
                }
            });
        }
    }
}
