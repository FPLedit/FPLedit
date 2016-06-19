namespace Buchfahrplan
{
    partial class TrainsEditForm
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
            this.topEditTrainButton = new System.Windows.Forms.Button();
            this.bottomEditTrainButton = new System.Windows.Forms.Button();
            this.bottomDeleteTrainButton = new System.Windows.Forms.Button();
            this.bottomTrainListView = new System.Windows.Forms.ListView();
            this.topNewTrainButton = new System.Windows.Forms.Button();
            this.bottomNewTrainButton = new System.Windows.Forms.Button();
            this.topFromToLabel = new System.Windows.Forms.Label();
            this.bottomFromToLabel = new System.Windows.Forms.Label();
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
            this.topTrainListView.FullRowSelect = true;
            this.topTrainListView.HideSelection = false;
            this.topTrainListView.Location = new System.Drawing.Point(12, 27);
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
            this.topDeleteTrainButton.Location = new System.Drawing.Point(652, 85);
            this.topDeleteTrainButton.Name = "topDeleteTrainButton";
            this.topDeleteTrainButton.Size = new System.Drawing.Size(102, 23);
            this.topDeleteTrainButton.TabIndex = 15;
            this.topDeleteTrainButton.Text = "Zug löschen";
            this.topDeleteTrainButton.UseVisualStyleBackColor = true;
            this.topDeleteTrainButton.Click += new System.EventHandler(this.topDeleteTrainButton_Click);
            // 
            // topEditTrainButton
            // 
            this.topEditTrainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.topEditTrainButton.Location = new System.Drawing.Point(652, 56);
            this.topEditTrainButton.Name = "topEditTrainButton";
            this.topEditTrainButton.Size = new System.Drawing.Size(102, 23);
            this.topEditTrainButton.TabIndex = 16;
            this.topEditTrainButton.Text = "Zug bearbeiten";
            this.topEditTrainButton.UseVisualStyleBackColor = true;
            this.topEditTrainButton.Click += new System.EventHandler(this.topEditTrainButton_Click);
            // 
            // bottomEditTrainButton
            // 
            this.bottomEditTrainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomEditTrainButton.Location = new System.Drawing.Point(655, 285);
            this.bottomEditTrainButton.Name = "bottomEditTrainButton";
            this.bottomEditTrainButton.Size = new System.Drawing.Size(102, 23);
            this.bottomEditTrainButton.TabIndex = 21;
            this.bottomEditTrainButton.Text = "Zug bearbeiten";
            this.bottomEditTrainButton.UseVisualStyleBackColor = true;
            this.bottomEditTrainButton.Click += new System.EventHandler(this.bottomEditTrainButton_Click);
            // 
            // bottomDeleteTrainButton
            // 
            this.bottomDeleteTrainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomDeleteTrainButton.Location = new System.Drawing.Point(655, 314);
            this.bottomDeleteTrainButton.Name = "bottomDeleteTrainButton";
            this.bottomDeleteTrainButton.Size = new System.Drawing.Size(102, 23);
            this.bottomDeleteTrainButton.TabIndex = 20;
            this.bottomDeleteTrainButton.Text = "Zug löschen";
            this.bottomDeleteTrainButton.UseVisualStyleBackColor = true;
            this.bottomDeleteTrainButton.Click += new System.EventHandler(this.bottomDeleteTrainButton_Click);
            // 
            // bottomTrainListView
            // 
            this.bottomTrainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomTrainListView.FullRowSelect = true;
            this.bottomTrainListView.HideSelection = false;
            this.bottomTrainListView.Location = new System.Drawing.Point(15, 256);
            this.bottomTrainListView.MultiSelect = false;
            this.bottomTrainListView.Name = "bottomTrainListView";
            this.bottomTrainListView.Size = new System.Drawing.Size(634, 196);
            this.bottomTrainListView.TabIndex = 19;
            this.bottomTrainListView.UseCompatibleStateImageBehavior = false;
            this.bottomTrainListView.View = System.Windows.Forms.View.Details;
            // 
            // topNewTrainButton
            // 
            this.topNewTrainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.topNewTrainButton.Location = new System.Drawing.Point(652, 27);
            this.topNewTrainButton.Name = "topNewTrainButton";
            this.topNewTrainButton.Size = new System.Drawing.Size(102, 23);
            this.topNewTrainButton.TabIndex = 24;
            this.topNewTrainButton.Text = "Neuer Zug...";
            this.topNewTrainButton.UseVisualStyleBackColor = true;
            this.topNewTrainButton.Click += new System.EventHandler(this.topNewTrainButton_Click);
            // 
            // bottomNewTrainButton
            // 
            this.bottomNewTrainButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomNewTrainButton.Location = new System.Drawing.Point(655, 256);
            this.bottomNewTrainButton.Name = "bottomNewTrainButton";
            this.bottomNewTrainButton.Size = new System.Drawing.Size(102, 23);
            this.bottomNewTrainButton.TabIndex = 25;
            this.bottomNewTrainButton.Text = "Neuer Zug...";
            this.bottomNewTrainButton.UseVisualStyleBackColor = true;
            this.bottomNewTrainButton.Click += new System.EventHandler(this.bottomNewTrainButton_Click);
            // 
            // topFromToLabel
            // 
            this.topFromToLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topFromToLabel.Location = new System.Drawing.Point(12, 9);
            this.topFromToLabel.Name = "topFromToLabel";
            this.topFromToLabel.Size = new System.Drawing.Size(634, 15);
            this.topFromToLabel.TabIndex = 26;
            this.topFromToLabel.Text = "Züge von ... nach ...";
            this.topFromToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomFromToLabel
            // 
            this.bottomFromToLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomFromToLabel.Location = new System.Drawing.Point(12, 238);
            this.bottomFromToLabel.Name = "bottomFromToLabel";
            this.bottomFromToLabel.Size = new System.Drawing.Size(634, 15);
            this.bottomFromToLabel.TabIndex = 27;
            this.bottomFromToLabel.Text = "Züge von ... nach ...";
            this.bottomFromToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrainsEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 515);
            this.Controls.Add(this.bottomFromToLabel);
            this.Controls.Add(this.topFromToLabel);
            this.Controls.Add(this.bottomNewTrainButton);
            this.Controls.Add(this.topNewTrainButton);
            this.Controls.Add(this.bottomEditTrainButton);
            this.Controls.Add(this.bottomDeleteTrainButton);
            this.Controls.Add(this.bottomTrainListView);
            this.Controls.Add(this.topEditTrainButton);
            this.Controls.Add(this.topDeleteTrainButton);
            this.Controls.Add(this.topTrainListView);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TrainsEditForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
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
        private System.Windows.Forms.Button topEditTrainButton;
        private System.Windows.Forms.Button bottomEditTrainButton;
        private System.Windows.Forms.Button bottomDeleteTrainButton;
        private System.Windows.Forms.ListView bottomTrainListView;
        private System.Windows.Forms.Button topNewTrainButton;
        private System.Windows.Forms.Button bottomNewTrainButton;
        private System.Windows.Forms.Label topFromToLabel;
        private System.Windows.Forms.Label bottomFromToLabel;

    }
}