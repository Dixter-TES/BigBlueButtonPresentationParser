using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBBPresentationParser.Utils
{
    internal static class UIUtility
    {
        public static void ShowMessage(string message)
        {
            new MessageWindow(message).Show();
        }
    }
}
