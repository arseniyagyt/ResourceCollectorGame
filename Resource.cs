using System;
using System.Drawing;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Типы ресурсов в игре
    /// </summary>
    public enum ResourceType
    {
        Wood,  // Дерево
        Stone, // Камень
        Gold   // Золото
    }

    /// <summary>
    /// Класс источника ресурсов (дерево, камень, золото)
    /// Управляет добычей ресурсов и отображением узлов на карте
    /// </summary>
    public class ResourceNode
    {
        /// <summary>
        /// Тип ресурса
        /// </summary>
        public ResourceType Type { get; set; }

        /// <summary>
        /// Позиция узла ресурса на карте
        /// </summary>
        public PointF Position { get; set; }

        /// <summary>
        /// Размер узла ресурса
        /// </summary>
        public SizeF Size { get; set; }

        /// <summary>
        /// Количество оставшихся ресурсов в узле (не используется, ресурсы бесконечны)
        /// </summary>
        public int ResourceAmount { get; set; }

        /// <summary>
        /// Флаг процесса добычи
        /// </summary>
        public bool IsBeingMined { get; set; }

        /// <summary>
        /// Текущий прогресс добычи (в секундах)
        /// </summary>
        public float MiningProgress { get; set; }

        /// <summary>
        /// Время, необходимое для добычи одного ресурса (в секундах)
        /// </summary>
        public float MiningTime { get; set; }

        /// <summary>
        /// Прямоугольные границы узла для проверки коллизий
        /// </summary>
        public RectangleF Bounds => new RectangleF(Position, Size);

        /// <summary>
        /// Кэшированная текстура для оптимизации
        /// </summary>
        private static Image cachedTreeTexture = null;
        private static Image cachedStoneTexture = null;
        private static Image cachedGoldTexture = null;

        /// <summary>
        /// Конструктор узла ресурса
        /// </summary>
        /// <param name="type">Тип ресурса</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public ResourceNode(ResourceType type, float x, float y)
        {
            Type = type;
            Position = new PointF(x, y);
            Size = new SizeF(40, 40);
            ResourceAmount = 999; // Бесконечные ресурсы
            IsBeingMined = false;
            MiningProgress = 0;
            MiningTime = 1.0f; // Время добычи 1 секунда
        }

        /// <summary>
        /// Отрисовка узла ресурса и прогресса добычи
        /// </summary>
        /// <param name="g">Графический контекст для рисования</param>
        public void Draw(Graphics g)
        {
            // Рисуем текстуру источника
            DrawTexture(g);

            // Показываем прогресс добычи, если ресурс добывается
            if (IsBeingMined)
            {
                float barWidth = 40;
                float barHeight = 6;
                float barX = Position.X;
                float barY = Position.Y - 10;

                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                using (SolidBrush progressBrush = new SolidBrush(Color.FromArgb(78, 201, 176)))
                {
                    // Фон полосы прогресса
                    g.FillRectangle(bgBrush, barX, barY, barWidth, barHeight);

                    // Заполненная часть полосы прогресса
                    g.FillRectangle(progressBrush, barX, barY, barWidth * (MiningProgress / MiningTime), barHeight);
                }
            }
        }

        /// <summary>
        /// Отрисовка текстуры узла ресурса
        /// </summary>
        /// <param name="g">Графический контекст</param>
        private void DrawTexture(Graphics g)
        {
            try
            {
                Image texture = null;
                string fileName = "";

                // Определяем имя файла текстуры в зависимости от типа ресурса
                switch (Type)
                {
                    case ResourceType.Wood:
                        fileName = "tree.png";
                        if (cachedTreeTexture == null)
                        {
                            cachedTreeTexture = LoadTexture(fileName);
                        }
                        texture = cachedTreeTexture;
                        break;
                    case ResourceType.Stone:
                        fileName = "stone.png";
                        if (cachedStoneTexture == null)
                        {
                            cachedStoneTexture = LoadTexture(fileName);
                        }
                        texture = cachedStoneTexture;
                        break;
                    case ResourceType.Gold:
                        fileName = "gold.png";
                        if (cachedGoldTexture == null)
                        {
                            cachedGoldTexture = LoadTexture(fileName);
                        }
                        texture = cachedGoldTexture;
                        break;
                }

                if (texture != null)
                {
                    g.DrawImage(texture, Bounds);
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
        /// Загрузка текстуры из файла с несколькими попытками поиска
        /// </summary>
        /// <param name="fileName">Имя файла текстуры</param>
        /// <returns>Загруженное изображение или null</returns>
        private static Image LoadTexture(string fileName)
        {
            try
            {
                // Попытка 1: Относительный путь от исполняемого файла
                string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures", fileName);
                if (System.IO.File.Exists(path1))
                {
                    return Image.FromFile(path1);
                }

                // Попытка 2: Папка Textures в корне проекта
                string path2 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Textures", fileName);
                if (System.IO.File.Exists(path2))
                {
                    return Image.FromFile(path2);
                }

                // Попытка 3: Прямо в папке с exe
                string path3 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (System.IO.File.Exists(path3))
                {
                    return Image.FromFile(path3);
                }

                // Попытка 4: В папке bin\Debug\Textures
                string path4 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Debug", "Textures", fileName);
                if (System.IO.File.Exists(path4))
                {
                    return Image.FromFile(path4);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Отрисовка фолбека (если текстура не загрузилась)
        /// </summary>
        /// <param name="g">Графический контекст</param>
        private void DrawFallback(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(GetColor()))
            using (Pen pen = new Pen(Color.Black, 2))
            {
                if (Type == ResourceType.Wood)
                {
                    // Дерево - круг с коричневым цветом
                    g.FillEllipse(brush, Bounds);
                    g.DrawEllipse(pen, Bounds);
                }
                else
                {
                    // Камень и золото - прямоугольники
                    g.FillRectangle(brush, Bounds);
                    g.DrawRectangle(pen, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
                }

                // Подпись типа ресурса
                using (Font font = new Font("Arial", 8, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    string label = Type == ResourceType.Wood ? "Д" :
                                   Type == ResourceType.Stone ? "К" : "З";
                    g.DrawString(label, font, textBrush, Position.X + 12, Position.Y + 12);
                }
            }
        }

        /// <summary>
        /// Получить цвет узла в зависимости от типа ресурса
        /// </summary>
        /// <returns>Цвет для отрисовки</returns>
        private Color GetColor()
        {
            switch (Type)
            {
                case ResourceType.Wood: return Color.FromArgb(101, 67, 33);     // Коричневый (дерево)
                case ResourceType.Stone: return Color.FromArgb(128, 128, 128);  // Серый (камень)
                case ResourceType.Gold: return Color.FromArgb(255, 215, 0);     // Золотой цвет
                default: return Color.White;
            }
        }
    }

    /// <summary>
    /// Класс входа в пещеру
    /// Представляет точку перехода из основного мира в пещеру с золотом
    /// </summary>
    public class CaveEntrance
    {
        /// <summary>
        /// Позиция входа в пещеру
        /// </summary>
        public PointF Position { get; set; }

        /// <summary>
        /// Размер входа в пещеру
        /// </summary>
        public SizeF Size { get; set; }

        /// <summary>
        /// Прямоугольные границы для проверки коллизий
        /// </summary>
        public RectangleF Bounds => new RectangleF(Position, Size);

        /// <summary>
        /// Кэшированная текстура
        /// </summary>
        private static Image cachedTexture = null;

        /// <summary>
        /// Конструктор входа в пещеру
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public CaveEntrance(float x, float y)
        {
            Position = new PointF(x, y);
            Size = new SizeF(50, 50);
        }

        /// <summary>
        /// Отрисовка входа в пещеру
        /// </summary>
        /// <param name="g">Графический контекст</param>
        public void Draw(Graphics g)
        {
            try
            {
                if (cachedTexture == null)
                {
                    cachedTexture = LoadTexture("cave_entrance.png");
                }

                if (cachedTexture != null)
                {
                    g.DrawImage(cachedTexture, Bounds);
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
        /// Загрузка текстуры из файла
        /// </summary>
        private static Image LoadTexture(string fileName)
        {
            try
            {
                string path1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures", fileName);
                if (System.IO.File.Exists(path1))
                {
                    return Image.FromFile(path1);
                }

                string path2 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Textures", fileName);
                if (System.IO.File.Exists(path2))
                {
                    return Image.FromFile(path2);
                }

                string path3 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (System.IO.File.Exists(path3))
                {
                    return Image.FromFile(path3);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Отрисовка фолбека для входа в пещеру
        /// </summary>
        /// <param name="g">Графический контекст</param>
        private void DrawFallback(Graphics g)
        {
            // Темный круг с оранжевой рамкой
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
            using (Pen pen = new Pen(Color.Orange, 3))
            {
                g.FillEllipse(brush, Bounds);
                g.DrawEllipse(pen, Bounds);
            }

            // Надпись "П" (Пещера)
            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString("П", font, textBrush, Position.X + 18, Position.Y + 15);
            }
        }
    }

    /// <summary>
    /// Класс монстра в пещере
    /// Преследует игрока и крадет золото при столкновении
    /// </summary>
    
}
