using System;
using System.Drawing;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Класс игрока
    /// Управляет позицией, размером и движением персонажа
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Позиция игрока на экране
        /// </summary>
        public PointF Position { get; set; }

        /// <summary>
        /// Размер игрока
        /// </summary>
        public SizeF Size { get; set; }

        /// <summary>
        /// Скорость движения
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Прямоугольник для проверки коллизий
        /// </summary>
        public RectangleF Bounds => new RectangleF(Position, Size);

        /// <summary>
        /// Кэшированная текстура
        /// </summary>
        private static Image cachedTexture = null;

        /// <summary>
        /// Кадры анимации ходьбы игрока.
        /// </summary>
        private static Image[] walkFrames = null;

        /// <summary>
        /// Текущий индекс кадра анимации.
        /// </summary>
        private int currentFrame = 0;

        /// <summary>
        /// Таймер для переключения кадров (секунды).
        /// </summary>
        private float frameTimer = 0f;

        /// <summary>
        /// Интервал между кадрами (секунды).
        /// </summary>
        private const float FrameInterval = 0.12f;

        /// <summary>
        /// Направление взгляда по X: 1 = вправо, -1 = влево.
        /// </summary>
        public int FacingX { get; private set; } = 1;

        /// <summary>
        /// Идёт ли игрок (есть ли движение).
        /// </summary>
        public bool IsMoving { get; private set; }


        /// <summary>
        /// Конструктор игрока
        /// </summary>
        /// <param name="x">Начальная координата X</param>
        /// <param name="y">Начальная координата Y</param>
        public Player(float x, float y)
        {
            Position = new PointF(x, y);
            Size = new SizeF(30, 30);
            Speed = 5f;
        }

        /// <summary>
        /// Движение по оси X.
        /// </summary>
        public void MoveX(float delta, float maxWidth)
        {
            float newX = Position.X + delta * Speed;
            if (newX >= 0 && newX + Size.Width <= maxWidth)
            {
                Position = new PointF(newX, Position.Y);

                // Запоминаем направление по X и флаг движения
                if (delta > 0) FacingX = 1;
                else if (delta < 0) FacingX = -1;

                if (delta != 0) IsMoving = true;
            }
        }

        /// <summary>
        /// Движение по оси Y.
        /// </summary>
        public void MoveY(float delta, float maxHeight)
        {
            float newY = Position.Y + delta * Speed;
            if (newY >= 0 && newY + Size.Height <= maxHeight)
            {
                Position = new PointF(Position.X, newY);

                if (delta != 0) IsMoving = true;
            }
        }

        /// <summary>
        /// Обновление таймера анимации ходьбы.
        /// </summary>
        public void UpdateAnimation(float deltaTime)
        {
            if (!IsMoving)
            {
                // Если не двигаемся – сбрасываем на первый кадр
                currentFrame = 0;
                frameTimer = 0f;
                return;
            }

            frameTimer += deltaTime;
            if (frameTimer >= FrameInterval)
            {
                frameTimer -= FrameInterval;
                currentFrame++;
                if (walkFrames != null && walkFrames.Length > 0)
                {
                    currentFrame %= walkFrames.Length;
                }
                else
                {
                    currentFrame = 0;
                }
            }

            // После обновления считаем, что на этом кадре движение обработано
            IsMoving = false;
        }


        /// <summary>
        /// Отрисовка игрока
        /// </summary>
        /// <param name="g">Графический контекст</param>
        public void Draw(Graphics g)
        {
            try
            {
                // Ленивая загрузка кадров анимации
                if (walkFrames == null)
                {
                    walkFrames = new Image[]
                    {
                        LoadTexture("player_walk_0.png"),
                        LoadTexture("player_walk_1.png"),
                        LoadTexture("player_walk_2.png")
                    };
                }

                // Фолбек к одной текстуре, если кадры не загрузились
                if ((walkFrames == null || walkFrames[0] == null) && cachedTexture == null)
                {
                    cachedTexture = LoadTexture("player.png");
                }

                Image frame = null;
                if (walkFrames != null && walkFrames.Length > 0 && walkFrames[0] != null)
                {
                    frame = walkFrames[Math.Max(0, Math.Min(currentFrame, walkFrames.Length - 1))];
                }
                else
                {
                    frame = cachedTexture;
                }

                if (frame != null)
                {
                    RectangleF b = Bounds;

                    if (FacingX >= 0)
                    {
                        g.DrawImage(frame, b);
                    }
                    else
                    {
                        g.DrawImage(frame, b.X + b.Width, b.Y, -b.Width, b.Height);
                    }
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
        /// Отрисовка фолбека для игрока
        /// </summary>
        /// <param name="g">Графический контекст</param>
        private void DrawFallback(Graphics g)
        {
            // Простой круг синего цвета
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
            using (Pen pen = new Pen(Color.White, 2))
            {
                g.FillEllipse(brush, Bounds);
                g.DrawEllipse(pen, Bounds);
            }
        }
    }
}
