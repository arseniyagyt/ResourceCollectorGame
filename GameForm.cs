using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Главная форма игры.
    /// Управляет отрисовкой, вводом и игровым циклом.
    /// </summary>
    public partial class GameForm : Form
    {
        /// <summary>
        /// Игровой движок (логика игры).
        /// </summary>
        private GameEngine engine;

        /// <summary>
        /// Таймер игрового цикла (~60 FPS).
        /// </summary>
        private Timer gameTimer;

        /// <summary>
        /// Флаг паузы.
        /// </summary>
        private bool isPaused;

        // Клавиши управления движением
        private bool keyUp, keyDown, keyLeft, keyRight;

        /// <summary>
        /// Список активных уведомлений на экране.
        /// </summary>
        private List<Notification> notifications = new List<Notification>();

        /// <summary>
        /// Вспомогательный класс для текстовых уведомлений.
        /// </summary>
        private class Notification
        {
            public string Message;
            public Color Color;
            public float TimeLeft;
        }
        /// <summary>
        /// Кадры анимации удара.
        /// </summary>
        private static Image[] attackFrames = null;

        /// <summary>
        /// Текущий кадр анимации удара.
        /// </summary>
        private int attackFrameIndex = 0;


        /// <summary>
        /// Конструктор формы игры.
        /// </summary>
        public GameForm()
        {
            InitializeComponent();
            SetupGame();
            // Аудио
            AudioManager.Initialize();
            AudioManager.SfxVolume = GameSettings.SfxVolume;

        }

        /// <summary>
        /// Настройка формы и запуск игры.
        /// </summary>
        private void SetupGame()
        {
            // Настройка формы
            this.Text = "Resource Collector Game";
            if (GameSettings.IsFullscreen)
            {
                // Полноэкранный режим
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;

                // Используем текущий размер экрана как "игровое поле"
                var screenBounds = Screen.PrimaryScreen.Bounds;
                this.ClientSize = new Size(screenBounds.Width, screenBounds.Height);
            }
            else
            {
                // Обычный оконный режим
                this.ClientSize = GameSettings.Resolution;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MaximizeBox = false;
            }

            this.BackColor = Color.FromArgb(34, 139, 34); // Зеленый фон (трава)
            this.DoubleBuffered = true; // Устранение мерцания

            // Инициализация движка
            engine = new GameEngine(this.ClientSize.Width, this.ClientSize.Height);

            // Настройка таймера
            gameTimer = new Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            isPaused = false;

            // Обработчики событий ввода/отрисовки
            this.Paint += GameForm_Paint;
            this.KeyDown += GameForm_KeyDown;
            this.KeyUp += GameForm_KeyUp;
            this.MouseDown += GameForm_MouseDown;
        }

        /// <summary>
        /// Игровой цикл (обновление каждый кадр).
        /// </summary>
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused) return;

            // Обработка движения
            float deltaX = 0;
            float deltaY = 0;

            if (keyLeft) deltaX -= 1;
            if (keyRight) deltaX += 1;
            if (keyUp) deltaY -= 1;
            if (keyDown) deltaY += 1;

            if (deltaX != 0 || deltaY != 0)
            {
                engine.MovePlayer(deltaX, deltaY);
            }

            // Обновление игровой логики
            engine.Update(0.016f); // ~60 FPS = 16ms

            // Проверка столкновения с монстрами
            if (engine.IsInCave && engine.CheckMonsterCollision())
            {
                ShowNotification("Монстр украл часть ресурсов!", Color.FromArgb(255, 84, 89));
            }

            // Перерисовка
            this.Invalidate();
        }

        /// <summary>
        /// Отрисовка игры.
        /// </summary>
        private void GameForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Фон и объекты в зависимости от локации
            if (engine.IsInCave)
            {
                using (SolidBrush caveBrush = new SolidBrush(Color.FromArgb(25, 25, 25)))
                {
                    g.FillRectangle(caveBrush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                }

                // Узлы ресурсов (золото) в пещере
                foreach (var node in engine.ResourceNodes)
                {
                    node.Draw(g);
                }

                // Монстры в пещере
                foreach (var monster in engine.Monsters)
                {
                    monster.Draw(g);
                }

                // В пещере НЕ рисуем базу и здания
            }
            else
            {
                // На поверхности: ресурсы
                foreach (var node in engine.ResourceNodes)
                {
                    node.Draw(g);
                }

                // Входы в пещеры (только на поверхности)
                foreach (var cave in engine.CaveEntrances)
                {
                    cave.Draw(g);
                }

                // Автоматические здания (только на поверхности)
                foreach (var building in engine.AutoBuildings)
                {
                    building.Draw(g);
                }

                // База (только на поверхности)
                engine.PlayerBase.Draw(g);
            }

            // Игрок (и в пещере, и на поверхности)
            engine.Player.Draw(g);

            // Отрисовка удара (если анимация активна)
            if (engine.IsAttacking)
            {
                EnsureAttackTextureLoaded();
                RectangleF hitbox = engine.GetAttackHitbox();

                if (engine.IsAttacking)
                {
                    EnsureAttackTextureLoaded();

                    //RectangleF attackHitbox = engine.GetAttackHitbox();

                    Image frame = null;
                    if (attackFrames != null && attackFrames.Length > 0)
                    {
                        // Выбираем кадр по прогрессу удара
                        float p = engine.AttackProgress; // 0..1
                        int idx = (int)(p * attackFrames.Length);
                        if (idx >= attackFrames.Length) idx = attackFrames.Length - 1;
                        frame = attackFrames[idx];
                    }

                    if (frame != null)
                    {
                        int facing = engine.Player.FacingX >= 0 ? 1 : -1;

                        if (facing >= 0)
                        {
                            g.DrawImage(frame, hitbox);
                        }
                        else
                        {
                            g.DrawImage(frame,
                                hitbox.X + hitbox.Width, hitbox.Y,
                                -hitbox.Width, hitbox.Height);
                        }
                    }
                    else
                    {
                        using (SolidBrush brush = new SolidBrush(Color.FromArgb(120, 255, 255, 0)))
                        {
                            g.FillEllipse(brush, hitbox);
                        }
                    }
                }


            }


            // UI и уведомления
            DrawUI(g);
            DrawNotifications(g);

            // Экран паузы
            if (isPaused)
            {
                DrawPauseScreen(g);
            }
        }

        /// <summary>
        /// Отрисовка пользовательского интерфейса (ресурсы, база, автодобыча, управление).
        /// </summary>
        private void DrawUI(Graphics g)
        {
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
            {
                // Фон для UI
                g.FillRectangle(bgBrush, 10, 10, 270, 290);

                // Ресурсы
                g.DrawString($"Дерево: {engine.WoodCount}", font, brush, 20, 20);
                g.DrawString($"Камень: {engine.StoneCount}", font, brush, 20, 45);
                g.DrawString($"Золото: {engine.GoldCount}", font, brush, 20, 70);
                g.DrawString($"Чертежи: {engine.AncientBlueprintsCount}", font, brush, 20, 95);

                // База
                g.DrawString($"База: {engine.PlayerBase.GetLevelName()}", font, brush, 20, 120);

                if (engine.PlayerBase.CanUpgrade())
                {
                    using (Font smallFont = new Font("Arial", 9))
                    {
                        int reqWood = engine.PlayerBase.GetRequiredWood();
                        int reqStone = engine.PlayerBase.GetRequiredStone();
                        int reqGold = engine.PlayerBase.GetRequiredGold();
                        int reqBp = engine.PlayerBase.GetRequiredBlueprints();

                        g.DrawString(
                            $"Для улучшения: {reqWood}Д {reqStone}К {reqGold}З + {reqBp}Ч",
                            smallFont, brush, 20, 140);

                        g.DrawString("[Пробел] - улучшить базу", smallFont, brush, 20, 160);
                    }
                }
                else
                {
                    using (SolidBrush goldBrush = new SolidBrush(Color.Gold))
                    {
                        g.DrawString("МАКСИМАЛЬНЫЙ УРОВЕНЬ", font, goldBrush, 20, 140);
                    }
                }

                // Автодобыча
                using (Font smallFont2 = new Font("Arial", 9))
                {
                    g.DrawString("Автодобыча:", smallFont2, brush, 20, 185);

                    // Лесопилка
                    var sawmill = engine.AutoBuildings.Find(b => b.Type == AutoBuildingType.Sawmill);
                    if (sawmill != null)
                    {
                        var upCost = sawmill.GetUpgradeCost();
                        int bpCost = sawmill.GetUpgradeBlueprintCost();

                        g.DrawString(
                            $"Лесопилка ур.{sawmill.Level} (+{sawmill.Level}/с дерева)",
                            smallFont2, brush, 20, 200);
                        g.DrawString(
                            $"Апгрейд (1): {upCost.wood}Д {upCost.stone}К {upCost.gold}З + {bpCost}Ч",
                            smallFont2, brush, 20, 212);
                    }
                    else
                    {
                        var tmp = new AutoBuilding(AutoBuildingType.Sawmill, 0, 0);
                        var cost = tmp.GetBuildCost();
                        int bpCost = tmp.GetBuildBlueprintCost();

                        g.DrawString(
                            "Лесопилка: нет (1 - построить)",
                            smallFont2, brush, 20, 200);
                        g.DrawString(
                            $"Цена: {cost.wood}Д {cost.stone}К {cost.gold}З + {bpCost}Ч",
                            smallFont2, brush, 20, 212);
                    }

                    // Каменоломня
                    var quarry = engine.AutoBuildings.Find(b => b.Type == AutoBuildingType.Quarry);
                    if (quarry != null)
                    {
                        var upCost = quarry.GetUpgradeCost();
                        int bpCost = quarry.GetUpgradeBlueprintCost();

                        g.DrawString(
                            $"Каменоломня ур.{quarry.Level} (+{quarry.Level}/с камня)",
                            smallFont2, brush, 20, 230);
                        g.DrawString(
                            $"Апгрейд (2): {upCost.wood}Д {upCost.stone}К {upCost.gold}З + {bpCost}Ч",
                            smallFont2, brush, 20, 242);
                    }
                    else
                    {
                        var tmp = new AutoBuilding(AutoBuildingType.Quarry, 0, 0);
                        var cost = tmp.GetBuildCost();
                        int bpCost = tmp.GetBuildBlueprintCost();

                        g.DrawString(
                            "Каменоломня: нет (2 - построить)",
                            smallFont2, brush, 20, 230);
                        g.DrawString(
                            $"Цена: {cost.wood}Д {cost.stone}К {cost.gold}З + {bpCost}Ч",
                            smallFont2, brush, 20, 242);
                    }

                    // Золотая шахта
                    var goldMine = engine.AutoBuildings.Find(b => b.Type == AutoBuildingType.GoldMine);
                    if (goldMine != null)
                    {
                        var upCost = goldMine.GetUpgradeCost();
                        int bpCost = goldMine.GetUpgradeBlueprintCost();

                        g.DrawString(
                            $"Шахта ур.{goldMine.Level} (+{goldMine.Level}/с золота)",
                            smallFont2, brush, 20, 260);
                        g.DrawString(
                            $"Апгрейд (3): {upCost.wood}Д {upCost.stone}К {upCost.gold}З + {bpCost}Ч",
                            smallFont2, brush, 20, 272);
                    }
                    else
                    {
                        var tmp = new AutoBuilding(AutoBuildingType.GoldMine, 0, 0);
                        var cost = tmp.GetBuildCost();
                        int bpCost = tmp.GetBuildBlueprintCost();

                        g.DrawString(
                            "Шахта: нет (3 - построить)",
                            smallFont2, brush, 20, 260);
                        g.DrawString(
                            $"Цена: {cost.wood}Д {cost.stone}К {cost.gold}З + {bpCost}Ч",
                            smallFont2, brush, 20, 272);
                    }
                }
            }

            // Подсказки управления (внизу экрана)
            using (Font smallFont = new Font("Arial", 9))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                string controls = engine.IsInCave
                    ? "WASD/Стрелки - движение | E - добывать | R/ЛКМ - удар | Q - выход из пещеры | ESC - пауза"
                    : "WASD/Стрелки - движение | E - добывать/войти | Пробел - улучшить базу | 1/2/3 - автодобыча | ESC - пауза";

                g.DrawString(controls, smallFont, brush, 10, this.ClientSize.Height - 25);
            }
        }

        /// <summary>
        /// Загрузка текстуры удара из Textures/attack.png.
        /// </summary>
        private void EnsureAttackTextureLoaded()
        {
            if (attackFrames != null) return;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string[] files = new[]
                {
            "attack_0.png",
            "attack_1.png",
            "attack_2.png"
        };

                attackFrames = new Image[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    string f = files[i];

                    string path1 = System.IO.Path.Combine(baseDir, "Textures", f);
                    string path2 = System.IO.Path.Combine(baseDir, "..", "..", "Textures", f);
                    string path3 = System.IO.Path.Combine(baseDir, f);

                    if (System.IO.File.Exists(path1))
                        attackFrames[i] = Image.FromFile(path1);
                    else if (System.IO.File.Exists(path2))
                        attackFrames[i] = Image.FromFile(path2);
                    else if (System.IO.File.Exists(path3))
                        attackFrames[i] = Image.FromFile(path3);
                    else
                        attackFrames[i] = null;
                }
            }
            catch
            {
                attackFrames = null;
            }
        }


        /// <summary>
        /// Показать уведомление на экране на пару секунд.
        /// </summary>
        private void ShowNotification(string message, Color color)
        {
            notifications.Add(new Notification
            {
                Message = message,
                Color = color,
                TimeLeft = 2.0f
            });
        }

        /// <summary>
        /// Отрисовка всех активных уведомлений.
        /// </summary>
        private void DrawNotifications(Graphics g)
        {
            for (int i = notifications.Count - 1; i >= 0; i--)
            {
                // Вычитаем время жизни (примерно 0.016f за кадр)
                notifications[i].TimeLeft -= 0.016f;

                if (notifications[i].TimeLeft <= 0)
                {
                    notifications.RemoveAt(i);
                    continue;
                }

                float alpha = Math.Min(1.0f, notifications[i].TimeLeft);
                int alphaValue = (int)(alpha * 255);

                using (Font font = new Font("Arial", 14, FontStyle.Bold))
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(alphaValue / 2, Color.Black)))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(alphaValue, notifications[i].Color)))
                {
                    SizeF textSize = g.MeasureString(notifications[i].Message, font);
                    float x = (this.ClientSize.Width - textSize.Width) / 2;
                    float y = 100 + i * 40;

                    g.FillRectangle(bgBrush, x - 10, y - 5, textSize.Width + 20, textSize.Height + 10);
                    g.DrawString(notifications[i].Message, font, textBrush, x, y);
                }
            }
        }

        /// <summary>
        /// Отрисовка экрана паузы.
        /// </summary>
        private void DrawPauseScreen(Graphics g)
        {
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                g.FillRectangle(overlay, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            using (Font font = new Font("Arial", 36, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                string text = "ПАУЗА";
                SizeF textSize = g.MeasureString(text, font);
                float x = (this.ClientSize.Width - textSize.Width) / 2;
                float y = (this.ClientSize.Height - textSize.Height) / 2;
                g.DrawString(text, font, brush, x, y);
            }

            using (Font smallFont = new Font("Arial", 14))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                string text = "Нажмите ESC для продолжения";
                SizeF textSize = g.MeasureString(text, smallFont);
                float x = (this.ClientSize.Width - textSize.Width) / 2;
                float y = this.ClientSize.Height / 2 + 50;
                g.DrawString(text, smallFont, brush, x, y);

                // Подсказка выхода из игры
                string exitText = "Нажмите Backspace для выхода из игры";
                SizeF exitSize = g.MeasureString(exitText, smallFont);
                float ex = (this.ClientSize.Width - exitSize.Width) / 2;
                float ey = y + 30;
                g.DrawString(exitText, smallFont, brush, ex, ey);

            }
        }

        /// <summary>
        /// Обработка нажатия клавиш.
        /// </summary>
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Пауза
            if (e.KeyCode == Keys.Escape)
            {
                isPaused = !isPaused;
                this.Invalidate();
                return;
            }

            // В режиме паузы допускаем только выход в меню
            if (isPaused)
            {
                if (e.KeyCode == Keys.Back)
                {
                    // Закрываем игровую форму -> MainMenuForm снова покажет себя
                    this.Close();
                }
                return;
            }



            if (isPaused) return;

            // Движение
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) keyUp = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) keyDown = true;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) keyLeft = true;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) keyRight = true;

            // Добыча ресурсов / Вход в пещеру
            if (e.KeyCode == Keys.E)
            {
                if (!engine.IsInCave && engine.TryEnterCave())
                {
                    ShowNotification("Вы вошли в пещеру! Берегитесь монстров!", Color.Orange);
                }
                else
                {
                    if (engine.StartMining())
                    {
                        ShowNotification("Добыча началась...", Color.FromArgb(78, 201, 176));
                    }
                }
            }

            // Выход из пещеры
            if (e.KeyCode == Keys.Q && engine.IsInCave)
            {
                engine.ExitCave();
                ShowNotification("Вы вернулись на поверхность", Color.FromArgb(78, 201, 176));
            }

            // Улучшение базы
            if (e.KeyCode == Keys.Space && !engine.IsInCave)
            {
                if (engine.TryUpgradeBase())
                {
                    ShowNotification(
                        $"База улучшена: {engine.PlayerBase.GetLevelName()}!",
                        Color.FromArgb(78, 201, 176)
                    );
                }
                else if (engine.PlayerBase.CanUpgrade())
                {
                    ShowNotification(
                        "Недостаточно ресурсов или чертежей!",
                        Color.FromArgb(255, 84, 89)
                    );
                }
            }

            // Автодобыча: 1 - лесопилка, 2 - каменоломня, 3 - золотая шахта
            if (!engine.IsInCave)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
                {
                    if (engine.TryBuildAutoBuilding(AutoBuildingType.Sawmill))
                        ShowNotification("Лесопилка построена!", Color.FromArgb(78, 201, 176));
                    else if (engine.TryUpgradeAutoBuilding(AutoBuildingType.Sawmill))
                        ShowNotification("Лесопилка улучшена!", Color.FromArgb(78, 201, 176));
                    else
                        ShowNotification("Не хватает ресурсов/чертежей на лесопилку!", Color.FromArgb(255, 84, 89));
                }

                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2)
                {
                    if (engine.TryBuildAutoBuilding(AutoBuildingType.Quarry))
                        ShowNotification("Каменоломня построена!", Color.FromArgb(78, 201, 176));
                    else if (engine.TryUpgradeAutoBuilding(AutoBuildingType.Quarry))
                        ShowNotification("Каменоломня улучшена!", Color.FromArgb(78, 201, 176));
                    else
                        ShowNotification("Не хватает ресурсов/чертежей на каменоломню!", Color.FromArgb(255, 84, 89));
                }

                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3)
                {
                    if (engine.TryBuildAutoBuilding(AutoBuildingType.GoldMine))
                        ShowNotification("Золотая шахта построена!", Color.FromArgb(78, 201, 176));
                    else if (engine.TryUpgradeAutoBuilding(AutoBuildingType.GoldMine))
                        ShowNotification("Золотая шахта улучшена!", Color.FromArgb(78, 201, 176));
                    else
                        ShowNotification("Не хватает ресурсов/чертежей на шахту!", Color.FromArgb(255, 84, 89));
                }
            }

            // Удар по монстру (R)
            if (e.KeyCode == Keys.R)
            {
                if (engine.TryAttackMonster())
                {
                    // Уведомление необязательно, чтобы не спамить
                }
            }
        }

        /// <summary>
        /// Обработка отпускания клавиш.
        /// </summary>
        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) keyUp = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) keyDown = false;
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) keyLeft = false;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) keyRight = false;

            // Остановка добычи при отпускании E
            if (e.KeyCode == Keys.E)
            {
                engine.StopMining();
            }
        }

        /// <summary>
        /// Обработка клика мыши (ЛКМ = удар).
        /// </summary>
        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (isPaused) return;

            if (e.Button == MouseButtons.Left)
            {
                if (engine.TryAttackMonster())
                {
                    // Можно вывести короткий эффект, но чтобы не спамить уведомлениями, оставим тихо
                }
            }
        }

        /// <summary>
        /// Базовая инициализация формы (генерируется дизайнером).
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1024, 768);
            this.Name = "GameForm";
            this.ResumeLayout(false);
        }
    }
}
