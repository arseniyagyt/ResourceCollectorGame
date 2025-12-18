using System;
using System.Drawing;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Тип автоматического здания (лесопилка, каменоломня, золотая шахта).
    /// Отвечает за пассивную добычу ресурсов.
    /// </summary>
    public enum AutoBuildingType
    {
        Sawmill,    // Лесопилка (дерево)
        Quarry,     // Каменоломня (камень)
        GoldMine    // Золотая шахта (золото)
    }

    /// <summary>
    /// Класс автоматического здания.
    /// Здание строится за ресурсы и даёт пассивную добычу: 1 ед/сек * уровень.
    /// </summary>
    public class AutoBuilding
    {
        /// <summary>
        /// Тип здания (определяет вид ресурса).
        /// </summary>
        public AutoBuildingType Type { get; private set; }

        /// <summary>
        /// Текущий уровень здания (1..3).
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Максимальный уровень здания.
        /// </summary>
        public const int MaxLevel = 3;

        /// <summary>
        /// Позиция на карте.
        /// </summary>
        public PointF Position { get; set; }

        /// <summary>
        /// Размер здания (для отрисовки и коллизий).
        /// </summary>
        public SizeF Size { get; set; }

        /// <summary>
        /// Прямоугольник границ здания.
        /// </summary>
        public RectangleF Bounds => new RectangleF(Position, Size);

        // Кэшированные текстуры для каждого типа
        private static Image sawmillTexture;
        private static Image quarryTexture;
        private static Image goldMineTexture;

        /// <summary>
        /// Конструктор автоматического здания.
        /// </summary>
        /// <param name="type">Тип здания.</param>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        public AutoBuilding(AutoBuildingType type, float x, float y)
        {
            Type = type;
            Level = 1;                // Начинаем с 1 уровня
            Position = new PointF(x, y);
            Size = new SizeF(60, 60); // Чуть меньше базы
        }

        /// <summary>
        /// Получить текущую скорость добычи ресурса (кол-во в секунду).
        /// 1 ресурс / секунда * уровень.
        /// </summary>
        public float GetProductionPerSecond()
        {
            return Level * 1.0f;
        }

        /// <summary>
        /// Можно ли улучшить здание (ещё не достигнут максимальный уровень).
        /// </summary>
        public bool CanUpgrade()
        {
            return Level < MaxLevel;
        }

        /// <summary>
        /// Улучшить здание на 1 уровень, если это возможно.
        /// </summary>
        public void Upgrade()
        {
            if (CanUpgrade())
            {
                Level++;
            }
        }

        /// <summary>
        /// Стоимость постройки здания (на 1 уровне).
        /// Возвращает дерево, камень, золото.
        /// </summary>
        public (int wood, int stone, int gold) GetBuildCost()
        {
            switch (Type)
            {
                case AutoBuildingType.Sawmill:
                    // Лесопилка: недорогая
                    return (wood: 20, stone: 10, gold: 0);
                case AutoBuildingType.Quarry:
                    // Каменоломня: подороже и по камню
                    return (wood: 10, stone: 20, gold: 0);
                case AutoBuildingType.GoldMine:
                    // Золотая шахта: дорогая, требует золото
                    return (wood: 30, stone: 20, gold: 5);
                default:
                    return (0, 0, 0);
            }
        }

        /// <summary>
        /// Стоимость улучшения здания (за каждый следующий уровень).
        /// Возвращает дерево, камень, золото.
        /// </summary>
        public (int wood, int stone, int gold) GetUpgradeCost()
        {
            // Улучшение в 2 раза дороже базовой постройки
            var baseCost = GetBuildCost();
            return (baseCost.wood * 2, baseCost.stone * 2, baseCost.gold * 2);
        }

        /// <summary>
        /// Сколько древних чертежей нужно для постройки.
        /// Для зданий: на первый уровень 5.
        /// </summary>
        public int GetBuildBlueprintCost()
        {
            return 5;
        }

        /// <summary>
        /// Сколько древних чертежей нужно для апгрейда.
        /// На каждый уровень в 2 раза больше: 5, 10, 20...
        /// </summary>
        public int GetUpgradeBlueprintCost()
        {
            return GetBuildBlueprintCost() * (int)Math.Pow(2, Level - 1);
        }

        /// <summary>
        /// Отрисовка здания на экране.
        /// Сначала пробуем PNG-текстуру, потом рисуем фолбек.
        /// </summary>
        /// <param name="g">Графический контекст.</param>
        public void Draw(Graphics g)
        {
            try
            {
                Image texture = GetTexture();
                if (texture != null)
                {
                    g.DrawImage(texture, Bounds);
                }
                else
                {
                    DrawFallback(g);
                }

                // Подпись уровня над зданием
                using (Font font = new Font("Arial", 9, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    string label = $"Ур. {Level}";
                    SizeF size = g.MeasureString(label, font);
                    float x = Position.X + (Size.Width - size.Width) / 2;
                    float y = Position.Y - size.Height - 2;
                    g.DrawString(label, font, brush, x, y);
                }
            }
            catch
            {
                DrawFallback(g);
            }
        }

        /// <summary>
        /// Получить PNG-текстуру для здания в зависимости от типа.
        /// </summary>
        private Image GetTexture()
        {
            string fileName = "";
            Image cached = null;

            switch (Type)
            {
                case AutoBuildingType.Sawmill:
                    fileName = "sawmill.png";
                    if (sawmillTexture == null)
                        sawmillTexture = LoadTexture(fileName);
                    cached = sawmillTexture;
                    break;
                case AutoBuildingType.Quarry:
                    fileName = "quarry.png";
                    if (quarryTexture == null)
                        quarryTexture = LoadTexture(fileName);
                    cached = quarryTexture;
                    break;
                case AutoBuildingType.GoldMine:
                    fileName = "goldmine.png";
                    if (goldMineTexture == null)
                        goldMineTexture = LoadTexture(fileName);
                    cached = goldMineTexture;
                    break;
            }

            return cached;
        }

        /// <summary>
        /// Загрузка текстуры из файла с несколькими вариантами пути.
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
        /// Отрисовка фолбек-варианта, если PNG не найден.
        /// </summary>
        private void DrawFallback(Graphics g)
        {
            Color color;
            string letter;

            switch (Type)
            {
                case AutoBuildingType.Sawmill:
                    color = Color.SaddleBrown;
                    letter = "Л";
                    break;
                case AutoBuildingType.Quarry:
                    color = Color.Gray;
                    letter = "К";
                    break;
                case AutoBuildingType.GoldMine:
                    color = Color.Gold;
                    letter = "З";
                    break;
                default:
                    color = Color.White;
                    letter = "?";
                    break;
            }

            using (SolidBrush brush = new SolidBrush(color))
            using (Pen pen = new Pen(Color.Black, 2))
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(brush, Bounds);
                g.DrawRectangle(pen, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);

                SizeF textSize = g.MeasureString(letter, font);
                float x = Position.X + (Size.Width - textSize.Width) / 2;
                float y = Position.Y + (Size.Height - textSize.Height) / 2;
                g.DrawString(letter, font, textBrush, x, y);
            }
        }
    }
}
