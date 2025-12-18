using System;
using System.Drawing;
using System.Windows.Forms;

namespace ResourceCollectorGame
{
    /// <summary>
    /// Форма настроек игры
    /// Позволяет выбрать разрешение экрана
    /// </summary>
    public partial class SettingsForm : Form
    {
        private ComboBox cmbResolution;
        private Button btnSave;
        private Button btnCancel;
        private Label lblResolution;
        private CheckBox chkFullscreen;
        private TrackBar trackSfx;
        private Label lblSfx;

        public SettingsForm()
        {
            InitializeComponent();
            SetupForm();
        }

        /// <summary>
        /// Настройка формы настроек
        /// </summary>
        private void SetupForm()
        {
            this.Text = "Настройки";
            this.Size = new Size(420, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Метка "Разрешение"
            lblResolution = new Label
            {
                Text = "Разрешение экрана:",
                Location = new Point(30, 40),
                Size = new Size(200, 25),
                Font = new Font("Arial", 11),
                ForeColor = Color.White
            };
            this.Controls.Add(lblResolution);

            // Комбобокс выбора разрешения
            cmbResolution = new ComboBox
            {
                Location = new Point(30, 70),
                Size = new Size(330, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Arial", 10)
            };
            cmbResolution.Items.AddRange(new object[] {
                "800 x 600",
                "1024 x 768",
                "1280 x 720",
                "1920 x 1080"
            });
            // Чекбокс "Полный экран"
            chkFullscreen = new CheckBox
            {
                Text = "Полный экран",
                Location = new Point(30, 110),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Checked = GameSettings.IsFullscreen
            };
            this.Controls.Add(chkFullscreen);

            // Метка "Громкость эффектов"
            lblSfx = new Label
            {
                Text = "Громкость эффектов:",
                Location = new Point(30, 190),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10),
                ForeColor = Color.White
            };
            this.Controls.Add(lblSfx);

            // Ползунок эффектов
            trackSfx = new TrackBar
            {
                Location = new Point(30, 210),
                Size = new Size(330, 30),
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 10,
                Value = (int)(GameSettings.SfxVolume * 100)
            };
            this.Controls.Add(trackSfx);


            // Установка текущего разрешения
            if (GameSettings.Resolution.Width == 800)
                cmbResolution.SelectedIndex = 0;
            else if (GameSettings.Resolution.Width == 1024)
                cmbResolution.SelectedIndex = 1;
            else if (GameSettings.Resolution.Width == 1280)
                cmbResolution.SelectedIndex = 2;
            else
                cmbResolution.SelectedIndex = 3;

            this.Controls.Add(cmbResolution);

            // Кнопка "Сохранить"
            btnSave = new Button
            {
                Text = "✓ Сохранить",
                Location = new Point(30, 140),
                Size = new Size(150, 40),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(78, 201, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Кнопка "Отмена"
            btnCancel = new Button
            {
                Text = "✕ Отмена",
                Location = new Point(210, 140),
                Size = new Size(150, 40),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        /// <summary>
        /// Обработчик сохранения настроек
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            switch (cmbResolution.SelectedIndex)
            {

                case 0:
                    GameSettings.Resolution = new Size(800, 600);
                    break;
                case 1:
                    GameSettings.Resolution = new Size(1024, 768);
                    break;
                case 2:
                    GameSettings.Resolution = new Size(1280, 720);
                    break;
                case 3:
                    GameSettings.Resolution = new Size(1920, 1080);
                    break;
            }
            // Сохранение флага полного экрана
            GameSettings.IsFullscreen = chkFullscreen.Checked;
            GameSettings.SfxVolume = trackSfx.Value / 100f;


            MessageBox.Show("Настройки сохранены! Новое разрешение будет применено при следующем запуске игры.",
                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(420, 360);
            this.Name = "SettingsForm";
            this.ResumeLayout(false);
        }
    }
}