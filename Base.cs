using System;
using System.Drawing;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Класс базы игрока.
    /// Хранит уровень улучшения и требования для апгрейда.
    /// </summary>
    public class Base
    {
        /// <summary>
        /// Текущий уровень базы (0-3).
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Позиция базы на карте.
        /// </summary>
        public PointF Position { get; set; }

        /// <summary>
        /// Размер базы (увеличивается с уровнем).
        /// </summary>
        public SizeF Size { get; set; }

        /// <summary>
        /// Названия уровней базы.
        /// </summary>
        private readonly string[] levelNames = { "Хижина", "Дом", "Особняк", "Замок" };

        /// <summary>
        /// Кэшированные текстуры для каждого уровня.
        /// </summary>
        private static Image[] cachedTextures = new Image[4];

        /// <summary>
        /// Конструктор базы.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        public Base(float x, float y)
        {
            Level = 0;
            Position = new PointF(x, y);
            Size = new SizeF(80, 80);
        }

        /// <summary>
        /// Получить название текущего уровня.
        /// </summary>
        public string GetLevelName()
        {
            return Level < levelNames.Length ? levelNames[Level] : "Максимум";
        }

        /// <summary>
        /// Требуемое дерево для улучшения (увеличено в 2 раза относительно базовой формулы).
        /// </summary>
        public int GetRequiredWood()
        {
            return (Level + 1) * 10 * 2;
        }

        /// <summary>
        /// Требуемый камень для улучшения (увеличено в 2 раза).
        /// </summary>
        public int GetRequiredStone()
        {
            return (Level + 1) * 8 * 2;
        }

        /// <summary>
        /// Требуемое золото для улучшения (увеличено в 2 раза).
        /// </summary>
        public int GetRequiredGold()
        {
            return (Level + 1) * 5 * 2;
        }

        /// <summary>
        /// Требуемое количество древних чертежей для улучшения базы.
        /// 1 уровень: 10, 2: 15, 3: 20, 4: 30.
        /// </summary>
        public int GetRequiredBlueprints()
        {
            switch (Level)
            {
                case 0: return 10;
                case 1: return 15;
                case 2: return 20;
                case 3: return 30;
                default: return 0;
            }
        }

        /// <summary>
        /// Можно ли улучшить базу.
        /// </summary>
        public bool CanUpgrade()
        {
            return Level < levelNames.Length - 1;
        }

        /// <summary>
        /// Улучшить базу на следующий уровень.
        /// </summary>
        public void Upgrade()
        {
            if (CanUpgrade())
            {
                Level++;
                Size = new SizeF(Size.Width + 20, Size.Height + 20);
            }
        }

        /// <summary>
        /// Отрисовка базы.
        /// </summary>
        public void Draw(Graphics g)
        {
            try
            {
                if (cachedTextures[Level] == null)
                {
                    string fileName = $"base_level{Level}.png";
                    cachedTextures[Level] = LoadTexture(fileName);
                }

                if (cachedTextures[Level] != null)
                {
                    g.DrawImage(cachedTextures[Level], Position.X, Position.Y, Size.Width, Size.Height);
                }
                else
                {
                    DrawFallback(g);
                }
            }
            catch
            {
                DrawFallback(g);
            }
        }

        /// <summary>
        /// Загрузка текстуры из файла.
        /// </summary>
        private static Image LoadTexture(string fileName)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string path1 = System.IO.Path.Combine(baseDir, "Textures", fileName);
                if (System.IO.File.Exists(path1))
                    return Image.FromFile(path1);

                string path2 = System.IO.Path.Combine(baseDir, "..", "..", "Textures", fileName);
                if (System.IO.File.Exists(path2))
                    return Image.FromFile(path2);

                string path3 = System.IO.Path.Combine(baseDir, fileName);
                if (System.IO.File.Exists(path3))
                    return Image.FromFile(path3);

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Отрисовка фолбека для базы (если текстура не найдена).
        /// </summary>
        private void DrawFallback(Graphics g)
        {
            Color baseColor = GetBaseColor();

            using (SolidBrush brush = new SolidBrush(baseColor))
            using (Pen pen = new Pen(Color.Black, 3))
            using (SolidBrush roofBrush = new SolidBrush(Color.FromArgb(139, 69, 19)))
            {
                g.FillRectangle(brush, Position.X, Position.Y, Size.Width, Size.Height);
                g.DrawRectangle(pen, Position.X, Position.Y, Size.Width, Size.Height);

                PointF[] roof =
                {
                    new PointF(Position.X + Size.Width / 2, Position.Y - 20),
                    new PointF(Position.X - 10, Position.Y),
                    new PointF(Position.X + Size.Width + 10, Position.Y)
                };
                g.FillPolygon(roofBrush, roof);
            }
        }

        /// <summary>
        /// Цвет базы в зависимости от уровня.
        /// </summary>
        private Color GetBaseColor()
        {
            switch (Level)
            {
                case 0: return Color.FromArgb(160, 82, 45);
                case 1: return Color.FromArgb(205, 133, 63);
                case 2: return Color.FromArgb(210, 180, 140);
                case 3: return Color.FromArgb(192, 192, 192);
                default: return Color.Gray;
            }
        }
    }
}
