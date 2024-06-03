using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _14._05
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Search_solutions_Click(object sender, EventArgs e)
        {
            string cryptarithm = textBox1.Text.ToUpper();

            if (string.IsNullOrWhiteSpace(cryptarithm))
            {
                MessageBox.Show("Пожалуйста, введите уравнение для решения.");
                return;
            }
            // Показываем ProgressBar и инициализируем его
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;

            await Task.Run(() => GenerateSolution(cryptarithm));

            // Скрываем ProgressBar после завершения задачи
            progressBar1.Visible = false;
           
        }

        private void GenerateSolution(string cryptarithm)
        {
            string[] sides = cryptarithm.Split('=');

            if (sides.Length != 2)
            {
                MessageBox.Show("Неверный формат уравнения.");
                return;
            }

            string leftSide = sides[0].Trim();
            string rightSide = sides[1].Trim();

            List<char> letters = leftSide.Where(char.IsLetter).Concat(rightSide.Where(char.IsLetter)).Distinct().ToList();

            if (letters.Count > 10)
            {
                MessageBox.Show("Слишком много различных букв для решения.");
                return;
            }

            var solutions = new ConcurrentBag<string>();

            if (cryptarithm.Contains("#"))
            {
                Parallel.For(1, 1000, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    string currentLeftSide = leftSide.Replace("#", i.ToString());
                    string currentRightSide = rightSide.Replace("#", i.ToString());
                    GenerateSolutionRecursive(currentLeftSide, currentRightSide, letters, new Dictionary<char, int>(), solutions, i);
                });

            }
            else
            {
                GenerateSolutionRecursive(leftSide, rightSide, letters, new Dictionary<char, int>(), solutions, 0);
            }

            if (solutions.Count > 0)
            {
                ShowSolutionsForm(solutions.ToList());
            }
            else
            {
                MessageBox.Show("Решение не найдено.", "Результаты поиска", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GenerateSolutionRecursive(string leftSide, string rightSide, List<char> letters, Dictionary<char, int> currentMapping, ConcurrentBag<string> solutions, int number)
        {
            if (letters.Count == currentMapping.Count)
            {
                if (HasLeadingZero(leftSide, currentMapping) || HasLeadingZero(rightSide, currentMapping))
                {
                    return;
                }

                string replacedLeftSide = ReplaceLettersWithDigits(leftSide, currentMapping);
                string replacedRightSide = ReplaceLettersWithDigits(rightSide, currentMapping);

                try
                {
                    double leftResult = EvaluateExpression(replacedLeftSide);
                    double rightResult = EvaluateExpression(replacedRightSide);

                    if (Math.Abs(leftResult - rightResult) < 0.000001)
                    {
                        if (number > 0)
                        {
                            solutions.Add($"{replacedLeftSide} = {replacedRightSide}  #={number}");
                        }
                        else
                        {
                            solutions.Add($"{replacedLeftSide} = {replacedRightSide}");
                        }
                    }
                }
                catch (FormatException fe)
                {
                    Console.WriteLine($"Ошибка при преобразовании формата: {fe.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при оценке выражения: {ex.Message}");
                }

                return;
            }

            char letter = letters[currentMapping.Count];

            for (int digit = 0; digit <= 9; digit++)
            {
                if (!currentMapping.Values.Contains(digit))
                {
                    currentMapping[letter] = digit;
                    GenerateSolutionRecursive(leftSide, rightSide, letters, currentMapping, solutions, number);
                    currentMapping.Remove(letter); // Возвращаем состояние
                }
            }
        }

        private bool HasLeadingZero(string expression, Dictionary<char, int> mapping)
        {
            foreach (var part in expression.Split(new char[] { ' ', '+', '-', '*', '/', '^', '(',')' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (char.IsLetter(part[0]) && mapping[part[0]] == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private string ReplaceLettersWithDigits(string expression, Dictionary<char, int> mapping)
        {
            StringBuilder replacedExpression = new StringBuilder(expression);

            foreach (var pair in mapping)
            {
                replacedExpression.Replace(pair.Key.ToString(), pair.Value.ToString());
            }

            return replacedExpression.ToString();
        }

        private void ShowSolutionsForm(List<string> solutions)
        {
            SolutionForm solutionForm = new SolutionForm();
            solutionForm.SetSolutions(solutions);
            // Скрываем ProgressBar сразу после открытия формы с решениями
            Invoke((Action)(() => progressBar1.Visible = false));
            solutionForm.ShowDialog();
            
        }

        private double EvaluateExpression(string expression)
        {
            // Handle parentheses by recursion
            while (expression.Contains("("))
            {
                int openIndex = expression.LastIndexOf('(');
                int closeIndex = expression.IndexOf(')', openIndex);
                string subExpression = expression.Substring(openIndex + 1, closeIndex - openIndex - 1);
                double subResult = EvaluateExpression(subExpression);
                expression = expression.Substring(0, openIndex) + subResult.ToString() + expression.Substring(closeIndex + 1);
            }

            // Handle the power operator
            if (expression.Contains('^'))
            {
                string[] powerParts = expression.Split('^');
                double baseNumber = EvaluateExpression(powerParts[0]);
                double exponent;
                if (char.IsLetter(powerParts[1][0]))
                {
                    exponent = EvaluateExpression(powerParts[1]);
                }
                else
                {
                    if (!double.TryParse(powerParts[1], out exponent))
                    {
                        throw new FormatException($"Неверный формат числа: {powerParts[1]}");
                    }
                }
                return Math.Pow(baseNumber, exponent);
            }

            // Handle other operators
            DataTable table = new DataTable();
            try
            {
                var value = table.Compute(expression, null);
                return Convert.ToDouble(value);
            }
            catch (FormatException fe)
            {
                Console.WriteLine($"Ошибка при преобразовании формата: {fe.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при вычислении выражения: {ex.Message}");
                throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

       
    }
}
