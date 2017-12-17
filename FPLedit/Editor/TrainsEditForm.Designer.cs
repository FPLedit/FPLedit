namespace FPLedit.Editor
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
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.topContentPanel = new System.Windows.Forms.Panel();
            this.topLineLabel = new System.Windows.Forms.Label();
            this.topListView = new System.Windows.Forms.ListView();
            this.bottomContentPanel = new System.Windows.Forms.Panel();
            this.bottomLineLabel = new System.Windows.Forms.Label();
            this.bottomListView = new System.Windows.Forms.ListView();
            this.topActionsPanel = new System.Windows.Forms.Panel();
            this.topDeleteButton = new System.Windows.Forms.Button();
            this.topEditButton = new System.Windows.Forms.Button();
            this.topNewButton = new System.Windows.Forms.Button();
            this.bottomActionsPanel = new System.Windows.Forms.Panel();
            this.bottomDeleteButton = new System.Windows.Forms.Button();
            this.bottomEditButton = new System.Windows.Forms.Button();
            this.bottomNewButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.topContentPanel.SuspendLayout();
            this.bottomContentPanel.SuspendLayout();
            this.topActionsPanel.SuspendLayout();
            this.bottomActionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(743, 517);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 2;
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
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.ColumnCount = 2;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainTableLayoutPanel.Controls.Add(this.topContentPanel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.bottomContentPanel, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.topActionsPanel, 1, 0);
            this.mainTableLayoutPanel.Controls.Add(this.bottomActionsPanel, 1, 1);
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 2;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(806, 499);
            this.mainTableLayoutPanel.TabIndex = 1;
            // 
            // topContentPanel
            // 
            this.topContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topContentPanel.Controls.Add(this.topLineLabel);
            this.topContentPanel.Controls.Add(this.topListView);
            this.topContentPanel.Location = new System.Drawing.Point(3, 3);
            this.topContentPanel.Name = "topContentPanel";
            this.topContentPanel.Size = new System.Drawing.Size(680, 243);
            this.topContentPanel.TabIndex = 0;
            // 
            // topLineLabel
            // 
            this.topLineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topLineLabel.Location = new System.Drawing.Point(3, 0);
            this.topLineLabel.Name = "topLineLabel";
            this.topLineLabel.Size = new System.Drawing.Size(674, 15);
            this.topLineLabel.TabIndex = 27;
            this.topLineLabel.Text = "Züge von ... nach ...";
            this.topLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // topListView
            // 
            this.topListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topListView.FullRowSelect = true;
            this.topListView.HideSelection = false;
            this.topListView.Location = new System.Drawing.Point(0, 18);
            this.topListView.MultiSelect = false;
            this.topListView.Name = "topListView";
            this.topListView.Size = new System.Drawing.Size(677, 222);
            this.topListView.TabIndex = 1;
            this.topListView.UseCompatibleStateImageBehavior = false;
            this.topListView.View = System.Windows.Forms.View.Details;
            this.topListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.topListView_MouseDoubleClick);
            // 
            // bottomContentPanel
            // 
            this.bottomContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomContentPanel.Controls.Add(this.bottomLineLabel);
            this.bottomContentPanel.Controls.Add(this.bottomListView);
            this.bottomContentPanel.Location = new System.Drawing.Point(3, 252);
            this.bottomContentPanel.Name = "bottomContentPanel";
            this.bottomContentPanel.Size = new System.Drawing.Size(680, 244);
            this.bottomContentPanel.TabIndex = 2;
            // 
            // bottomLineLabel
            // 
            this.bottomLineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomLineLabel.Location = new System.Drawing.Point(3, 0);
            this.bottomLineLabel.Name = "bottomLineLabel";
            this.bottomLineLabel.Size = new System.Drawing.Size(671, 15);
            this.bottomLineLabel.TabIndex = 29;
            this.bottomLineLabel.Text = "Züge von ... nach ...";
            this.bottomLineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bottomListView
            // 
            this.bottomListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomListView.FullRowSelect = true;
            this.bottomListView.HideSelection = false;
            this.bottomListView.Location = new System.Drawing.Point(6, 18);
            this.bottomListView.MultiSelect = false;
            this.bottomListView.Name = "bottomListView";
            this.bottomListView.Size = new System.Drawing.Size(671, 223);
            this.bottomListView.TabIndex = 28;
            this.bottomListView.UseCompatibleStateImageBehavior = false;
            this.bottomListView.View = System.Windows.Forms.View.Details;
            this.bottomListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.bottomListView_MouseDoubleClick);
            // 
            // topActionsPanel
            // 
            this.topActionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topActionsPanel.Controls.Add(this.topDeleteButton);
            this.topActionsPanel.Controls.Add(this.topEditButton);
            this.topActionsPanel.Controls.Add(this.topNewButton);
            this.topActionsPanel.Location = new System.Drawing.Point(689, 3);
            this.topActionsPanel.Name = "topActionsPanel";
            this.topActionsPanel.Size = new System.Drawing.Size(114, 243);
            this.topActionsPanel.TabIndex = 1;
            // 
            // topDeleteButton
            // 
            this.topDeleteButton.Location = new System.Drawing.Point(3, 76);
            this.topDeleteButton.Name = "topDeleteButton";
            this.topDeleteButton.Size = new System.Drawing.Size(108, 23);
            this.topDeleteButton.TabIndex = 2;
            this.topDeleteButton.Text = "Zug löschen";
            this.topDeleteButton.UseVisualStyleBackColor = true;
            this.topDeleteButton.Click += new System.EventHandler(this.topDeleteButton_Click);
            // 
            // topEditButton
            // 
            this.topEditButton.Location = new System.Drawing.Point(3, 47);
            this.topEditButton.Name = "topEditButton";
            this.topEditButton.Size = new System.Drawing.Size(108, 23);
            this.topEditButton.TabIndex = 1;
            this.topEditButton.Text = "Zug bearbeiten";
            this.topEditButton.UseVisualStyleBackColor = true;
            this.topEditButton.Click += new System.EventHandler(this.topEditButton_Click);
            // 
            // topNewButton
            // 
            this.topNewButton.Location = new System.Drawing.Point(3, 18);
            this.topNewButton.Name = "topNewButton";
            this.topNewButton.Size = new System.Drawing.Size(108, 23);
            this.topNewButton.TabIndex = 0;
            this.topNewButton.Text = "Neuer Zug";
            this.topNewButton.UseVisualStyleBackColor = true;
            this.topNewButton.Click += new System.EventHandler(this.topNewButton_Click);
            // 
            // bottomActionsPanel
            // 
            this.bottomActionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomActionsPanel.Controls.Add(this.bottomDeleteButton);
            this.bottomActionsPanel.Controls.Add(this.bottomEditButton);
            this.bottomActionsPanel.Controls.Add(this.bottomNewButton);
            this.bottomActionsPanel.Location = new System.Drawing.Point(689, 252);
            this.bottomActionsPanel.Name = "bottomActionsPanel";
            this.bottomActionsPanel.Size = new System.Drawing.Size(114, 244);
            this.bottomActionsPanel.TabIndex = 3;
            // 
            // bottomDeleteButton
            // 
            this.bottomDeleteButton.Location = new System.Drawing.Point(3, 76);
            this.bottomDeleteButton.Name = "bottomDeleteButton";
            this.bottomDeleteButton.Size = new System.Drawing.Size(108, 23);
            this.bottomDeleteButton.TabIndex = 5;
            this.bottomDeleteButton.Text = "Zug löschen";
            this.bottomDeleteButton.UseVisualStyleBackColor = true;
            this.bottomDeleteButton.Click += new System.EventHandler(this.bottomDeleteButton_Click);
            // 
            // bottomEditButton
            // 
            this.bottomEditButton.Location = new System.Drawing.Point(3, 47);
            this.bottomEditButton.Name = "bottomEditButton";
            this.bottomEditButton.Size = new System.Drawing.Size(108, 23);
            this.bottomEditButton.TabIndex = 4;
            this.bottomEditButton.Text = "Zug bearbeiten";
            this.bottomEditButton.UseVisualStyleBackColor = true;
            this.bottomEditButton.Click += new System.EventHandler(this.bottomEditButton_Click);
            // 
            // bottomNewButton
            // 
            this.bottomNewButton.Location = new System.Drawing.Point(3, 18);
            this.bottomNewButton.Name = "bottomNewButton";
            this.bottomNewButton.Size = new System.Drawing.Size(108, 23);
            this.bottomNewButton.TabIndex = 3;
            this.bottomNewButton.Text = "Neuer Zug";
            this.bottomNewButton.UseVisualStyleBackColor = true;
            this.bottomNewButton.Click += new System.EventHandler(this.bottomNewButton_Click);
            // 
            // TrainsEditForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(830, 552);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Name = "TrainsEditForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Züge bearbeiten";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.topContentPanel.ResumeLayout(false);
            this.bottomContentPanel.ResumeLayout(false);
            this.topActionsPanel.ResumeLayout(false);
            this.bottomActionsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.Panel topContentPanel;
        private System.Windows.Forms.ListView topListView;
        private System.Windows.Forms.Label topLineLabel;
        private System.Windows.Forms.Panel bottomContentPanel;
        private System.Windows.Forms.Label bottomLineLabel;
        private System.Windows.Forms.ListView bottomListView;
        private System.Windows.Forms.Panel topActionsPanel;
        private System.Windows.Forms.Panel bottomActionsPanel;
        private System.Windows.Forms.Button topDeleteButton;
        private System.Windows.Forms.Button topEditButton;
        private System.Windows.Forms.Button topNewButton;
        private System.Windows.Forms.Button bottomDeleteButton;
        private System.Windows.Forms.Button bottomEditButton;
        private System.Windows.Forms.Button bottomNewButton;
    }
}