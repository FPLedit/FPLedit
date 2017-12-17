namespace FPLedit.NewEditor
{
    partial class TrainsEditingForm
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
            this.nameValidator = new FPLedit.Shared.Validators.NotEmptyValidator();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.topListView = new System.Windows.Forms.ListView();
            this.topActionsPanel = new System.Windows.Forms.Panel();
            this.editTimetableButton = new System.Windows.Forms.Button();
            this.topDeleteButton = new System.Windows.Forms.Button();
            this.topEditButton = new System.Windows.Forms.Button();
            this.topNewButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.topActionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameValidator
            // 
            this.nameValidator.Control = null;
            this.nameValidator.ErrorMessage = "Bitte einen Namen eingeben!";
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.ColumnCount = 2;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainTableLayoutPanel.Controls.Add(this.topListView, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.topActionsPanel, 1, 0);
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 1;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(800, 425);
            this.mainTableLayoutPanel.TabIndex = 2;
            // 
            // topListView
            // 
            this.topListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topListView.FullRowSelect = true;
            this.topListView.HideSelection = false;
            this.topListView.Location = new System.Drawing.Point(3, 3);
            this.topListView.MultiSelect = false;
            this.topListView.Name = "topListView";
            this.topListView.Size = new System.Drawing.Size(674, 419);
            this.topListView.TabIndex = 4;
            this.topListView.UseCompatibleStateImageBehavior = false;
            this.topListView.View = System.Windows.Forms.View.Details;
            // 
            // topActionsPanel
            // 
            this.topActionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topActionsPanel.Controls.Add(this.editTimetableButton);
            this.topActionsPanel.Controls.Add(this.topDeleteButton);
            this.topActionsPanel.Controls.Add(this.topEditButton);
            this.topActionsPanel.Controls.Add(this.topNewButton);
            this.topActionsPanel.Location = new System.Drawing.Point(683, 3);
            this.topActionsPanel.Name = "topActionsPanel";
            this.topActionsPanel.Size = new System.Drawing.Size(114, 419);
            this.topActionsPanel.TabIndex = 1;
            // 
            // editTimetableButton
            // 
            this.editTimetableButton.Location = new System.Drawing.Point(3, 131);
            this.editTimetableButton.Name = "editTimetableButton";
            this.editTimetableButton.Size = new System.Drawing.Size(108, 41);
            this.editTimetableButton.TabIndex = 3;
            this.editTimetableButton.Text = "Fahrplan bearbeiten";
            this.editTimetableButton.UseVisualStyleBackColor = true;
            // 
            // topDeleteButton
            // 
            this.topDeleteButton.Location = new System.Drawing.Point(0, 61);
            this.topDeleteButton.Name = "topDeleteButton";
            this.topDeleteButton.Size = new System.Drawing.Size(108, 23);
            this.topDeleteButton.TabIndex = 2;
            this.topDeleteButton.Text = "Zug löschen";
            this.topDeleteButton.UseVisualStyleBackColor = true;
            this.topDeleteButton.Click += new System.EventHandler(this.topDeleteButton_Click);
            // 
            // topEditButton
            // 
            this.topEditButton.Location = new System.Drawing.Point(0, 32);
            this.topEditButton.Name = "topEditButton";
            this.topEditButton.Size = new System.Drawing.Size(108, 23);
            this.topEditButton.TabIndex = 1;
            this.topEditButton.Text = "Zug bearbeiten";
            this.topEditButton.UseVisualStyleBackColor = true;
            this.topEditButton.Click += new System.EventHandler(this.topEditButton_Click);
            // 
            // topNewButton
            // 
            this.topNewButton.Location = new System.Drawing.Point(0, 3);
            this.topNewButton.Name = "topNewButton";
            this.topNewButton.Size = new System.Drawing.Size(108, 23);
            this.topNewButton.TabIndex = 0;
            this.topNewButton.Text = "Neuer Zug";
            this.topNewButton.UseVisualStyleBackColor = true;
            this.topNewButton.Click += new System.EventHandler(this.topNewButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(656, 443);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(737, 443);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 13;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // TrainsEditingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 478);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "TrainsEditingForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Züge bearbeiten";
            this.Load += new System.EventHandler(this.TrainsEditingForm_Load);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.topActionsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Shared.Validators.NotEmptyValidator nameValidator;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.ListView topListView;
        private System.Windows.Forms.Panel topActionsPanel;
        private System.Windows.Forms.Button topDeleteButton;
        private System.Windows.Forms.Button topEditButton;
        private System.Windows.Forms.Button topNewButton;
        private System.Windows.Forms.Button editTimetableButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
    }
}