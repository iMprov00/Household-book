using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using Dapper;
using System.IO;
using static Household_book.Authorization;

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
            LoadPeopleData(); // Загружаем данные при создании формы
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        public static class Database_pl
        {
            private static string DatabaseFile = "household_book.db";
            private static string ConnectionString => $"Data Source={DatabaseFile};Version=3;";

            public static IEnumerable<Person> GetAllPeople()
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    return connection.Query<Person>("SELECT * FROM people");
                }
            }

            public class Person
            {
                public int person_id { get; set; }
                public string full_name { get; set; }
                public string birth_date { get; set; }
                public string address { get; set; }
            }
        }

        private void LoadPeopleData()
        {
            try
            {
                // Получаем данные из БД
                var people = Database_pl.GetAllPeople().ToList();

                // Настраиваем DataGridView
                bunifuDataGridView1.AutoGenerateColumns = false;
                bunifuDataGridView1.DataSource = people;

                // Добавляем колонки, если их нет
                if (bunifuDataGridView1.Columns.Count == 0)
                {
                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "person_id",
                        HeaderText = "ID",
                        Width = 50
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "full_name",
                        HeaderText = "ФИО",
                        Width = 200
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "birth_date",
                        HeaderText = "Дата рождения",
                        Width = 100
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        DataPropertyName = "address",
                        HeaderText = "Адрес",
                        Width = 250
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Настройка цветов
            bunifuDataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240); // Цвет четных строк
            bunifuDataGridView1.RowsDefaultCellStyle.BackColor = Color.White; // Цвет нечетных строк
            bunifuDataGridView1.BackgroundColor = Color.White; // Цвет фона

            // Цвет выделенной строки
            bunifuDataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.LightGray;
            bunifuDataGridView1.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.LightGray;

            // Цвет заголовков
            bunifuDataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 51, 51);
            bunifuDataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

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

        private void button_pl_Click(object sender, EventArgs e)
        {
            LoadPeopleData();
        }

        private void bunifuDataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
