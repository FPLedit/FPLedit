namespace Buchfahrplan.Standard
{
    partial class TrainEditForm
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
            this.daysGroupBox = new System.Windows.Forms.GroupBox();
            this.sundayCheckBox = new System.Windows.Forms.CheckBox();
            this.saturdayCheckBox = new System.Windows.Forms.CheckBox();
            this.fridayCheckBox = new System.Windows.Forms.CheckBox();
            this.thursdayCheckBox = new System.Windows.Forms.CheckBox();
            this.wednesdayCheckBox = new System.Windows.Forms.CheckBox();
            this.tuesdayCheckBox = new System.Windows.Forms.CheckBox();
            this.mondayCheckBox = new System.Windows.Forms.CheckBox();
            this.daysGroupBox.SuspendLayout();
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
            this.cancelButton.Location = new System.Drawing.Point(44, 160);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(125, 160);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 14;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // daysGroupBox
            // 
            this.daysGroupBox.Controls.Add(this.sundayCheckBox);
            this.daysGroupBox.Controls.Add(this.saturdayCheckBox);
            this.daysGroupBox.Controls.Add(this.fridayCheckBox);
            this.daysGroupBox.Controls.Add(this.thursdayCheckBox);
            this.daysGroupBox.Controls.Add(this.wednesdayCheckBox);
            this.daysGroupBox.Controls.Add(this.tuesdayCheckBox);
            this.daysGroupBox.Controls.Add(this.mondayCheckBox);
            this.daysGroupBox.Location = new System.Drawing.Point(12, 85);
            this.daysGroupBox.Name = "daysGroupBox";
            this.daysGroupBox.Size = new System.Drawing.Size(188, 69);
            this.daysGroupBox.TabIndex = 22;
            this.daysGroupBox.TabStop = false;
            this.daysGroupBox.Text = "Verkehrstage";
            // 
            // sundayCheckBox
            // 
            this.sundayCheckBox.AutoSize = true;
            this.sundayCheckBox.Checked = true;
            this.sundayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.sundayCheckBox.Location = new System.Drawing.Point(99, 44);
            this.sundayCheckBox.Name = "sundayCheckBox";
            this.sundayCheckBox.Size = new System.Drawing.Size(39, 17);
            this.sundayCheckBox.TabIndex = 28;
            this.sundayCheckBox.Text = "So";
            this.sundayCheckBox.UseVisualStyleBackColor = true;
            // 
            // saturdayCheckBox
            // 
            this.saturdayCheckBox.AutoSize = true;
            this.saturdayCheckBox.Checked = true;
            this.saturdayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.saturdayCheckBox.Location = new System.Drawing.Point(52, 44);
            this.saturdayCheckBox.Name = "saturdayCheckBox";
            this.saturdayCheckBox.Size = new System.Drawing.Size(39, 17);
            this.saturdayCheckBox.TabIndex = 27;
            this.saturdayCheckBox.Text = "Sa";
            this.saturdayCheckBox.UseVisualStyleBackColor = true;
            // 
            // fridayCheckBox
            // 
            this.fridayCheckBox.AutoSize = true;
            this.fridayCheckBox.Checked = true;
            this.fridayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fridayCheckBox.Location = new System.Drawing.Point(5, 44);
            this.fridayCheckBox.Name = "fridayCheckBox";
            this.fridayCheckBox.Size = new System.Drawing.Size(35, 17);
            this.fridayCheckBox.TabIndex = 26;
            this.fridayCheckBox.Text = "Fr";
            this.fridayCheckBox.UseVisualStyleBackColor = true;
            // 
            // thursdayCheckBox
            // 
            this.thursdayCheckBox.AutoSize = true;
            this.thursdayCheckBox.Checked = true;
            this.thursdayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.thursdayCheckBox.Location = new System.Drawing.Point(146, 21);
            this.thursdayCheckBox.Name = "thursdayCheckBox";
            this.thursdayCheckBox.Size = new System.Drawing.Size(40, 17);
            this.thursdayCheckBox.TabIndex = 25;
            this.thursdayCheckBox.Text = "Do";
            this.thursdayCheckBox.UseVisualStyleBackColor = true;
            // 
            // wednesdayCheckBox
            // 
            this.wednesdayCheckBox.AutoSize = true;
            this.wednesdayCheckBox.Checked = true;
            this.wednesdayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wednesdayCheckBox.Location = new System.Drawing.Point(99, 21);
            this.wednesdayCheckBox.Name = "wednesdayCheckBox";
            this.wednesdayCheckBox.Size = new System.Drawing.Size(37, 17);
            this.wednesdayCheckBox.TabIndex = 24;
            this.wednesdayCheckBox.Text = "Mi";
            this.wednesdayCheckBox.UseVisualStyleBackColor = true;
            // 
            // tuesdayCheckBox
            // 
            this.tuesdayCheckBox.AutoSize = true;
            this.tuesdayCheckBox.Checked = true;
            this.tuesdayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tuesdayCheckBox.Location = new System.Drawing.Point(52, 21);
            this.tuesdayCheckBox.Name = "tuesdayCheckBox";
            this.tuesdayCheckBox.Size = new System.Drawing.Size(36, 17);
            this.tuesdayCheckBox.TabIndex = 23;
            this.tuesdayCheckBox.Text = "Di";
            this.tuesdayCheckBox.UseVisualStyleBackColor = true;
            // 
            // mondayCheckBox
            // 
            this.mondayCheckBox.AutoSize = true;
            this.mondayCheckBox.Checked = true;
            this.mondayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mondayCheckBox.Location = new System.Drawing.Point(5, 21);
            this.mondayCheckBox.Name = "mondayCheckBox";
            this.mondayCheckBox.Size = new System.Drawing.Size(41, 17);
            this.mondayCheckBox.TabIndex = 22;
            this.mondayCheckBox.Text = "Mo";
            this.mondayCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrainEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(212, 197);
            this.Controls.Add(this.daysGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.locomotiveTextBox);
            this.Controls.Add(this.locomotiveLabel);
            this.Controls.Add(this.lineLabel);
            this.Controls.Add(this.lineTextBox);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "TrainEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Neuen Zug erstellen";
            this.daysGroupBox.ResumeLayout(false);
            this.daysGroupBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox daysGroupBox;
        private System.Windows.Forms.CheckBox sundayCheckBox;
        private System.Windows.Forms.CheckBox saturdayCheckBox;
        private System.Windows.Forms.CheckBox fridayCheckBox;
        private System.Windows.Forms.CheckBox thursdayCheckBox;
        private System.Windows.Forms.CheckBox wednesdayCheckBox;
        private System.Windows.Forms.CheckBox tuesdayCheckBox;
        private System.Windows.Forms.CheckBox mondayCheckBox;
    }
}