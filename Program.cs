using System;
using System.Windows.Forms;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Главный класс приложения
    /// Точка входа в программу
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainMenuForm());
        }
    }
}