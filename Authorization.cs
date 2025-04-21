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
            Database.Initialize();

        }

        public static class Database
        {
            private static string DatabaseFile = "household_book.db";
            private static string ConnectionString => $"Data Source={DatabaseFile};Version=3;";

            public static void Initialize()
            {
                if (!File.Exists(DatabaseFile))
                {
                    SQLiteConnection.CreateFile(DatabaseFile);

                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(@"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Login TEXT NOT NULL UNIQUE,
                            Pass TEXT NOT NULL
                        );

                    ");
                    }
                }
            }

            public static bool ValidateUser(string login, string password)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    var user = connection.QueryFirstOrDefault<User>(
                        "SELECT * FROM Users WHERE Login = @Login AND Pass = @Pass",
                        new { Login = login, Pass = password });

                    return user != null;
                }
            }

            public class User
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string Login { get; set; }
                public string Pass { get; set; }
            }
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

        private async Task AnimateClose()
        {
            int duration = 300;
            int steps = 10;
            float opacityStep = 1f / steps;
            int yStep = this.Height / steps;

            for (int i = 0; i < steps; i++)
            {
                this.Opacity -= opacityStep;
                this.Top -= yStep;
                await Task.Delay(duration / steps);
            }

            this.Hide();
        }

        private async Task AnimateShow(Form form)
        {
            form.Opacity = 0;
            form.Show();

            int duration = 300;
            int steps = 20;
            float opacityStep = 1f / steps;
            int yStep = form.Height / steps;

            // Начальная позиция (форма появляется снизу)
            form.Top += yStep * steps / 2;

            for (int i = 0; i < steps; i++)
            {
                form.Opacity += opacityStep;
                form.Top -= yStep / 2;
                await Task.Delay(duration / steps);
            }

            form.Opacity = 1;
            form.Top = (Screen.PrimaryScreen.WorkingArea.Height - form.Height) / 2;
        }

        private async void button_go_Click(object sender, EventArgs e)
        {

            login = text_login.Text;
            pass = text_pass.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                ShowMessage("Пожалуйста, введите логин и пароль!",
                          "Ошибка ввода",
                          MessageBoxIcon.Warning);
                return;
            }

            if (Database.ValidateUser(login, pass))
            {

                Main mainForm = new Main();
                await AnimateClose();

                await AnimateShow(mainForm);

            }
            else
            {
                ShowMessage("Неверный логин или пароль!",
                          "Ошибка авторизации",
                          MessageBoxIcon.Error);
            }
        }

        private void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }
    }
}
