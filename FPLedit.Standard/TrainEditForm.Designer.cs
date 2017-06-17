namespace FPLedit.Standard
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
            this.nameValidator = new FPLedit.Shared.Validators.NotEmptyValidator();
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lastTextBox = new System.Windows.Forms.TextBox();
            this.lastLabel = new System.Windows.Forms.Label();
            this.mbrTextBox = new System.Windows.Forms.TextBox();
            this.mbrLabel = new System.Windows.Forms.Label();
            this.locomotiveComboBox = new System.Windows.Forms.ComboBox();
            this.locomotiveLabel = new System.Windows.Forms.Label();
            this.daysGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(12, 9);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(78, 6);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(135, 20);
            this.nameTextBox.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(57, 238);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(138, 238);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 8;
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
            this.daysGroupBox.Location = new System.Drawing.Point(15, 161);
            this.daysGroupBox.Name = "daysGroupBox";
            this.daysGroupBox.Size = new System.Drawing.Size(198, 69);
            this.daysGroupBox.TabIndex = 6;
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
            this.sundayCheckBox.TabIndex = 6;
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
            this.saturdayCheckBox.TabIndex = 5;
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
            this.fridayCheckBox.TabIndex = 4;
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
            this.thursdayCheckBox.TabIndex = 3;
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
            this.wednesdayCheckBox.TabIndex = 2;
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
            this.tuesdayCheckBox.TabIndex = 1;
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
            this.mondayCheckBox.TabIndex = 0;
            this.mondayCheckBox.Text = "Mo";
            this.mondayCheckBox.UseVisualStyleBackColor = true;
            // 
            // nameValidator
            // 
            this.nameValidator.Control = this.nameTextBox;
            this.nameValidator.ErrorMessage = "Bitte einen Namen eingeben!";
            // 
            // commentTextBox
            // 
            this.commentTextBox.Location = new System.Drawing.Point(78, 32);
            this.commentTextBox.Name = "commentTextBox";
            this.commentTextBox.Size = new System.Drawing.Size(135, 20);
            this.commentTextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Kommentar";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lastTextBox);
            this.groupBox1.Controls.Add(this.lastLabel);
            this.groupBox1.Controls.Add(this.mbrTextBox);
            this.groupBox1.Controls.Add(this.mbrLabel);
            this.groupBox1.Controls.Add(this.locomotiveComboBox);
            this.groupBox1.Controls.Add(this.locomotiveLabel);
            this.groupBox1.Location = new System.Drawing.Point(15, 58);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(198, 97);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Erweiterte Eigenschaften (für Bfpl)";
            // 
            // lastTextBox
            // 
            this.lastTextBox.Location = new System.Drawing.Point(63, 72);
            this.lastTextBox.Name = "lastTextBox";
            this.lastTextBox.Size = new System.Drawing.Size(129, 20);
            this.lastTextBox.TabIndex = 20;
            // 
            // lastLabel
            // 
            this.lastLabel.AutoSize = true;
            this.lastLabel.Location = new System.Drawing.Point(6, 75);
            this.lastLabel.Name = "lastLabel";
            this.lastLabel.Size = new System.Drawing.Size(27, 13);
            this.lastLabel.TabIndex = 19;
            this.lastLabel.Text = "Last";
            // 
            // mbrTextBox
            // 
            this.mbrTextBox.Location = new System.Drawing.Point(63, 46);
            this.mbrTextBox.Name = "mbrTextBox";
            this.mbrTextBox.Size = new System.Drawing.Size(129, 20);
            this.mbrTextBox.TabIndex = 18;
            // 
            // mbrLabel
            // 
            this.mbrLabel.AutoSize = true;
            this.mbrLabel.Location = new System.Drawing.Point(6, 49);
            this.mbrLabel.Name = "mbrLabel";
            this.mbrLabel.Size = new System.Drawing.Size(25, 13);
            this.mbrLabel.TabIndex = 17;
            this.mbrLabel.Text = "Mbr";
            // 
            // locomotiveComboBox
            // 
            this.locomotiveComboBox.FormattingEnabled = true;
            this.locomotiveComboBox.Location = new System.Drawing.Point(63, 19);
            this.locomotiveComboBox.Name = "locomotiveComboBox";
            this.locomotiveComboBox.Size = new System.Drawing.Size(129, 21);
            this.locomotiveComboBox.TabIndex = 16;
            // 
            // locomotiveLabel
            // 
            this.locomotiveLabel.AutoSize = true;
            this.locomotiveLabel.Location = new System.Drawing.Point(6, 22);
            this.locomotiveLabel.Name = "locomotiveLabel";
            this.locomotiveLabel.Size = new System.Drawing.Size(22, 13);
            this.locomotiveLabel.TabIndex = 15;
            this.locomotiveLabel.Text = "Tfz";
            // 
            // TrainEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(229, 273);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.commentTextBox);
            this.Controls.Add(this.daysGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "TrainEditForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Neuen Zug erstellen";
            this.daysGroupBox.ResumeLayout(false);
            this.daysGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
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
        private Shared.Validators.NotEmptyValidator nameValidator;
        private System.Windows.Forms.TextBox commentTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox lastTextBox;
        private System.Windows.Forms.Label lastLabel;
        private System.Windows.Forms.TextBox mbrTextBox;
        private System.Windows.Forms.Label mbrLabel;
        private System.Windows.Forms.ComboBox locomotiveComboBox;
        private System.Windows.Forms.Label locomotiveLabel;
    }
}