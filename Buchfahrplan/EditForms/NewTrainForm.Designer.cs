namespace Buchfahrplan
{
    partial class NewTrainForm
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
            this.locomotiveTextBox = new System.Windows.Forms.TextBox();
            this.locomotiveLabel = new System.Windows.Forms.Label();
            this.lineLabel = new System.Windows.Forms.Label();
            this.lineTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.negativeCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // locomotiveTextBox
            // 
            this.locomotiveTextBox.Location = new System.Drawing.Point(65, 59);
            this.locomotiveTextBox.Name = "locomotiveTextBox";
            this.locomotiveTextBox.Size = new System.Drawing.Size(131, 20);
            this.locomotiveTextBox.TabIndex = 12;
            // 
            // locomotiveLabel
            // 
            this.locomotiveLabel.AutoSize = true;
            this.locomotiveLabel.Location = new System.Drawing.Point(12, 62);
            this.locomotiveLabel.Name = "locomotiveLabel";
            this.locomotiveLabel.Size = new System.Drawing.Size(22, 13);
            this.locomotiveLabel.TabIndex = 11;
            this.locomotiveLabel.Text = "Tfz";
            // 
            // lineLabel
            // 
            this.lineLabel.AutoSize = true;
            this.lineLabel.Location = new System.Drawing.Point(12, 35);
            this.lineLabel.Name = "lineLabel";
            this.lineLabel.Size = new System.Drawing.Size(44, 13);
            this.lineLabel.TabIndex = 9;
            this.lineLabel.Text = "Strecke";
            // 
            // lineTextBox
            // 
            this.lineTextBox.Location = new System.Drawing.Point(65, 32);
            this.lineTextBox.Name = "lineTextBox";
            this.lineTextBox.Size = new System.Drawing.Size(131, 20);
            this.lineTextBox.TabIndex = 10;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(12, 9);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 7;
            this.nameLabel.Text = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(65, 6);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(131, 20);
            this.nameTextBox.TabIndex = 8;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(40, 108);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(121, 108);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 14;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // negativeCheckBox
            // 
            this.negativeCheckBox.AutoSize = true;
            this.negativeCheckBox.Location = new System.Drawing.Point(15, 85);
            this.negativeCheckBox.Name = "negativeCheckBox";
            this.negativeCheckBox.Size = new System.Drawing.Size(78, 17);
            this.negativeCheckBox.TabIndex = 15;
            this.negativeCheckBox.Text = "Umgekehrt";
            this.negativeCheckBox.UseVisualStyleBackColor = true;
            // 
            // NewTrainForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(212, 137);
            this.Controls.Add(this.negativeCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.locomotiveTextBox);
            this.Controls.Add(this.locomotiveLabel);
            this.Controls.Add(this.lineLabel);
            this.Controls.Add(this.lineTextBox);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "NewTrainForm";
            this.Text = "Neuen Zug erstellen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox locomotiveTextBox;
        private System.Windows.Forms.Label locomotiveLabel;
        private System.Windows.Forms.Label lineLabel;
        private System.Windows.Forms.TextBox lineTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckBox negativeCheckBox;
    }
}