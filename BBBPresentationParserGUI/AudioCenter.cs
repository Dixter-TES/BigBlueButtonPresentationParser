using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;

namespace BBBPresentationParser
{
    internal static class AudioCenter
    {
        public static void PlaySound(Stream source)
        {
            using SoundPlayer player = new SoundPlayer(source);
            player.Load();
            player.Play();
        }
    }
}
