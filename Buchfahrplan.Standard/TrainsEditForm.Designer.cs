namespace Buchfahrplan.Standard
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.topFromToLabel = new System.Windows.Forms.Label();
            this.topTrainListView = new System.Windows.Forms.ListView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bottomFromToLabel = new System.Windows.Forms.Label();
            this.bottomTrainListView = new System.Windows.Forms.ListView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.topDeleteTrainButton = new System.Windows.Forms.Button();
            this.topEditTrainButton = new System.Windows.Forms.Button();
            this.topNewTrainButton = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.bottomDeleteTrainButton = new System.Windows.Forms.Button();
            this.bottomEditTrainButton = new System.Windows.Forms.Button();
            this.bottomNewTrainButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(743, 517);
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
            this.cancelButton.Location = new System.Drawing.Point(662, 517);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(806, 499);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.topFromToLabel);
            this.panel1.Controls.Add(this.topTrainListView);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(680, 243);
            this.panel1.TabIndex = 0;
            // 
            // topFromToLabel
            // 
            this.topFromToLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topFromToLabel.Location = new System.Drawing.Point(3, 0);
            this.topFromToLabel.Name = "topFromToLabel";
            this.topFromToLabel.Size = new System.Drawing.Size(674, 15);
            this.topFromToLabel.TabIndex = 27;
            this.topFromToLabel.Text = "Züge von ... nach ...";
            this.topFromToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // topTrainListView
            // 
            this.topTrainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topTrainListView.FullRowSelect = true;
            this.topTrainListView.HideSelection = false;
            this.topTrainListView.Location = new System.Drawing.Point(0, 18);
            this.topTrainListView.MultiSelect = false;
            this.topTrainListView.Name = "topTrainListView";
            this.topTrainListView.Size = new System.Drawing.Size(677, 222);
            this.topTrainListView.TabIndex = 8;
            this.topTrainListView.UseCompatibleStateImageBehavior = false;
            this.topTrainListView.View = System.Windows.Forms.View.Details;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.bottomFromToLabel);
            this.panel2.Controls.Add(this.bottomTrainListView);
            this.panel2.Location = new System.Drawing.Point(3, 252);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(680, 244);
            this.panel2.TabIndex = 1;
            // 
            // bottomFromToLabel
            // 
            this.bottomFromToLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomFromToLabel.Location = new System.Drawing.Point(3, 0);
            this.bottomFromToLabel.Name = "bottomFromToLabel";
            this.bottomFromToLabel.Size = new System.Drawing.Size(671, 15);
            this.bottomFromToLabel.TabIndex = 29;
            this.bottomFromToLabel.Text = "Züge von ... nach ...";
            this.bottomFromToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomTrainListView
            // 
            this.bottomTrainListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomTrainListView.FullRowSelect = true;
            this.bottomTrainListView.HideSelection = false;
            this.bottomTrainListView.Location = new System.Drawing.Point(6, 18);
            this.bottomTrainListView.MultiSelect = false;
            this.bottomTrainListView.Name = "bottomTrainListView";
            this.bottomTrainListView.Size = new System.Drawing.Size(671, 223);
            this.bottomTrainListView.TabIndex = 28;
            this.bottomTrainListView.UseCompatibleStateImageBehavior = false;
            this.bottomTrainListView.View = System.Windows.Forms.View.Details;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.topDeleteTrainButton);
            this.panel3.Controls.Add(this.topEditTrainButton);
            this.panel3.Controls.Add(this.topNewTrainButton);
            this.panel3.Location = new System.Drawing.Point(689, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(114, 243);
            this.panel3.TabIndex = 2;
            // 
            // topDeleteTrainButton
            // 
            this.topDeleteTrainButton.Location = new System.Drawing.Point(3, 76);
            this.topDeleteTrainButton.Name = "topDeleteTrainButton";
            this.topDeleteTrainButton.Size = new System.Drawing.Size(108, 23);
            this.topDeleteTrainButton.TabIndex = 2;
            this.topDeleteTrainButton.Text = "Zug löschen";
            this.topDeleteTrainButton.UseVisualStyleBackColor = true;
            this.topDeleteTrainButton.Click += new System.EventHandler(this.topDeleteTrainButton_Click);
            // 
            // topEditTrainButton
            // 
            this.topEditTrainButton.Location = new System.Drawing.Point(3, 47);
            this.topEditTrainButton.Name = "topEditTrainButton";
            this.topEditTrainButton.Size = new System.Drawing.Size(108, 23);
            this.topEditTrainButton.TabIndex = 1;
            this.topEditTrainButton.Text = "Zug bearbeiten";
            this.topEditTrainButton.UseVisualStyleBackColor = true;
            this.topEditTrainButton.Click += new System.EventHandler(this.topEditTrainButton_Click);
            // 
            // topNewTrainButton
            // 
            this.topNewTrainButton.Location = new System.Drawing.Point(3, 18);
            this.topNewTrainButton.Name = "topNewTrainButton";
            this.topNewTrainButton.Size = new System.Drawing.Size(108, 23);
            this.topNewTrainButton.TabIndex = 0;
            this.topNewTrainButton.Text = "Neuer Zug...";
            this.topNewTrainButton.UseVisualStyleBackColor = true;
            this.topNewTrainButton.Click += new System.EventHandler(this.topNewTrainButton_Click);
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.Controls.Add(this.bottomDeleteTrainButton);
            this.panel4.Controls.Add(this.bottomEditTrainButton);
            this.panel4.Controls.Add(this.bottomNewTrainButton);
            this.panel4.Location = new System.Drawing.Point(689, 252);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(114, 244);
            this.panel4.TabIndex = 3;
            // 
            // bottomDeleteTrainButton
            // 
            this.bottomDeleteTrainButton.Location = new System.Drawing.Point(3, 76);
            this.bottomDeleteTrainButton.Name = "bottomDeleteTrainButton";
            this.bottomDeleteTrainButton.Size = new System.Drawing.Size(108, 23);
            this.bottomDeleteTrainButton.TabIndex = 5;
            this.bottomDeleteTrainButton.Text = "Zug löschen";
            this.bottomDeleteTrainButton.UseVisualStyleBackColor = true;
            this.bottomDeleteTrainButton.Click += new System.EventHandler(this.bottomDeleteTrainButton_Click);
            // 
            // bottomEditTrainButton
            // 
            this.bottomEditTrainButton.Location = new System.Drawing.Point(3, 47);
            this.bottomEditTrainButton.Name = "bottomEditTrainButton";
            this.bottomEditTrainButton.Size = new System.Drawing.Size(108, 23);
            this.bottomEditTrainButton.TabIndex = 4;
            this.bottomEditTrainButton.Text = "Zug bearbeiten";
            this.bottomEditTrainButton.UseVisualStyleBackColor = true;
            this.bottomEditTrainButton.Click += new System.EventHandler(this.bottomEditTrainButton_Click);
            // 
            // bottomNewTrainButton
            // 
            this.bottomNewTrainButton.Location = new System.Drawing.Point(3, 18);
            this.bottomNewTrainButton.Name = "bottomNewTrainButton";
            this.bottomNewTrainButton.Size = new System.Drawing.Size(108, 23);
            this.bottomNewTrainButton.TabIndex = 3;
            this.bottomNewTrainButton.Text = "Neuer Zug...";
            this.bottomNewTrainButton.UseVisualStyleBackColor = true;
            this.bottomNewTrainButton.Click += new System.EventHandler(this.bottomNewTrainButton_Click);
            // 
            // TrainsEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(830, 552);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TrainsEditForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Züge bearbeiten...";
            this.Load += new System.EventHandler(this.TrainsEditForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView topTrainListView;
        private System.Windows.Forms.Label topFromToLabel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label bottomFromToLabel;
        private System.Windows.Forms.ListView bottomTrainListView;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button topDeleteTrainButton;
        private System.Windows.Forms.Button topEditTrainButton;
        private System.Windows.Forms.Button topNewTrainButton;
        private System.Windows.Forms.Button bottomDeleteTrainButton;
        private System.Windows.Forms.Button bottomEditTrainButton;
        private System.Windows.Forms.Button bottomNewTrainButton;
    }
}