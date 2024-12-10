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
        private List<Student> _list; // �������� ������ ���������

        public MainForm()
        {
            InitializeComponent();
            _list = new List<Student>(); // ������������� ������ ���������
        }

        // ����� ��� �������� ������ �� �����
        private void loadingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var filePath = "C:\\Users\\lenovo\\OneDrive\\������� ����\\Lab01_DataBase-master\\Lab01\\bin\\Debug\\students.txt";

                // �������� ������� �����
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"���� {filePath} �� ������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var lines = File.ReadAllLines(filePath);

                // ��������, ��� ���� �� ����
                if (lines.Length == 0)
                {
                    MessageBox.Show("���� ����.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ������� ����� ����� � ���������� ������ ���������
                _list = lines
                    .Select((line, index) =>
                    {
                        try
                        {
                            var data = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            // �������� ���������� ��������� � ������
                            if (data.Length != 7)
                                throw new FormatException($"������ {index + 1} ����� �������� ������: {line}");

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
                            MessageBox.Show($"������ � ������ {index + 1}: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null; // ������� null ��� �������� �������� ������
                        }
                    })
                    .Where(student => student != null) // ���������� null ��������
                    .Cast<Student>() // ���������� ����
                    .ToList();

                // ��������, ��������� �� ������
                if (_list.Count == 0)
                {
                    MessageBox.Show("�� ������� ��������� �� ����� ������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"���� ������� ��������. ��������� ���������: {_list.Count}", "����������", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // ���������� ������� � ������ ���������
                statusStrip1.Items[1].Text = _list.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ����� ��� ���������� ������ � ����������� ������
        private void ExecuteTask(Func<Student, bool> predicate, Func<Student, string> resultSelector, double timeoutMinutes)
        {
            rtbResult.Clear(); // ������� �����������

            // �������� ������� ������
            if (_list == null || !_list.Any())
            {
                MessageBox.Show("������ ��������� ����. ��������� ������ �� �����.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var results = _list.Where(predicate) // ���������� ���������
                .TakeWhile(_ => stopwatch.Elapsed.TotalMinutes < timeoutMinutes) // ����������� �� �������
                .Select(resultSelector) // ������������ �����������
                .ToList();

            // �������� ������� �����������
            if (!results.Any())
            {
                MessageBox.Show("������ �����������.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                rtbResult.Text = string.Join(Environment.NewLine, results); // ����� �����������
            }

            // �������� ������� ����������
            if (stopwatch.Elapsed.TotalMinutes >= timeoutMinutes)
            {
                MessageBox.Show($"����� �������� ��������� {timeoutMinutes} �����.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            stopwatch.Stop();
        }

        // ������ 1: ����� �������� �� �������
        private void task1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var stSurname = tb1Surname?.Text?.Trim(); // ��������� �����
            if (string.IsNullOrEmpty(stSurname))
            {
                MessageBox.Show("������� ������� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExecuteTask(
                student => student.StLastName.Equals(stSurname, StringComparison.OrdinalIgnoreCase),
                student => student.ToStringStudentClassTeacher(),
                5);
        }

        // ������ 2: ����� �������� �������� �� �������
        private void task2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var stSurname = tb2Surname?.Text?.Trim(); // ��������� �����
            if (string.IsNullOrEmpty(stSurname))
            {
                MessageBox.Show("������� ������� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExecuteTask(
                student => student.StLastName.Equals(stSurname, StringComparison.OrdinalIgnoreCase),
                student => student.ToStringStudentBus(),
                2);
        }

        // ������ 3: ����� ��������� �� ����� � ������� �������
        private void task3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tSurname = tb3Surname?.Text?.Trim(); // ������� �������
            var tName = tbName?.Text?.Trim(); // ��� �������

            if (string.IsNullOrEmpty(tSurname) || string.IsNullOrEmpty(tName))
            {
                MessageBox.Show("������� ������� � ��� �������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExecuteTask(
                student => student.TLastName.Equals(tSurname, StringComparison.OrdinalIgnoreCase) &&
                           student.TFirstName.Equals(tName, StringComparison.OrdinalIgnoreCase),
                student => student.ToStringTeacherStudents(),
                2);
        }

        // ������ 4: ����� ��������� �� ������ ��������
        private void task4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tbNumberBus?.Text?.Trim(), out var stBus)) // ��������� ������ ��������
            {
                ExecuteTask(
                    student => student.Bus == stBus,
                    student => student.ToStringBusStudent(),
                    2);
            }
            else
            {
                MessageBox.Show("������������ ������ ������ ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ������ 5: ����� ��������� �� ������ ������
        private void task5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tbNumberGrade?.Text?.Trim(), out var stGrade)) // ��������� ������ ������
            {
                ExecuteTask(
                    student => student.Grade == stGrade,
                    student => student.ToStringStudentGrade(),
                    2);
            }
            else
            {
                MessageBox.Show("������������ ������ ������ ������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
