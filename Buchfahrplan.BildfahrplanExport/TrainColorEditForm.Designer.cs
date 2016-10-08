namespace FPLedit.BildfahrplanExport
{
    partial class TrainColorEditForm
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
            this.colorWidthLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.colorComboBox = new System.Windows.Forms.ComboBox();
            this.widthComboBox = new System.Windows.Forms.ComboBox();
            this.drawCheckBox = new System.Windows.Forms.CheckBox();
            this.layoutTextBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // colorWidthLabel
            // 
            this.colorWidthLabel.AutoSize = true;
            this.colorWidthLabel.Location = new System.Drawing.Point(12, 9);
            this.colorWidthLabel.Name = "colorWidthLabel";
            this.colorWidthLabel.Size = new System.Drawing.Size(94, 13);
            this.colorWidthLabel.TabIndex = 7;
            this.colorWidthLabel.Text = "Linienfarbe, stärke";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(166, 62);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(247, 62);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 14;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // colorComboBox
            // 
            this.colorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorComboBox.FormattingEnabled = true;
            this.colorComboBox.Location = new System.Drawing.Point(109, 6);
            this.colorComboBox.Name = "colorComboBox";
            this.colorComboBox.Size = new System.Drawing.Size(150, 21);
            this.colorComboBox.TabIndex = 20;
            // 
            // widthComboBox
            // 
            this.widthComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.widthComboBox.FormattingEnabled = true;
            this.widthComboBox.Location = new System.Drawing.Point(265, 6);
            this.widthComboBox.Name = "widthComboBox";
            this.widthComboBox.Size = new System.Drawing.Size(57, 21);
            this.widthComboBox.TabIndex = 36;
            // 
            // drawCheckBox
            // 
            this.drawCheckBox.AutoSize = true;
            this.drawCheckBox.Location = new System.Drawing.Point(15, 33);
            this.drawCheckBox.Name = "drawCheckBox";
            this.drawCheckBox.Size = new System.Drawing.Size(91, 17);
            this.drawCheckBox.TabIndex = 37;
            this.drawCheckBox.Text = "Zug zeichnen";
            this.drawCheckBox.UseVisualStyleBackColor = true;
            // 
            // layoutTextBox1
            // 
            this.layoutTextBox1.Location = new System.Drawing.Point(109, 31);
            this.layoutTextBox1.Name = "layoutTextBox1";
            this.layoutTextBox1.Size = new System.Drawing.Size(10, 20);
            this.layoutTextBox1.TabIndex = 50;
            this.layoutTextBox1.Visible = false;
            // 
            // TrainColorEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(334, 95);
            this.Controls.Add(this.layoutTextBox1);
            this.Controls.Add(this.drawCheckBox);
            this.Controls.Add(this.widthComboBox);
            this.Controls.Add(this.colorComboBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.colorWidthLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "TrainColorEditForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Zugdarstellung ändern";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label colorWidthLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ComboBox colorComboBox;
        private System.Windows.Forms.ComboBox widthComboBox;
        private System.Windows.Forms.CheckBox drawCheckBox;
        private System.Windows.Forms.TextBox layoutTextBox1;
    }
}