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
    public partial class SettingsForm : Form
    {
        public int SelectedWidth { get; private set; }
        public int SelectedHeight { get; private set; }
        public Color SelectedRestColor { get; private set; }

        public SettingsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Загружаем текущие настройки из главного меню
            SelectedWidth = MainMenuForm.GameWidth;
            SelectedHeight = MainMenuForm.GameHeight;
            SelectedRestColor = MainMenuForm.RestRoomColor;

            // Устанавливаем выбранные значения в комбобоксы
            cmbResolution.SelectedIndex = GetResolutionIndex(SelectedWidth, SelectedHeight);

            // Устанавливаем цвет
            colorPicker.BackColor = SelectedRestColor;
            UpdateColorPreview();
        }

        private int GetResolutionIndex(int width, int height)
        {
            if (width == 1024 && height == 768) return 0;
            if (width == 1280 && height == 720) return 1;
            if (width == 1366 && height == 768) return 2;
            if (width == 1600 && height == 900) return 3;
            if (width == 1920 && height == 1080) return 4;
            return 3; // По умолчанию 1600x900
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Применяем настройки
            ApplyResolution();
            ApplyColor();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ApplyResolution()
        {
            switch (cmbResolution.SelectedIndex)
            {
                case 0: // 1024x768
                    SelectedWidth = 1024;
                    SelectedHeight = 768;
                    break;
                case 1: // 1280x720
                    SelectedWidth = 1280;
                    SelectedHeight = 720;
                    break;
                case 2: // 1366x768
                    SelectedWidth = 1366;
                    SelectedHeight = 768;
                    break;
                case 3: // 1600x900
                    SelectedWidth = 1600;
                    SelectedHeight = 900;
                    break;
                case 4: // 1920x1080
                    SelectedWidth = 1920;
                    SelectedHeight = 1080;
                    break;
                default:
                    SelectedWidth = 1600;
                    SelectedHeight = 900;
                    break;
            }
        }

        private void ApplyColor()
        {
            SelectedRestColor = colorPicker.BackColor;
        }

        private void colorPicker_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = colorPicker.BackColor;
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            colorDialog.AnyColor = true;
            colorDialog.SolidColorOnly = false;
            colorDialog.CustomColors = new int[] { 0x006400, 0x008000, 0x228B22, 0x32CD32 };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorPicker.BackColor = colorDialog.Color;
                UpdateColorPreview();
            }
        }

        private void UpdateColorPreview()
        {
            colorPreview.BackColor = colorPicker.BackColor;
            lblColorHex.Text = $"RGB: {colorPicker.BackColor.R}, {colorPicker.BackColor.G}, {colorPicker.BackColor.B}";
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            // Сброс к настройкам по умолчанию
            cmbResolution.SelectedIndex = 3; // 1600x900
            colorPicker.BackColor = Color.FromArgb(0, 100, 0); // Темно-зеленый
            UpdateColorPreview();
        }
    }
}