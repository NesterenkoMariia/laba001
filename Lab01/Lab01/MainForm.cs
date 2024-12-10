using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Lab01
{
    public partial class MainForm : Form
    {
        private List<Student> _list; // Хранение списка студентов

        public MainForm()
        {
            InitializeComponent();
            _list = new List<Student>(); // Инициализация списка студентов
        }

        // Метод для загрузки данных из файла
        private void loadingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var filePath = "C:\\Users\\lenovo\\OneDrive\\Рабочий стол\\Lab01_DataBase-master\\Lab01\\bin\\Debug\\students.txt";

                // Проверка наличия файла
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Файл {filePath} не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var lines = File.ReadAllLines(filePath);

                // Проверка, что файл не пуст
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Парсинг строк файла и заполнение списка студентов
                _list = lines
                    .Select((line, index) =>
                    {
                        try
                        {
                            var data = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            // Проверка количества элементов в строке
                            if (data.Length != 7)
                                throw new FormatException($"Строка {index + 1} имеет неверный формат: {line}");

                            return new Student
                            {
                                StLastName = data[0].Trim(),
                                StFirstName = data[1].Trim(),
                                Grade = int.Parse(data[2]),
                                Classroom = int.Parse(data[3]),
                                Bus = int.Parse(data[4]),
                                TLastName = data[5].Trim(),
                                TFirstName = data[6].Trim()
                            };
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка в строке {index + 1}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null; // Возврат null для пропуска неверной строки
                        }
                    })
                    .Where(student => student != null) // Исключение null значений
                    .Cast<Student>() // Приведение типа
                    .ToList();

                // Проверка, загружены ли данные
                if (_list.Count == 0)
                {
                    MessageBox.Show("Не удалось загрузить ни одной записи.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Файл успешно загружен. Загружено студентов: {_list.Count}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Обновление статуса в строке состояния
                statusStrip1.Items[1].Text = _list.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для выполнения задачи с фильтрацией данных
        private void ExecuteTask(Func<Student, bool> predicate, Func<Student, string> resultSelector, double timeoutMinutes)
        {
            rtbResult.Clear(); // Очистка результатов

            // Проверка наличия данных
            if (_list == null || !_list.Any())
            {
                MessageBox.Show("Список студентов пуст. Загрузите данные из файла.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var results = _list.Where(predicate) // Фильтрация студентов
                .TakeWhile(_ => stopwatch.Elapsed.TotalMinutes < timeoutMinutes) // Ограничение по времени
                .Select(resultSelector) // Формирование результатов
                .ToList();

            // Проверка наличия результатов
            if (!results.Any())
            {
                MessageBox.Show("Данные отсутствуют.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                rtbResult.Text = string.Join(Environment.NewLine, results); // Вывод результатов
            }

            // Проверка времени выполнения
            if (stopwatch.Elapsed.TotalMinutes >= timeoutMinutes)
            {
                MessageBox.Show($"Время ожидания превысило {timeoutMinutes} минут.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            stopwatch.Stop();
        }

        // Задача 1: Поиск студента по фамилии
        private void task1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var stSurname = tb1Surname?.Text?.Trim(); // Получение ввода
            if (string.IsNullOrEmpty(stSurname))
            {
                MessageBox.Show("Введите фамилию студента.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExecuteTask(
                student => student.StLastName.Equals(stSurname, StringComparison.OrdinalIgnoreCase),
                student => student.ToStringStudentClassTeacher(),
                5);
        }

        // Задача 2: Поиск автобуса студента по фамилии
        private void task2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var stSurname = tb2Surname?.Text?.Trim(); // Получение ввода
            if (string.IsNullOrEmpty(stSurname))
            {
                MessageBox.Show("Введите фамилию студента.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExecuteTask(
                student => student.StLastName.Equals(stSurname, StringComparison.OrdinalIgnoreCase),
                student => student.ToStringStudentBus(),
                2);
        }

        // Задача 3: Поиск студентов по имени и фамилии учителя
        private void task3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tSurname = tb3Surname?.Text?.Trim(); // Фамилия учителя
            var tName = tbName?.Text?.Trim(); // Имя учителя

            if (string.IsNullOrEmpty(tSurname) || string.IsNullOrEmpty(tName))
            {
                MessageBox.Show("Введите фамилию и имя учителя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExecuteTask(
                student => student.TLastName.Equals(tSurname, StringComparison.OrdinalIgnoreCase) &&
                           student.TFirstName.Equals(tName, StringComparison.OrdinalIgnoreCase),
                student => student.ToStringTeacherStudents(),
                2);
        }

        // Задача 4: Поиск студентов по номеру автобуса
        private void task4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tbNumberBus?.Text?.Trim(), out var stBus)) // Получение номера автобуса
            {
                ExecuteTask(
                    student => student.Bus == stBus,
                    student => student.ToStringBusStudent(),
                    2);
            }
            else
            {
                MessageBox.Show("Неправильный формат номера автобуса.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Задача 5: Поиск студентов по номеру класса
        private void task5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tbNumberGrade?.Text?.Trim(), out var stGrade)) // Получение номера класса
            {
                ExecuteTask(
                    student => student.Grade == stGrade,
                    student => student.ToStringStudentGrade(),
                    2);
            }
            else
            {
                MessageBox.Show("Неправильный формат номера класса.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
