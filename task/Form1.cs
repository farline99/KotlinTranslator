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
			listBoxLexicalErrors.Items.Clear();
			listBoxSyntacticErrors.Items.Clear();
			listBoxSemanticErrors.Items.Clear();
			textBox3.Clear();
			listView1.Items.Clear();

			string text = textBox1.Text;
			if (text == "") return;

			List<(ErrorType Type, string Message)> lexicalErrors;
			List<Token> tokens = LexAn.InterpString(text, out lexicalErrors);

			foreach (var error in lexicalErrors)
			{
				listBoxLexicalErrors.Items.Add(error.Message);
			}

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

			foreach (var error in analysisErrors)
			{
				if (error.Type == ErrorType.Syntactic)
				{
					listBoxSyntacticErrors.Items.Add(error.Message);
				}
				else if (error.Type == ErrorType.Semantic)
				{
					listBoxSemanticErrors.Items.Add(error.Message);
				}
			}

			List<RussBlock> russianCode = parser.GetRussianCodeMatrix();
			foreach (RussBlock block in russianCode)
			{
				textBox3.AppendText(block.ToRussianString() + Environment.NewLine);
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
