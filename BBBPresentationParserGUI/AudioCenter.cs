using System.IO;
using System.Media;

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
