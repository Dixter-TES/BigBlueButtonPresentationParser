using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.util;

namespace BBBPresentationParser
{
    internal class AudioCenter
    {
        public static void PlaySound(Stream soundStream)
        {
            SoundPlayer player = new SoundPlayer(soundStream);
            player.Play();
            player.Dispose();
        }
    }
}
