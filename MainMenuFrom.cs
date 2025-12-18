using System;
using System.Drawing;
using System.Windows.Forms;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Главное меню игры
    /// Предоставляет кнопки для начала игры, настроек и выхода
    /// </summary>
    public partial class MainMenuForm : Form
    {
        private Button btnStart;
        private Button btnSettings;
        private Button btnExit;
        private Label lblTitle;

        public MainMenuForm()
        {
            InitializeComponent();
            SetupForm();
        }

        /// <summary>
        /// Настройка формы и создание элементов интерфейса
        /// </summary>
        private void SetupForm()
        {
            // Настройка формы
            this.Text = "Resource Collector - Главное меню";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Заголовок
            lblTitle = new Label
            {
                Text = "🎮 RESOURCE COLLECTOR",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(78, 201, 176),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(500, 60),
                Location = new Point(50, 50)
            };
            this.Controls.Add(lblTitle);

            // Кнопка "Начать игру"
            btnStart = CreateMenuButton("▶ Начать игру", 150);
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            // Кнопка "Настройки"
            btnSettings = CreateMenuButton("⚙ Настройки", 210);
            btnSettings.Click += BtnSettings_Click;
            this.Controls.Add(btnSettings);

            // Кнопка "Выход"
            btnExit = CreateMenuButton("✕ Выход", 270);
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);
        }

        /// <summary>
        /// Создание кнопки меню с единым стилем
        /// </summary>
        private Button CreateMenuButton(string text, int y)
        {
            return new Button
            {
                Text = text,
                Size = new Size(250, 45),
                Location = new Point(175, y),
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Начать игру"
        /// </summary>
        private void BtnStart_Click(object sender, EventArgs e)
        {
            this.Hide();
            GameForm gameForm = new GameForm();
            gameForm.FormClosed += (s, args) => this.Show();
            gameForm.Show();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Настройки"
        /// </summary>
        private void BtnSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Выход"
        /// </summary>
        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(600, 400);
            this.Name = "MainMenuForm";
            this.ResumeLayout(false);
        }
    }
}