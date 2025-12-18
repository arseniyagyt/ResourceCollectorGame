using System;
using System.Drawing;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Класс монстра в пещере.
    /// Преследует игрока, крадёт золото и может получать урон.
    /// </summary>
    public class Monster
    {
        /// <summary>
        /// Текущая позиция монстра.
        /// </summary>
        public PointF Position { get; set; }

        /// <summary>
        /// Размер монстра.
        /// </summary>
        public SizeF Size { get; set; }

        /// <summary>
        /// Скорость движения при патрулировании.
        /// </summary>
        public float WanderSpeed { get; set; }

        /// <summary>
        /// Скорость движения при преследовании игрока.
        /// </summary>
        public float ChaseSpeed { get; set; }

        /// <summary>
        /// Радиус обнаружения игрока.
        /// </summary>
        public float DetectionRadius { get; set; }

        /// <summary>
        /// Прямоугольные границы для проверки коллизий.
        /// </summary>
        public RectangleF Bounds => new RectangleF(Position, Size);

        /// <summary>
        /// Таймер кражи ресурсов.
        /// </summary>
        public float StealTimer { get; set; }

        /// <summary>
        /// Интервал между кражами ресурсов.
        /// </summary>
        private const float StealInterval = 1.0f;

        /// <summary>
        /// Здоровье монстра (количество ударов до смерти).
        /// </summary>
        public int Health { get; private set; }

        /// <summary>
        /// Направление по X: 1 = вправо, -1 = влево.
        /// </summary>
        public int FacingX { get; private set; } = 1;

        /// <summary>
        /// Флаг, что монстр недавно получил урон.
        /// </summary>
        public bool IsHit { get; private set; }

        /// <summary>
        /// Оставшееся время эффекта попадания (сек).
        /// </summary>
        public float HitEffectTime { get; private set; }

        /// <summary>
        /// Генератор случайных чисел для патруля.
        /// </summary>
        private Random random;

        /// <summary>
        /// Направление патрулирования.
        /// </summary>
        private PointF wanderDirection;

        /// <summary>
        /// Таймер смены направления патрулирования.
        /// </summary>
        private float wanderTimer;

        /// <summary>
        /// Границы пещеры.
        /// </summary>
        private float maxWidth;
        private float maxHeight;

        /// <summary>
        /// Кэшированная текстура монстра.
        /// </summary>
        private static Image cachedTexture = null;

        /// <summary>
        /// Кадры анимации ходьбы монстра.
        /// </summary>
        private static Image[] walkFrames = null;

        /// <summary>
        /// Текущий кадр анимации.
        /// </summary>
        private int currentFrame = 0;

        /// <summary>
        /// Таймер анимации.
        /// </summary>
        private float frameTimer = 0f;

        /// <summary>
        /// Интервал между кадрами.
        /// </summary>
        private const float FrameInterval = 0.15f;

        /// <summary>
        /// Идёт ли монстр (двигался ли в этом кадре).
        /// </summary>
        public bool IsMoving { get; private set; }


        /// <summary>
        /// Конструктор монстра.
        /// </summary>
        public Monster(float x, float y)
        {
            Position = new PointF(x, y);
            Size = new SizeF(25, 25);
            WanderSpeed = 1f;
            ChaseSpeed = 2f;
            DetectionRadius = 150f;
            StealTimer = 0f;
            random = new Random(Guid.NewGuid().GetHashCode());
            wanderDirection = new PointF(0, 0);
            wanderTimer = 0f;
            maxWidth = 1024;
            maxHeight = 768;
            Health = 3;
        }

        /// <summary>
        /// Задать границы пещеры.
        /// </summary>
        public void SetBounds(float width, float height)
        {
            maxWidth = width;
            maxHeight = height;
        }

        /// <summary>
        /// Обновление состояния монстра (патруль/преследование игрока).
        /// </summary>
        public void Update(PointF playerPos, float deltaTime)
        {
            float distance = GetDistance(playerPos);

            // Преследование игрока
            if (distance < DetectionRadius)
            {
                float dx = playerPos.X - Position.X;
                float dy = playerPos.Y - Position.Y;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);

                if (length > 0)
                {
                    dx /= length;
                    dy /= length;

                    if (dx > 0) FacingX = 1;
                    else if (dx < 0) FacingX = -1;

                    float newX = Position.X + dx * ChaseSpeed;
                    float newY = Position.Y + dy * ChaseSpeed;

                    newX = Math.Max(0, Math.Min(newX, maxWidth - Size.Width));
                    newY = Math.Max(0, Math.Min(newY, maxHeight - Size.Height));

                    Position = new PointF(newX, newY);
                    IsMoving = true;

                }

            }
            else
            {
                // Патрулирование
                wanderTimer -= deltaTime;
                if (wanderTimer <= 0)
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2);
                    wanderDirection = new PointF(
                        (float)Math.Cos(angle),
                        (float)Math.Sin(angle)
                    );

                    if (wanderDirection.X > 0) FacingX = 1;
                    else if (wanderDirection.X < 0) FacingX = -1;

                    wanderTimer = 2f;
                }

                float newX = Position.X + wanderDirection.X * WanderSpeed;
                float newY = Position.Y + wanderDirection.Y * WanderSpeed;

                newX = Math.Max(0, Math.Min(newX, maxWidth - Size.Width));
                newY = Math.Max(0, Math.Min(newY, maxHeight - Size.Height));

                if (newX <= 0 || newX >= maxWidth - Size.Width ||
                    newY <= 0 || newY >= maxHeight - Size.Height)
                {
                    wanderTimer = 0;
                }

                Position = new PointF(newX, newY);
                IsMoving = wanderDirection.X != 0 || wanderDirection.Y != 0;
            }

            // Кража
            StealTimer += deltaTime;

            // Эффект попадания
            if (IsHit)
            {
                HitEffectTime -= deltaTime;
                if (HitEffectTime <= 0f)
                {
                    IsHit = false;
                    HitEffectTime = 0f;
                }
            }
            // Обновление анимации ходьбы
            if (!IsMoving)
            {
                currentFrame = 0;
                frameTimer = 0f;
            }
            else
            {
                frameTimer += deltaTime;
                if (frameTimer >= FrameInterval)
                {
                    frameTimer -= FrameInterval;
                    currentFrame++;
                    if (walkFrames != null && walkFrames.Length > 0)
                        currentFrame %= walkFrames.Length;
                    else
                        currentFrame = 0;
                }
            }

            // Сброс флага, движение будет заново выставлено в следующем кадре
            IsMoving = false;

        }

        /// <summary>
        /// Можно ли украсть ресурсы (по таймеру).
        /// </summary>
        public bool CanSteal()
        {
            if (StealTimer >= StealInterval)
            {
                StealTimer = 0f;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Расстояние до точки.
        /// </summary>
        public float GetDistance(PointF point)
        {
            float dx = point.X - Position.X;
            float dy = point.Y - Position.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Нанесение удара монстру.
        /// </summary>
        public bool TakeHit()
        {
            if (Health <= 0) return false;

            Health--;

            IsHit = true;
            HitEffectTime = 0.2f;

            return Health <= 0;
        }

        /// <summary>
        /// Отрисовка монстра.
        /// </summary>
        public void Draw(Graphics g)
        {
            try
            {
                // Ленивая загрузка кадров монстра
                if (walkFrames == null)
                {
                    walkFrames = new Image[]
                    {
        LoadTexture("monster_walk_0.png"),
        LoadTexture("monster_walk_1.png")
                    };
                }

                if ((walkFrames == null || walkFrames[0] == null) && cachedTexture == null)
                {
                    cachedTexture = LoadTexture("monster.png");
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

                RectangleF b = Bounds;

                if (frame != null)
                {
                    if (FacingX >= 0)
                        g.DrawImage(frame, b);
                    else
                        g.DrawImage(frame, b.X + b.Width, b.Y, -b.Width, b.Height);
                }
                else
                {
                    DrawFallback(g);
                }


                if (IsHit)
                {
                    using (SolidBrush hitBrush = new SolidBrush(Color.FromArgb(120, 255, 0, 0)))
                    {
                        g.FillRectangle(hitBrush, b);
                    }
                }
            }
            catch
            {
                DrawFallback(g);
            }
        }

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

        private void DrawFallback(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(139, 0, 0)))
            using (Pen pen = new Pen(Color.Black, 2))
            {
                g.FillEllipse(brush, Bounds);
                g.DrawEllipse(pen, Bounds);
            }

            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString("М", font, textBrush, Position.X + 6, Position.Y + 5);
            }
        }
    }
}
