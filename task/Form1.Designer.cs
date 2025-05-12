namespace task
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button2 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.listBoxLexicalErrors = new System.Windows.Forms.ListBox();
            this.listBoxSyntacticErrors = new System.Windows.Forms.ListBox();
            this.listBoxSemanticErrors = new System.Windows.Forms.ListBox();
            this.labelSourceCode = new System.Windows.Forms.Label();
            this.labelLexAn = new System.Windows.Forms.Label();
            this.labelCodeGeneration = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 25);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(276, 330);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(160, 560);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(143, 58);
            this.button1.TabIndex = 1;
            this.button1.Text = "Считать из файла";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(294, 25);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(271, 330);
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Лексема";
            this.columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Результат";
            this.columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Индекс";
            this.columnHeader3.Width = 80;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(11, 560);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(143, 58);
            this.button2.TabIndex = 7;
            this.button2.Text = "Анализировать";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(570, 25);
            this.textBox3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox3.Size = new System.Drawing.Size(527, 331);
            this.textBox3.TabIndex = 8;
            // 
            // listBoxLexicalErrors
            // 
            this.listBoxLexicalErrors.FormattingEnabled = true;
            this.listBoxLexicalErrors.Location = new System.Drawing.Point(11, 382);
            this.listBoxLexicalErrors.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBoxLexicalErrors.Name = "listBoxLexicalErrors";
            this.listBoxLexicalErrors.Size = new System.Drawing.Size(1085, 43);
            this.listBoxLexicalErrors.TabIndex = 9;
            // 
            // listBoxSyntacticErrors
            // 
            this.listBoxSyntacticErrors.FormattingEnabled = true;
            this.listBoxSyntacticErrors.Location = new System.Drawing.Point(11, 442);
            this.listBoxSyntacticErrors.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBoxSyntacticErrors.Name = "listBoxSyntacticErrors";
            this.listBoxSyntacticErrors.Size = new System.Drawing.Size(1085, 43);
            this.listBoxSyntacticErrors.TabIndex = 10;
            // 
            // listBoxSemanticErrors
            // 
            this.listBoxSemanticErrors.FormattingEnabled = true;
            this.listBoxSemanticErrors.Location = new System.Drawing.Point(11, 502);
            this.listBoxSemanticErrors.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBoxSemanticErrors.Name = "listBoxSemanticErrors";
            this.listBoxSemanticErrors.Size = new System.Drawing.Size(1085, 43);
            this.listBoxSemanticErrors.TabIndex = 11;
            // 
            // labelSourceCode
            // 
            this.labelSourceCode.AutoSize = true;
            this.labelSourceCode.Location = new System.Drawing.Point(12, 9);
            this.labelSourceCode.Name = "labelSourceCode";
            this.labelSourceCode.Size = new System.Drawing.Size(79, 13);
            this.labelSourceCode.TabIndex = 12;
            this.labelSourceCode.Text = "Исходный код";
            // 
            // labelLexAn
            // 
            this.labelLexAn.AutoSize = true;
            this.labelLexAn.Location = new System.Drawing.Point(296, 9);
            this.labelLexAn.Name = "labelLexAn";
            this.labelLexAn.Size = new System.Drawing.Size(113, 13);
            this.labelLexAn.TabIndex = 13;
            this.labelLexAn.Text = "Лексический анализ";
            // 
            // labelCodeGeneration
            // 
            this.labelCodeGeneration.AutoSize = true;
            this.labelCodeGeneration.Location = new System.Drawing.Point(567, 9);
            this.labelCodeGeneration.Name = "labelCodeGeneration";
            this.labelCodeGeneration.Size = new System.Drawing.Size(88, 13);
            this.labelCodeGeneration.TabIndex = 14;
            this.labelCodeGeneration.Text = "Генерация кода";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 367);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Лексические ошибки";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 427);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Синтаксические ошибки";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 487);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Семантические ошибки";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1112, 630);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelCodeGeneration);
            this.Controls.Add(this.labelLexAn);
            this.Controls.Add(this.labelSourceCode);
            this.Controls.Add(this.listBoxSemanticErrors);
            this.Controls.Add(this.listBoxSyntacticErrors);
            this.Controls.Add(this.listBoxLexicalErrors);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Транслятор подмножества языка Kotlin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.ListBox listBoxLexicalErrors;
        private System.Windows.Forms.ListBox listBoxSyntacticErrors;
        private System.Windows.Forms.ListBox listBoxSemanticErrors;
        private System.Windows.Forms.Label labelSourceCode;
        private System.Windows.Forms.Label labelLexAn;
        private System.Windows.Forms.Label labelCodeGeneration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}

