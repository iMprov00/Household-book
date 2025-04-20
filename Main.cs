using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Household_book
{
    public partial class Main : Form
    {
        private bool isDragging = false;
        private Point lastCursorPos;

        public Main()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void bunifuFormControlBox1_HelpClicked(object sender, EventArgs e)
        {

        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastCursorPos = e.Location;
            }
        }

        private void menuStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - lastCursorPos.X;
                this.Top += e.Y - lastCursorPos.Y;
            }
        }

        private void menuStrip1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void text_date_KeyPress(object sender, KeyPressEventArgs e)
        {
            var textBox = (Bunifu.UI.WinForms.BunifuTextBox)sender;

            // Разрешаем только цифры и Backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
                return;
            }

            // Автоматическая расстановка точек
            if (e.KeyChar != '\b' && (textBox.Text.Length == 2 || textBox.Text.Length == 5))
            {
                textBox.Text += ".";
                textBox.SelectionStart = textBox.Text.Length;
            }

            // Ограничение длины (10 символов)
            if (textBox.Text.Length >= 10)
                e.Handled = true;
        }
    }
}
