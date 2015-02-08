namespace Buchfahrplan
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
            this.closeButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.topTrainListView = new System.Windows.Forms.ListView();
            this.topDeleteTrainButton = new System.Windows.Forms.Button();
            this.topChangeNameButton = new System.Windows.Forms.Button();
            this.topChangeLineButton = new System.Windows.Forms.Button();
            this.topChangeLocomotiveButton = new System.Windows.Forms.Button();
            this.bottomChangeLocomotiveButton = new System.Windows.Forms.Button();
            this.bottomChangeLineButton = new System.Windows.Forms.Button();
            this.bottomChangeNameButton = new System.Windows.Forms.Button();
            this.bottomDeleteTrainButton = new System.Windows.Forms.Button();
            this.bottomTrainListView = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(679, 480);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 5;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(598, 480);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // topTrainListView
            // 
            this.topTrainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topTrainListView.HideSelection = false;
            this.topTrainListView.Location = new System.Drawing.Point(12, 12);
            this.topTrainListView.MultiSelect = false;
            this.topTrainListView.Name = "topTrainListView";
            this.topTrainListView.Size = new System.Drawing.Size(634, 196);
            this.topTrainListView.TabIndex = 7;
            this.topTrainListView.UseCompatibleStateImageBehavior = false;
            this.topTrainListView.View = System.Windows.Forms.View.Details;
            // 
            // topDeleteTrainButton
            // 
            this.topDeleteTrainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.topDeleteTrainButton.Location = new System.Drawing.Point(652, 99);
            this.topDeleteTrainButton.Name = "topDeleteTrainButton";
            this.topDeleteTrainButton.Size = new System.Drawing.Size(102, 23);
            this.topDeleteTrainButton.TabIndex = 15;
            this.topDeleteTrainButton.Text = "Zug löschen";
            this.topDeleteTrainButton.UseVisualStyleBackColor = true;
            this.topDeleteTrainButton.Click += new System.EventHandler(this.topDeleteTrainButton_Click);
            // 
            // topChangeNameButton
            // 
            this.topChangeNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.topChangeNameButton.Location = new System.Drawing.Point(652, 12);
            this.topChangeNameButton.Name = "topChangeNameButton";
            this.topChangeNameButton.Size = new System.Drawing.Size(102, 23);
            this.topChangeNameButton.TabIndex = 16;
            this.topChangeNameButton.Text = "Name ändern";
            this.topChangeNameButton.UseVisualStyleBackColor = true;
            this.topChangeNameButton.Click += new System.EventHandler(this.topChangeNameButton_Click);
            // 
            // topChangeLineButton
            // 
            this.topChangeLineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.topChangeLineButton.Location = new System.Drawing.Point(652, 41);
            this.topChangeLineButton.Name = "topChangeLineButton";
            this.topChangeLineButton.Size = new System.Drawing.Size(102, 23);
            this.topChangeLineButton.TabIndex = 17;
            this.topChangeLineButton.Text = "Strecke ändern";
            this.topChangeLineButton.UseVisualStyleBackColor = true;
            this.topChangeLineButton.Click += new System.EventHandler(this.topChangeLineButton_Click);
            // 
            // topChangeLocomotiveButton
            // 
            this.topChangeLocomotiveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.topChangeLocomotiveButton.Location = new System.Drawing.Point(652, 70);
            this.topChangeLocomotiveButton.Name = "topChangeLocomotiveButton";
            this.topChangeLocomotiveButton.Size = new System.Drawing.Size(102, 23);
            this.topChangeLocomotiveButton.TabIndex = 18;
            this.topChangeLocomotiveButton.Text = "Tfz ändern";
            this.topChangeLocomotiveButton.UseVisualStyleBackColor = true;
            this.topChangeLocomotiveButton.Click += new System.EventHandler(this.topChangeLocomotiveButton_Click);
            // 
            // bottomChangeLocomotiveButton
            // 
            this.bottomChangeLocomotiveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomChangeLocomotiveButton.Location = new System.Drawing.Point(652, 294);
            this.bottomChangeLocomotiveButton.Name = "bottomChangeLocomotiveButton";
            this.bottomChangeLocomotiveButton.Size = new System.Drawing.Size(102, 23);
            this.bottomChangeLocomotiveButton.TabIndex = 23;
            this.bottomChangeLocomotiveButton.Text = "Tfz ändern";
            this.bottomChangeLocomotiveButton.UseVisualStyleBackColor = true;
            // 
            // bottomChangeLineButton
            // 
            this.bottomChangeLineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomChangeLineButton.Location = new System.Drawing.Point(652, 265);
            this.bottomChangeLineButton.Name = "bottomChangeLineButton";
            this.bottomChangeLineButton.Size = new System.Drawing.Size(102, 23);
            this.bottomChangeLineButton.TabIndex = 22;
            this.bottomChangeLineButton.Text = "Strecke ändern";
            this.bottomChangeLineButton.UseVisualStyleBackColor = true;
            // 
            // bottomChangeNameButton
            // 
            this.bottomChangeNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomChangeNameButton.Location = new System.Drawing.Point(652, 236);
            this.bottomChangeNameButton.Name = "bottomChangeNameButton";
            this.bottomChangeNameButton.Size = new System.Drawing.Size(102, 23);
            this.bottomChangeNameButton.TabIndex = 21;
            this.bottomChangeNameButton.Text = "Name ändern";
            this.bottomChangeNameButton.UseVisualStyleBackColor = true;
            // 
            // bottomDeleteTrainButton
            // 
            this.bottomDeleteTrainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomDeleteTrainButton.Location = new System.Drawing.Point(652, 323);
            this.bottomDeleteTrainButton.Name = "bottomDeleteTrainButton";
            this.bottomDeleteTrainButton.Size = new System.Drawing.Size(102, 23);
            this.bottomDeleteTrainButton.TabIndex = 20;
            this.bottomDeleteTrainButton.Text = "Zug löschen";
            this.bottomDeleteTrainButton.UseVisualStyleBackColor = true;
            // 
            // bottomTrainListView
            // 
            this.bottomTrainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomTrainListView.HideSelection = false;
            this.bottomTrainListView.Location = new System.Drawing.Point(12, 236);
            this.bottomTrainListView.MultiSelect = false;
            this.bottomTrainListView.Name = "bottomTrainListView";
            this.bottomTrainListView.Size = new System.Drawing.Size(634, 196);
            this.bottomTrainListView.TabIndex = 19;
            this.bottomTrainListView.UseCompatibleStateImageBehavior = false;
            this.bottomTrainListView.View = System.Windows.Forms.View.Details;
            // 
            // EditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 515);
            this.Controls.Add(this.bottomChangeLocomotiveButton);
            this.Controls.Add(this.bottomChangeLineButton);
            this.Controls.Add(this.bottomChangeNameButton);
            this.Controls.Add(this.bottomDeleteTrainButton);
            this.Controls.Add(this.bottomTrainListView);
            this.Controls.Add(this.topChangeLocomotiveButton);
            this.Controls.Add(this.topChangeLineButton);
            this.Controls.Add(this.topChangeNameButton);
            this.Controls.Add(this.topDeleteTrainButton);
            this.Controls.Add(this.topTrainListView);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "EditForm";
            this.Text = "Züge bearbeiten...";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditForm_FormClosing);
            this.Load += new System.EventHandler(this.NewEditForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ListView topTrainListView;
        private System.Windows.Forms.Button topDeleteTrainButton;
        private System.Windows.Forms.Button topChangeNameButton;
        private System.Windows.Forms.Button topChangeLineButton;
        private System.Windows.Forms.Button topChangeLocomotiveButton;
        private System.Windows.Forms.Button bottomChangeLocomotiveButton;
        private System.Windows.Forms.Button bottomChangeLineButton;
        private System.Windows.Forms.Button bottomChangeNameButton;
        private System.Windows.Forms.Button bottomDeleteTrainButton;
        private System.Windows.Forms.ListView bottomTrainListView;

    }
}