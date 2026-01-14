using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shoot_Out_Game_MOO_ICT
{
    public partial class MainMenuForm : Form
    {
        // Настройки игры
        public static int GameWidth = 1600;
        public static int GameHeight = 900;
        public static Color RestRoomColor = Color.FromArgb(0, 100, 0); // Темно-зеленый

        public MainMenuForm()
        {
            InitializeComponent();
            CenterToScreen();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Запускаем игру с текущими настройками
            Form1 gameForm = new Form1();
            gameForm.StartPosition = FormStartPosition.CenterScreen;
            gameForm.Show();
            this.Hide();

            gameForm.FormClosed += (s, args) =>
            {
                this.Show();
                this.CenterToScreen();
            };
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            // Открываем настройки
            SettingsForm settings = new SettingsForm();
            settings.StartPosition = FormStartPosition.CenterParent;
            if (settings.ShowDialog() == DialogResult.OK)
            {
                // Применяем новые настройки
                GameWidth = settings.SelectedWidth;
                GameHeight = settings.SelectedHeight;
                RestRoomColor = settings.SelectedRestColor;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {
            // Загружаем последние настройки (если бы было сохранение)
            // Пока используем значения по умолчанию
        }
    }
}