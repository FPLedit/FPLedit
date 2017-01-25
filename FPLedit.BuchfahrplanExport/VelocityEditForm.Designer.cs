namespace FPLedit.BuchfahrplanExport
{
    partial class VelocityEditForm
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
            this.velocityLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.velocityTextBox = new System.Windows.Forms.TextBox();
            this.velocityValidator = new FPLedit.Shared.Validators.NumberValidator();
            this.positionTextBox = new System.Windows.Forms.TextBox();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.positionLabel = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // velocityLabel
            // 
            this.velocityLabel.AutoSize = true;
            this.velocityLabel.Location = new System.Drawing.Point(12, 67);
            this.velocityLabel.Name = "velocityLabel";
            this.velocityLabel.Size = new System.Drawing.Size(117, 13);
            this.velocityLabel.TabIndex = 0;
            this.velocityLabel.Text = "Höchstgeschwindigkeit";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(109, 89);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(190, 89);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 2;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // velocityTextBox
            // 
            this.velocityTextBox.Location = new System.Drawing.Point(135, 64);
            this.velocityTextBox.Name = "velocityTextBox";
            this.velocityTextBox.Size = new System.Drawing.Size(130, 20);
            this.velocityTextBox.TabIndex = 1;
            // 
            // velocityValidator
            // 
            this.velocityValidator.Control = this.velocityTextBox;
            this.velocityValidator.ErrorMessage = "Bitte eine Zahl als Geschwindigkeit angeben!";
            // 
            // positionTextBox
            // 
            this.positionTextBox.Location = new System.Drawing.Point(135, 12);
            this.positionTextBox.Name = "positionTextBox";
            this.positionTextBox.Size = new System.Drawing.Size(130, 20);
            this.positionTextBox.TabIndex = 4;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(135, 38);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(130, 20);
            this.nameTextBox.TabIndex = 5;
            // 
            // positionLabel
            // 
            this.positionLabel.AutoSize = true;
            this.positionLabel.Location = new System.Drawing.Point(12, 15);
            this.positionLabel.Name = "positionLabel";
            this.positionLabel.Size = new System.Drawing.Size(44, 13);
            this.positionLabel.TabIndex = 6;
            this.positionLabel.Text = "Position";
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(12, 41);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 7;
            this.nameLabel.Text = "Name";
            // 
            // VelocityEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(277, 124);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.positionLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.positionTextBox);
            this.Controls.Add(this.velocityTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.velocityLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "VelocityEditForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Höchstgeschwindigkeit ändern";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label velocityLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.TextBox velocityTextBox;
        private Shared.Validators.NumberValidator velocityValidator;
        private System.Windows.Forms.TextBox positionTextBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label positionLabel;
        private System.Windows.Forms.Label nameLabel;
    }
}