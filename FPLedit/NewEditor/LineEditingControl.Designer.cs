namespace FPLedit.NewEditor
{
    partial class LineEditingControl
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.newButton = new System.Windows.Forms.Button();
            this.routesComboBox = new System.Windows.Forms.ComboBox();
            this.newLineButton = new System.Windows.Forms.Button();
            this.toolbarFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.dividerPanel = new System.Windows.Forms.Panel();
            this.lineRenderer = new FPLedit.NewEditor.LineRenderer();
            this.toolbarFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // newButton
            // 
            this.newButton.Enabled = false;
            this.newButton.Location = new System.Drawing.Point(130, 3);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(86, 23);
            this.newButton.TabIndex = 1;
            this.newButton.Text = "Neue Station";
            this.newButton.UseVisualStyleBackColor = true;
            // 
            // routesComboBox
            // 
            this.routesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routesComboBox.Enabled = false;
            this.routesComboBox.FormattingEnabled = true;
            this.routesComboBox.Location = new System.Drawing.Point(3, 3);
            this.routesComboBox.Name = "routesComboBox";
            this.routesComboBox.Size = new System.Drawing.Size(121, 21);
            this.routesComboBox.TabIndex = 2;
            // 
            // newLineButton
            // 
            this.newLineButton.Enabled = false;
            this.newLineButton.Location = new System.Drawing.Point(230, 3);
            this.newLineButton.Name = "newLineButton";
            this.newLineButton.Size = new System.Drawing.Size(86, 23);
            this.newLineButton.TabIndex = 3;
            this.newLineButton.Text = "Neue Strecke";
            this.newLineButton.UseVisualStyleBackColor = true;
            // 
            // toolbarFlowLayoutPanel
            // 
            this.toolbarFlowLayoutPanel.Controls.Add(this.routesComboBox);
            this.toolbarFlowLayoutPanel.Controls.Add(this.newButton);
            this.toolbarFlowLayoutPanel.Controls.Add(this.dividerPanel);
            this.toolbarFlowLayoutPanel.Controls.Add(this.newLineButton);
            this.toolbarFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.toolbarFlowLayoutPanel.Name = "toolbarFlowLayoutPanel";
            this.toolbarFlowLayoutPanel.Size = new System.Drawing.Size(618, 33);
            this.toolbarFlowLayoutPanel.TabIndex = 4;
            // 
            // dividerPanel
            // 
            this.dividerPanel.BackColor = System.Drawing.Color.Gray;
            this.dividerPanel.Location = new System.Drawing.Point(222, 3);
            this.dividerPanel.Name = "dividerPanel";
            this.dividerPanel.Size = new System.Drawing.Size(2, 23);
            this.dividerPanel.TabIndex = 4;
            // 
            // lineRenderer
            // 
            this.lineRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lineRenderer.Location = new System.Drawing.Point(0, 39);
            this.lineRenderer.Name = "lineRenderer";
            this.lineRenderer.SelectedRoute = 0;
            this.lineRenderer.Size = new System.Drawing.Size(618, 285);
            this.lineRenderer.StationMovingEnabled = true;
            this.lineRenderer.TabIndex = 0;
            this.lineRenderer.Text = "lineRenderer1";
            // 
            // LineEditingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.toolbarFlowLayoutPanel);
            this.Controls.Add(this.lineRenderer);
            this.Name = "LineEditingControl";
            this.Size = new System.Drawing.Size(618, 324);
            this.toolbarFlowLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private LineRenderer lineRenderer;
        private System.Windows.Forms.Button newButton;
        private System.Windows.Forms.ComboBox routesComboBox;
        private System.Windows.Forms.Button newLineButton;
        private System.Windows.Forms.FlowLayoutPanel toolbarFlowLayoutPanel;
        private System.Windows.Forms.Panel dividerPanel;
    }
}
