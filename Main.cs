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
using Bunifu.UI.WinForms;
using System.Drawing.Drawing2D;

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
            this.FormClosing += Main_FormClosing;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            ApplyRoundedCorners(bunifuDataGridView1, 15);
        }

        private void ApplyRoundedCorners(BunifuDataGridView dgv, int radius)
        {
            // Создаем GraphicsPath с закругленными углами
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, dgv.Width, dgv.Height);

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Верхний левый угол
            path.AddArc(rect.Width - radius, rect.Y, radius, radius, 270, 90); // Верхний правый угол
            path.AddArc(rect.Width - radius, rect.Height - radius, radius, radius, 0, 90); // Нижний правый угол
            path.AddArc(rect.X, rect.Height - radius, radius, radius, 90, 90); // Нижний левый угол
            path.CloseFigure();

            // Применяем регион к DataGridView
            dgv.Region = new Region(path);
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

            public static bool AddPerson(Person person)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "INSERT INTO people (full_name, birth_date, address) VALUES (@full_name, @birth_date, @address)",
                            person);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            public static bool UpdatePerson(Person person)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "UPDATE people SET full_name = @full_name, birth_date = @birth_date, address = @address WHERE person_id = @person_id",
                            person);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            public static bool DeletePerson(int personId)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "DELETE FROM people WHERE person_id = @personId",
                            new { personId });
                        return true;
                    }
                }
                catch
                {
                    return false;
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

                // Добавляем колонки только если их нет
                if (bunifuDataGridView1.Columns.Count == 0)
                {
                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "col_id", // Добавляем Name для обращения
                        DataPropertyName = "person_id",
                        HeaderText = "ID",
                        Width = 50
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "col_name",
                        DataPropertyName = "full_name",
                        HeaderText = "ФИО",
                        Width = 200
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "col_date",
                        DataPropertyName = "birth_date",
                        HeaderText = "Дата рождения",
                        Width = 100
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "col_address",
                        DataPropertyName = "address",
                        HeaderText = "Адрес",
                        Width = 250
                    });
                }

                // Устанавливаем источник данных
                bunifuDataGridView1.DataSource = people;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            // Настройка цветов
            bunifuDataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.PapayaWhip; // Цвет четных строк
            bunifuDataGridView1.RowsDefaultCellStyle.BackColor = Color.White; // Цвет нечетных строк
            bunifuDataGridView1.BackgroundColor = Color.White; // Цвет фона

            // Цвет выделенной строки
            bunifuDataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGray;
            bunifuDataGridView1.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.DarkGray;

            // Цвет заголовков
            bunifuDataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Goldenrod;
            bunifuDataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

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

        private void bunifuFormControlBox1_HelpClicked_1(object sender, EventArgs e)
        {
            this.Close();   
        }

        private void bunifuFormControlBox1_CloseClicked(object sender, EventArgs e)
        {

                DialogResult result = MessageBox.Show("Вы точно хотите выйти?", "Подтверждение выхода",
                                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Application.Exit(); // или this.Close();
                }
                // Если нет - ничего не делаем, форма не закроется
            
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

        private async void button_exit_Click(object sender, EventArgs e)
        {

            var result = MessageBox.Show("Вы точно хотите выйти из текущей учетной записи?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Authorization mainForm = new Authorization();
                await AnimateClose();

                await AnimateShow(mainForm);
            }

        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы точно хотите выйти?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void bunifuDataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (bunifuDataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = bunifuDataGridView1.SelectedRows[0];

                // Используем имена колонок, которые задали в LoadPeopleData
                text_id.Text = selectedRow.Cells["col_id"].Value?.ToString() ?? "";
                text_login.Text = selectedRow.Cells["col_name"].Value?.ToString() ?? "";
                date.Text = selectedRow.Cells["col_date"].Value?.ToString() ?? "";
                text_adress.Text = selectedRow.Cells["col_address"].Value?.ToString() ?? "";
            }
        }

        private void bunifuButton21_Click(object sender, EventArgs e)
        {
            // Добавление новой записи
            if (string.IsNullOrEmpty(text_login.Text) || string.IsNullOrEmpty(date.Text) || string.IsNullOrEmpty(text_adress.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newPerson = new Database_pl.Person
            {
                full_name = text_login.Text,
                birth_date = date.Text,
                address = text_adress.Text
            };

            if (Database_pl.AddPerson(newPerson))
            {
                MessageBox.Show("Запись успешно добавлена!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPeopleData(); // Обновляем данные в таблице
                ClearFields(); // Очищаем поля
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении записи!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bunifuButton22_Click(object sender, EventArgs e)
        {
            // Изменение существующей записи
            if (string.IsNullOrEmpty(text_id.Text))
            {
                MessageBox.Show("Не выбрана запись для изменения!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(text_login.Text) || string.IsNullOrEmpty(date.Text) || string.IsNullOrEmpty(text_adress.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var updatedPerson = new Database_pl.Person
            {
                person_id = int.Parse(text_id.Text),
                full_name = text_login.Text,
                birth_date = date.Text,
                address = text_adress.Text
            };

            if (Database_pl.UpdatePerson(updatedPerson))
            {
                MessageBox.Show("Запись успешно обновлена!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPeopleData(); // Обновляем данные в таблице
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении записи!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bunifuButton23_Click(object sender, EventArgs e)
        {
            // Удаление записи
            if (string.IsNullOrEmpty(text_id.Text))
            {
                MessageBox.Show("Не выбрана запись для удаления!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int personId = int.Parse(text_id.Text);

                if (Database_pl.DeletePerson(personId))
                {
                    MessageBox.Show("Запись успешно удалена!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPeopleData(); // Обновляем данные в таблице
                    ClearFields(); // Очищаем поля
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении записи!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearFields()
        {
            text_id.Text = "";
            text_login.Text = "";
            date.Text = "";
            text_adress.Text = "";
        }
    }
}
