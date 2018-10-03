namespace ea2dita
{
    partial class Export2DitaForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.ditamapInput = new System.Windows.Forms.TextBox();
            this.ditamapSelectBtn = new System.Windows.Forms.Button();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.hideEmptyElementsCb = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "DitaMap файл:";
            // 
            // ditamapInput
            // 
            this.ditamapInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ditamapInput.Location = new System.Drawing.Point(121, 12);
            this.ditamapInput.Name = "ditamapInput";
            this.ditamapInput.Size = new System.Drawing.Size(469, 22);
            this.ditamapInput.TabIndex = 1;
            // 
            // ditamapSelectBtn
            // 
            this.ditamapSelectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ditamapSelectBtn.Location = new System.Drawing.Point(596, 12);
            this.ditamapSelectBtn.Name = "ditamapSelectBtn";
            this.ditamapSelectBtn.Size = new System.Drawing.Size(75, 23);
            this.ditamapSelectBtn.TabIndex = 2;
            this.ditamapSelectBtn.Text = "обзор...";
            this.ditamapSelectBtn.UseVisualStyleBackColor = true;
            this.ditamapSelectBtn.Click += new System.EventHandler(this.ditamapSelectBtn_Click);
            // 
            // okBtn
            // 
            this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okBtn.Location = new System.Drawing.Point(512, 142);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(78, 23);
            this.okBtn.TabIndex = 3;
            this.okBtn.Text = "Начать";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(596, 142);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.Text = "Отмена";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // hideEmptyElementsCb
            // 
            this.hideEmptyElementsCb.AutoSize = true;
            this.hideEmptyElementsCb.Location = new System.Drawing.Point(13, 61);
            this.hideEmptyElementsCb.Name = "hideEmptyElementsCb";
            this.hideEmptyElementsCb.Size = new System.Drawing.Size(234, 21);
            this.hideEmptyElementsCb.TabIndex = 5;
            this.hideEmptyElementsCb.Text = "Не выводить пустые элементы";
            this.hideEmptyElementsCb.UseVisualStyleBackColor = true;
            // 
            // Export2DitaForm
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(683, 177);
            this.Controls.Add(this.hideEmptyElementsCb);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.ditamapSelectBtn);
            this.Controls.Add(this.ditamapInput);
            this.Controls.Add(this.label1);
            this.Name = "Export2DitaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Экспорт в DITA";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ditamapInput;
        private System.Windows.Forms.Button ditamapSelectBtn;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.CheckBox hideEmptyElementsCb;
    }
}