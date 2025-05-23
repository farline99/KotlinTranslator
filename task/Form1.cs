using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace task
{
	public enum ErrorType
	{
		Lexical,
		Syntactic,
		Semantic
	}

	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void AnalyzeText()
		{
			listBox1.Items.Clear();
			textBox3.Clear();
			listView1.Items.Clear();

			string text = textBox1.Text;
			if (text == "") return;

			List<(ErrorType Type, string Message)> lexicalErrors;
			List<Token> tokens = LexAn.InterpString(text, out lexicalErrors);

			int j = 0;
			foreach (var token in tokens)
			{
				var item = new ListViewItem(token.Value);
				item.SubItems.Add(token.ToString());
				item.SubItems.Add(j.ToString());
				listView1.Items.Add(item);
				j++;
			}

			List<(ErrorType Type, string Message)> analysisErrors;
			SyntLLParser parser = new SyntLLParser(tokens, out analysisErrors);
			ErrorCode result = parser.Parse();

			List<RussBlock> russianCode = parser.GetRussianCodeMatrix();
			foreach (RussBlock block in russianCode)
			{
				textBox3.AppendText(block.ToRussianString() + Environment.NewLine);
			}

			if (lexicalErrors.Count == 0 && analysisErrors.Count == 0)
			{
				listBox1.Items.Add("Ошибки отсутствуют. Трансляция завершилась успешно.");
			}
			else
			{
                foreach (var error in lexicalErrors)
                {
                    listBox1.Items.Add("Лексическая " + error.Message);
                }

                foreach (var error in analysisErrors)
                {
                    if (error.Type == ErrorType.Syntactic)
                    {
                        listBox1.Items.Add("Синтаксическая " + error.Message);
                    }
                    else if (error.Type == ErrorType.Semantic)
                    {
                        listBox1.Items.Add("Семантическая " + error.Message);
                    }
                }
            }
		}

		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Выберите файл для анализа";
			openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					string text = File.ReadAllText(openFileDialog.FileName);

					textBox1.Text = text;

					AnalyzeText();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Ошибка при загрузке файла: " + ex.Message,
								  "Ошибка", MessageBoxButtons.OK,
								  MessageBoxIcon.Error);
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			AnalyzeText();
		}
	}
}
