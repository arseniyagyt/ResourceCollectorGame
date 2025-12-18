using System.Drawing;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Класс для хранения глобальных настроек игры
    /// </summary>
    public static class GameSettings
    {
        public static Size Resolution { get; set; } = new Size(1024, 768);


        /// <summary>Громкость эффектов (0..1).</summary>
        public static float SfxVolume { get; set; } = 1.0f;

        public static bool IsFullscreen { get; set; } = false;
    }

}