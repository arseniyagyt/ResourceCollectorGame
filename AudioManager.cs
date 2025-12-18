using System;
using System.IO;
using System.Media;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Простой аудио‑менеджер SFX.
    /// </summary>
    public static class AudioManager
    {
        private static SoundPlayer hitPlayer;
        private static SoundPlayer buildPlayer;

      

        /// <summary>
        /// Громкость эффектов (0..1).
        /// </summary>
        public static float SfxVolume { get; set; } = 1.0f;

        /// <summary>
        /// Инициализация плееров.
        /// </summary>
        public static void Initialize()
        {

            // Звук удара
            string hitPath = FindFile("hit.wav");
            if (hitPath != null)
            {
                hitPlayer = new SoundPlayer(hitPath);
            }

            // Звук постройки
            string buildPath = FindFile("build.wav");
            if (buildPath != null)
            {
                buildPlayer = new SoundPlayer(buildPath);
            }
        }

        private static string FindFile(string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path1 = Path.Combine(baseDir, "Sounds", fileName);
            string path2 = Path.Combine(baseDir, "..", "..", "Sounds", fileName);
            string path3 = Path.Combine(baseDir, fileName);

            if (File.Exists(path1)) return path1;
            if (File.Exists(path2)) return path2;
            if (File.Exists(path3)) return path3;
            return null;
        }

        /// <summary>
        /// Звук попадания по монстру.
        /// </summary>
        public static void PlayHit()
        {
            if (hitPlayer == null) return;
            if (SfxVolume <= 0f) return;
            hitPlayer.Play();
        }

        /// <summary>
        /// Звук постройки/улучшения.
        /// </summary>
        public static void PlayBuild()
        {
            if (buildPlayer == null) return;
            if (SfxVolume <= 0f) return;
            buildPlayer.Play();
        }
    }
}
