namespace FPLedit.AushangfahrplanExport.Forms
{
    partial class EditPatternForm
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
            this.startsWithRadioButton = new System.Windows.Forms.RadioButton();
            this.endsWithRadioButton = new System.Windows.Forms.RadioButton();
            this.containsRadioButton = new System.Windows.Forms.RadioButton();
            this.equalsRadioButton = new System.Windows.Forms.RadioButton();
            this.propertyLabel = new System.Windows.Forms.Label();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // startsWithRadioButton
            //
            this.startsWithRadioButton.AutoSize = true;
            this.startsWithRadioButton.Location = new System.Drawing.Point(15, 34);
            this.startsWithRadioButton.Name = "startsWithRadioButton";
            this.startsWithRadioButton.Size = new System.Drawing.Size(76, 17);
            this.startsWithRadioButton.TabIndex = 0;
            this.startsWithRadioButton.TabStop = true;
            this.startsWithRadioButton.Text = "beginnt mit";
            this.startsWithRadioButton.UseVisualStyleBackColor = true;
            //
            // endsWithRadioButton
            //
            this.endsWithRadioButton.AutoSize = true;
            this.endsWithRadioButton.Location = new System.Drawing.Point(106, 34);
            this.endsWithRadioButton.Name = "endsWithRadioButton";
            this.endsWithRadioButton.Size = new System.Drawing.Size(68, 17);
            this.endsWithRadioButton.TabIndex = 1;
            this.endsWithRadioButton.TabStop = true;
            this.endsWithRadioButton.Text = "endet mit";
            this.endsWithRadioButton.UseVisualStyleBackColor = true;
            //
            // containsRadioButton
            //
            this.containsRadioButton.AutoSize = true;
            this.containsRadioButton.Location = new System.Drawing.Point(15, 57);
            this.containsRadioButton.Name = "containsRadioButton";
            this.containsRadioButton.Size = new System.Drawing.Size(57, 17);
            this.containsRadioButton.TabIndex = 2;
            this.containsRadioButton.TabStop = true;
            this.containsRadioButton.Text = "enthält";
            this.containsRadioButton.UseVisualStyleBackColor = true;
            //
            // equalsRadioButton
            //
            this.equalsRadioButton.AutoSize = true;
            this.equalsRadioButton.Location = new System.Drawing.Point(106, 57);
            this.equalsRadioButton.Name = "equalsRadioButton";
            this.equalsRadioButton.Size = new System.Drawing.Size(35, 17);
            this.equalsRadioButton.TabIndex = 3;
            this.equalsRadioButton.TabStop = true;
            this.equalsRadioButton.Text = "ist";
            this.equalsRadioButton.UseVisualStyleBackColor = true;
            //
            // propertyLabel
            //
            this.propertyLabel.AutoSize = true;
            this.propertyLabel.Location = new System.Drawing.Point(12, 9);
            this.propertyLabel.Name = "propertyLabel";
            this.propertyLabel.Size = new System.Drawing.Size(90, 13);
            this.propertyLabel.TabIndex = 4;
            this.propertyLabel.Text = "{EIGENSCHAFT}";
            //
            // searchTextBox
            //
            this.searchTextBox.Location = new System.Drawing.Point(15, 80);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(159, 20);
            this.searchTextBox.TabIndex = 5;
            //
            // closeButton
            //
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(102, 122);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 13;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(21, 122);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            //
            // EditPatternForm
            //
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(189, 157);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.propertyLabel);
            this.Controls.Add(this.equalsRadioButton);
            this.Controls.Add(this.containsRadioButton);
            this.Controls.Add(this.endsWithRadioButton);
            this.Controls.Add(this.startsWithRadioButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "EditPatternForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Regel bearbeiten";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton startsWithRadioButton;
        private System.Windows.Forms.RadioButton endsWithRadioButton;
        private System.Windows.Forms.RadioButton containsRadioButton;
        private System.Windows.Forms.RadioButton equalsRadioButton;
        private System.Windows.Forms.Label propertyLabel;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button cancelButton;
    }
}