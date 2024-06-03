using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace _14._05
{
    public partial class SolutionForm : Form
    {
        private List<string> solutions = new List<string>();

        public SolutionForm()
        {
            InitializeComponent();
        }

        public void SetSolutions(List<string> solutions)
        {
            this.solutions = solutions;
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            listBox1.Items.Clear();

            if (comboBox2.SelectedIndex == 2)
            {
                foreach (var solution in solutions)
                {
                    if (IsValueDivisibleByTwo(solution))
                    {
                        listBox1.Items.Add(solution);
                    }
                }
            }
            else if (comboBox2.SelectedIndex == 3)
            {
                foreach (var solution in solutions)
                {
                    if (IsValueDivisibleByThree(solution))
                    {
                        listBox1.Items.Add(solution);
                    }
                }
            }
            else if (comboBox2.SelectedIndex == 4)
            {
                foreach (var solution in solutions)
                {
                    if (IsValueDivisibleByFive(solution))
                    {
                        listBox1.Items.Add(solution);
                    }
                }
            }
            else
            {
                listBox1.Items.AddRange(solutions.ToArray());
            }

            label1.Text = $"Решения: {listBox1.Items.Count}";
        }

        private double ExtractNumericValue(string solution)
        {
            string[] parts = solution.Split(new[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                string numericPart = parts[1].Split('#')[0].Trim();
                if (double.TryParse(numericPart, out double value))
                {
                    return value;
                }
            }
            throw new FormatException("Невозможно извлечь числовое значение из строки решения.");
        }

        private bool IsValueDivisibleByTwo(string solution)
        {
            return ExtractNumericValue(solution) % 2 == 0;
        }

        private bool IsValueDivisibleByThree(string solution)
        {
            return ExtractNumericValue(solution) % 3 == 0;
        }

        private bool IsValueDivisibleByFive(string solution)
        {
            return ExtractNumericValue(solution) % 5 == 0;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0: // По возрастанию
                    SortSolutionsAscending();
                    break;
                case 1: // По убыванию
                    SortSolutionsDescending();
                    break;
                case 2: // По делимости на 2
                case 3: // По делимости на 3
                case 4: // По делимости на 5
                    UpdateListBox();
                    break;
            }
        }

        private void SortSolutionsAscending()
        {
            solutions.Sort((x, y) =>
            {
                double xNum = ExtractNumericValue(x);
                double yNum = ExtractNumericValue(y);
                return xNum.CompareTo(yNum);
            });
            UpdateListBox();
        }

        private void SortSolutionsDescending()
        {
            solutions.Sort((x, y) =>
            {
                double xNum = ExtractNumericValue(x);
                double yNum = ExtractNumericValue(y);
                return yNum.CompareTo(xNum);
            });
            UpdateListBox();
        }
    }
}
