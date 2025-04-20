using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using Dapper;
using System.IO;


namespace Household_book
{

    public partial class Authorization : Form
    {
        private string login;
        private string pass;

        private bool isDragging = false;
        private Point lastCursorPos;

        public Authorization()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();

        }

        private void bunifuTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void bunifuLabel2_Click(object sender, EventArgs e)
        {

        }

        private void Authorization_Load(object sender, EventArgs e)
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

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {

        }

        private void button_go_Click(object sender, EventArgs e)
        {

            login = text_login.Text;
            pass = text_pass.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show(
                    "Пожалуйста, введите логин и пароль!",  // Текст сообщения
                    "Ошибка ввода",                         // Заголовок окна
                    MessageBoxButtons.OK,                   // Кнопка "OK"
                    MessageBoxIcon.Warning                 // Иконка предупреждения
                );
                return;
            }

            if (login != "1" || pass != "11")
            {
                MessageBox.Show(
                    "Неверный логин или пароль!",  // Текст сообщения
                    "Ошибка ввода",                         // Заголовок окна
                    MessageBoxButtons.OK,                   // Кнопка "OK"
                    MessageBoxIcon.Warning                 // Иконка предупреждения
                );
            }
            else
            {

                this.Close();
            }
            
        }
    }
}
