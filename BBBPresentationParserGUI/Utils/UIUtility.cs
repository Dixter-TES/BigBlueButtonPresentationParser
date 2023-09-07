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
