namespace FPLedit.BildfahrplanExport
{
    partial class ConfigForm
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.stationFontComboBox = new System.Windows.Forms.ComboBox();
            this.stationFontLabel = new System.Windows.Forms.Label();
            this.timeColorLabel = new System.Windows.Forms.Label();
            this.timeColorComboBox = new System.Windows.Forms.ComboBox();
            this.trainColorLabel = new System.Windows.Forms.Label();
            this.trainColorComboBox = new System.Windows.Forms.ComboBox();
            this.stationColorLabel = new System.Windows.Forms.Label();
            this.stationColorComboBox = new System.Windows.Forms.ComboBox();
            this.bgColorLabel = new System.Windows.Forms.Label();
            this.bgColorComboBox = new System.Windows.Forms.ComboBox();
            this.timeFontLabel = new System.Windows.Forms.Label();
            this.timeFontComboBox = new System.Windows.Forms.ComboBox();
            this.trainFontLabel = new System.Windows.Forms.Label();
            this.trainFontComboBox = new System.Windows.Forms.ComboBox();
            this.stationLinesCheckBox = new System.Windows.Forms.CheckBox();
            this.stationFontSizeComboBox = new System.Windows.Forms.ComboBox();
            this.timeFontSizeComboBox = new System.Windows.Forms.ComboBox();
            this.trainFontSizeComboBox = new System.Windows.Forms.ComboBox();
            this.stationWidthComboBox = new System.Windows.Forms.ComboBox();
            this.hourTimeWidthComboBox = new System.Windows.Forms.ComboBox();
            this.trainWidthComboBox = new System.Windows.Forms.ComboBox();
            this.heightPerHourTextBox = new System.Windows.Forms.TextBox();
            this.endTimeLabel = new System.Windows.Forms.Label();
            this.startTimeLabel = new System.Windows.Forms.Label();
            this.heightPerHourLabel = new System.Windows.Forms.Label();
            this.minuteTimeWidthComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.layoutTextBox1 = new System.Windows.Forms.TextBox();
            this.defaultValuesButton = new System.Windows.Forms.Button();
            this.startTimeTextBox = new System.Windows.Forms.MaskedTextBox();
            this.endTimeTextBox = new System.Windows.Forms.MaskedTextBox();
            this.layoutTextBox2 = new System.Windows.Forms.TextBox();
            this.includeKilometreCheckBox = new System.Windows.Forms.CheckBox();
            this.layoutTextBox3 = new System.Windows.Forms.TextBox();
            this.drawStationNamesCheckBox = new System.Windows.Forms.CheckBox();
            this.heightPerHourValidator = new FPLedit.Shared.Validators.NumberValidator();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(296, 427);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 15;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(377, 427);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 16;
            this.closeButton.Text = "Schließen";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // stationFontComboBox
            // 
            this.stationFontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stationFontComboBox.FormattingEnabled = true;
            this.stationFontComboBox.Location = new System.Drawing.Point(239, 168);
            this.stationFontComboBox.Name = "stationFontComboBox";
            this.stationFontComboBox.Size = new System.Drawing.Size(150, 21);
            this.stationFontComboBox.TabIndex = 17;
            // 
            // stationFontLabel
            // 
            this.stationFontLabel.AutoSize = true;
            this.stationFontLabel.Location = new System.Drawing.Point(12, 171);
            this.stationFontLabel.Name = "stationFontLabel";
            this.stationFontLabel.Size = new System.Drawing.Size(128, 13);
            this.stationFontLabel.TabIndex = 18;
            this.stationFontLabel.Text = "Bahnhofsschriftart, -größe";
            // 
            // timeColorLabel
            // 
            this.timeColorLabel.AutoSize = true;
            this.timeColorLabel.Location = new System.Drawing.Point(12, 63);
            this.timeColorLabel.Name = "timeColorLabel";
            this.timeColorLabel.Size = new System.Drawing.Size(73, 13);
            this.timeColorLabel.TabIndex = 26;
            this.timeColorLabel.Text = "Zeitlinienfarbe";
            // 
            // timeColorComboBox
            // 
            this.timeColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.timeColorComboBox.FormattingEnabled = true;
            this.timeColorComboBox.Location = new System.Drawing.Point(239, 60);
            this.timeColorComboBox.Name = "timeColorComboBox";
            this.timeColorComboBox.Size = new System.Drawing.Size(150, 21);
            this.timeColorComboBox.TabIndex = 25;
            // 
            // trainColorLabel
            // 
            this.trainColorLabel.AutoSize = true;
            this.trainColorLabel.Location = new System.Drawing.Point(13, 144);
            this.trainColorLabel.Name = "trainColorLabel";
            this.trainColorLabel.Size = new System.Drawing.Size(112, 13);
            this.trainColorLabel.TabIndex = 24;
            this.trainColorLabel.Text = "Zuglinienfarbe, -stärke";
            // 
            // trainColorComboBox
            // 
            this.trainColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trainColorComboBox.FormattingEnabled = true;
            this.trainColorComboBox.Location = new System.Drawing.Point(239, 141);
            this.trainColorComboBox.Name = "trainColorComboBox";
            this.trainColorComboBox.Size = new System.Drawing.Size(150, 21);
            this.trainColorComboBox.TabIndex = 23;
            // 
            // stationColorLabel
            // 
            this.stationColorLabel.AutoSize = true;
            this.stationColorLabel.Location = new System.Drawing.Point(12, 36);
            this.stationColorLabel.Name = "stationColorLabel";
            this.stationColorLabel.Size = new System.Drawing.Size(138, 13);
            this.stationColorLabel.TabIndex = 22;
            this.stationColorLabel.Text = "Bahnhofslinienfarbe, -stärke";
            // 
            // stationColorComboBox
            // 
            this.stationColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stationColorComboBox.FormattingEnabled = true;
            this.stationColorComboBox.Location = new System.Drawing.Point(239, 33);
            this.stationColorComboBox.Name = "stationColorComboBox";
            this.stationColorComboBox.Size = new System.Drawing.Size(150, 21);
            this.stationColorComboBox.TabIndex = 21;
            // 
            // bgColorLabel
            // 
            this.bgColorLabel.AutoSize = true;
            this.bgColorLabel.Location = new System.Drawing.Point(12, 9);
            this.bgColorLabel.Name = "bgColorLabel";
            this.bgColorLabel.Size = new System.Drawing.Size(86, 13);
            this.bgColorLabel.TabIndex = 20;
            this.bgColorLabel.Text = "Hintergrundfarbe";
            // 
            // bgColorComboBox
            // 
            this.bgColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bgColorComboBox.FormattingEnabled = true;
            this.bgColorComboBox.Location = new System.Drawing.Point(239, 6);
            this.bgColorComboBox.Name = "bgColorComboBox";
            this.bgColorComboBox.Size = new System.Drawing.Size(150, 21);
            this.bgColorComboBox.TabIndex = 19;
            // 
            // timeFontLabel
            // 
            this.timeFontLabel.AutoSize = true;
            this.timeFontLabel.Location = new System.Drawing.Point(12, 198);
            this.timeFontLabel.Name = "timeFontLabel";
            this.timeFontLabel.Size = new System.Drawing.Size(113, 13);
            this.timeFontLabel.TabIndex = 28;
            this.timeFontLabel.Text = "Zeitenschriftart, -größe";
            // 
            // timeFontComboBox
            // 
            this.timeFontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.timeFontComboBox.FormattingEnabled = true;
            this.timeFontComboBox.Location = new System.Drawing.Point(239, 195);
            this.timeFontComboBox.Name = "timeFontComboBox";
            this.timeFontComboBox.Size = new System.Drawing.Size(150, 21);
            this.timeFontComboBox.TabIndex = 27;
            // 
            // trainFontLabel
            // 
            this.trainFontLabel.AutoSize = true;
            this.trainFontLabel.Location = new System.Drawing.Point(12, 225);
            this.trainFontLabel.Name = "trainFontLabel";
            this.trainFontLabel.Size = new System.Drawing.Size(145, 13);
            this.trainFontLabel.TabIndex = 30;
            this.trainFontLabel.Text = "Zugnummernschriftart, -größe";
            // 
            // trainFontComboBox
            // 
            this.trainFontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trainFontComboBox.FormattingEnabled = true;
            this.trainFontComboBox.Location = new System.Drawing.Point(239, 222);
            this.trainFontComboBox.Name = "trainFontComboBox";
            this.trainFontComboBox.Size = new System.Drawing.Size(150, 21);
            this.trainFontComboBox.TabIndex = 29;
            // 
            // stationLinesCheckBox
            // 
            this.stationLinesCheckBox.AutoSize = true;
            this.stationLinesCheckBox.Location = new System.Drawing.Point(15, 251);
            this.stationLinesCheckBox.Name = "stationLinesCheckBox";
            this.stationLinesCheckBox.Size = new System.Drawing.Size(210, 17);
            this.stationLinesCheckBox.TabIndex = 31;
            this.stationLinesCheckBox.Text = "Linien für stehende Züge in Bahnhöfen";
            this.stationLinesCheckBox.UseVisualStyleBackColor = true;
            // 
            // stationFontSizeComboBox
            // 
            this.stationFontSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stationFontSizeComboBox.FormattingEnabled = true;
            this.stationFontSizeComboBox.Location = new System.Drawing.Point(395, 168);
            this.stationFontSizeComboBox.Name = "stationFontSizeComboBox";
            this.stationFontSizeComboBox.Size = new System.Drawing.Size(57, 21);
            this.stationFontSizeComboBox.TabIndex = 32;
            // 
            // timeFontSizeComboBox
            // 
            this.timeFontSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.timeFontSizeComboBox.FormattingEnabled = true;
            this.timeFontSizeComboBox.Location = new System.Drawing.Point(395, 195);
            this.timeFontSizeComboBox.Name = "timeFontSizeComboBox";
            this.timeFontSizeComboBox.Size = new System.Drawing.Size(57, 21);
            this.timeFontSizeComboBox.TabIndex = 33;
            // 
            // trainFontSizeComboBox
            // 
            this.trainFontSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trainFontSizeComboBox.FormattingEnabled = true;
            this.trainFontSizeComboBox.Location = new System.Drawing.Point(395, 222);
            this.trainFontSizeComboBox.Name = "trainFontSizeComboBox";
            this.trainFontSizeComboBox.Size = new System.Drawing.Size(57, 21);
            this.trainFontSizeComboBox.TabIndex = 34;
            // 
            // stationWidthComboBox
            // 
            this.stationWidthComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stationWidthComboBox.FormattingEnabled = true;
            this.stationWidthComboBox.Location = new System.Drawing.Point(395, 33);
            this.stationWidthComboBox.Name = "stationWidthComboBox";
            this.stationWidthComboBox.Size = new System.Drawing.Size(57, 21);
            this.stationWidthComboBox.TabIndex = 35;
            // 
            // hourTimeWidthComboBox
            // 
            this.hourTimeWidthComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.hourTimeWidthComboBox.FormattingEnabled = true;
            this.hourTimeWidthComboBox.Location = new System.Drawing.Point(395, 87);
            this.hourTimeWidthComboBox.Name = "hourTimeWidthComboBox";
            this.hourTimeWidthComboBox.Size = new System.Drawing.Size(57, 21);
            this.hourTimeWidthComboBox.TabIndex = 36;
            // 
            // trainWidthComboBox
            // 
            this.trainWidthComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trainWidthComboBox.FormattingEnabled = true;
            this.trainWidthComboBox.Location = new System.Drawing.Point(395, 141);
            this.trainWidthComboBox.Name = "trainWidthComboBox";
            this.trainWidthComboBox.Size = new System.Drawing.Size(57, 21);
            this.trainWidthComboBox.TabIndex = 37;
            // 
            // heightPerHourTextBox
            // 
            this.heightPerHourTextBox.Location = new System.Drawing.Point(239, 327);
            this.heightPerHourTextBox.Name = "heightPerHourTextBox";
            this.heightPerHourTextBox.Size = new System.Drawing.Size(150, 20);
            this.heightPerHourTextBox.TabIndex = 41;
            // 
            // endTimeLabel
            // 
            this.endTimeLabel.AutoSize = true;
            this.endTimeLabel.Location = new System.Drawing.Point(12, 386);
            this.endTimeLabel.Name = "endTimeLabel";
            this.endTimeLabel.Size = new System.Drawing.Size(42, 13);
            this.endTimeLabel.TabIndex = 40;
            this.endTimeLabel.Text = "Endzeit";
            // 
            // startTimeLabel
            // 
            this.startTimeLabel.AutoSize = true;
            this.startTimeLabel.Location = new System.Drawing.Point(12, 360);
            this.startTimeLabel.Name = "startTimeLabel";
            this.startTimeLabel.Size = new System.Drawing.Size(45, 13);
            this.startTimeLabel.TabIndex = 39;
            this.startTimeLabel.Text = "Startzeit";
            // 
            // heightPerHourLabel
            // 
            this.heightPerHourLabel.AutoSize = true;
            this.heightPerHourLabel.Location = new System.Drawing.Point(12, 334);
            this.heightPerHourLabel.Name = "heightPerHourLabel";
            this.heightPerHourLabel.Size = new System.Drawing.Size(88, 13);
            this.heightPerHourLabel.TabIndex = 38;
            this.heightPerHourLabel.Text = "Höhe pro Stunde";
            // 
            // minuteTimeWidthComboBox
            // 
            this.minuteTimeWidthComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.minuteTimeWidthComboBox.FormattingEnabled = true;
            this.minuteTimeWidthComboBox.Location = new System.Drawing.Point(395, 114);
            this.minuteTimeWidthComboBox.Name = "minuteTimeWidthComboBox";
            this.minuteTimeWidthComboBox.Size = new System.Drawing.Size(57, 21);
            this.minuteTimeWidthComboBox.TabIndex = 46;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 13);
            this.label1.TabIndex = 47;
            this.label1.Text = "Zeitlinienstärke zur vollen Stunde";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 117);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 48;
            this.label2.Text = "Zeitlinienstärke";
            // 
            // layoutTextBox1
            // 
            this.layoutTextBox1.Location = new System.Drawing.Point(239, 249);
            this.layoutTextBox1.Name = "layoutTextBox1";
            this.layoutTextBox1.Size = new System.Drawing.Size(10, 20);
            this.layoutTextBox1.TabIndex = 49;
            this.layoutTextBox1.Visible = false;
            // 
            // defaultValuesButton
            // 
            this.defaultValuesButton.Location = new System.Drawing.Point(12, 427);
            this.defaultValuesButton.Name = "defaultValuesButton";
            this.defaultValuesButton.Size = new System.Drawing.Size(182, 23);
            this.defaultValuesButton.TabIndex = 50;
            this.defaultValuesButton.Text = "Auf Standardwerte zurücksetzen";
            this.defaultValuesButton.UseVisualStyleBackColor = true;
            this.defaultValuesButton.Click += new System.EventHandler(this.defaultValuesButton_Click);
            // 
            // startTimeTextBox
            // 
            this.startTimeTextBox.Location = new System.Drawing.Point(239, 357);
            this.startTimeTextBox.Mask = "90:00";
            this.startTimeTextBox.Name = "startTimeTextBox";
            this.startTimeTextBox.Size = new System.Drawing.Size(150, 20);
            this.startTimeTextBox.TabIndex = 51;
            // 
            // endTimeTextBox
            // 
            this.endTimeTextBox.Location = new System.Drawing.Point(239, 383);
            this.endTimeTextBox.Mask = "90:00";
            this.endTimeTextBox.Name = "endTimeTextBox";
            this.endTimeTextBox.Size = new System.Drawing.Size(150, 20);
            this.endTimeTextBox.TabIndex = 52;
            // 
            // layoutTextBox2
            // 
            this.layoutTextBox2.Location = new System.Drawing.Point(239, 275);
            this.layoutTextBox2.Name = "layoutTextBox2";
            this.layoutTextBox2.Size = new System.Drawing.Size(10, 20);
            this.layoutTextBox2.TabIndex = 53;
            this.layoutTextBox2.Visible = false;
            // 
            // includeKilometreCheckBox
            // 
            this.includeKilometreCheckBox.AutoSize = true;
            this.includeKilometreCheckBox.Location = new System.Drawing.Point(15, 277);
            this.includeKilometreCheckBox.Name = "includeKilometreCheckBox";
            this.includeKilometreCheckBox.Size = new System.Drawing.Size(157, 17);
            this.includeKilometreCheckBox.TabIndex = 54;
            this.includeKilometreCheckBox.Text = "Streckenkilometer anzeigen";
            this.includeKilometreCheckBox.UseVisualStyleBackColor = true;
            // 
            // layoutTextBox3
            // 
            this.layoutTextBox3.Location = new System.Drawing.Point(239, 301);
            this.layoutTextBox3.Name = "layoutTextBox3";
            this.layoutTextBox3.Size = new System.Drawing.Size(10, 20);
            this.layoutTextBox3.TabIndex = 55;
            this.layoutTextBox3.Visible = false;
            // 
            // drawStationNamesCheckBox
            // 
            this.drawStationNamesCheckBox.AutoSize = true;
            this.drawStationNamesCheckBox.Location = new System.Drawing.Point(15, 303);
            this.drawStationNamesCheckBox.Name = "drawStationNamesCheckBox";
            this.drawStationNamesCheckBox.Size = new System.Drawing.Size(142, 17);
            this.drawStationNamesCheckBox.TabIndex = 56;
            this.drawStationNamesCheckBox.Text = "Stationsnamen anzeigen";
            this.drawStationNamesCheckBox.UseVisualStyleBackColor = true;
            // 
            // heightPerHourValidator
            // 
            this.heightPerHourValidator.Control = this.heightPerHourTextBox;
            this.heightPerHourValidator.ErrorMessage = "Bitte eine Zahl angeben!";
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 462);
            this.Controls.Add(this.drawStationNamesCheckBox);
            this.Controls.Add(this.layoutTextBox3);
            this.Controls.Add(this.includeKilometreCheckBox);
            this.Controls.Add(this.layoutTextBox2);
            this.Controls.Add(this.endTimeTextBox);
            this.Controls.Add(this.startTimeTextBox);
            this.Controls.Add(this.defaultValuesButton);
            this.Controls.Add(this.layoutTextBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.minuteTimeWidthComboBox);
            this.Controls.Add(this.heightPerHourTextBox);
            this.Controls.Add(this.endTimeLabel);
            this.Controls.Add(this.startTimeLabel);
            this.Controls.Add(this.heightPerHourLabel);
            this.Controls.Add(this.trainWidthComboBox);
            this.Controls.Add(this.hourTimeWidthComboBox);
            this.Controls.Add(this.stationWidthComboBox);
            this.Controls.Add(this.trainFontSizeComboBox);
            this.Controls.Add(this.timeFontSizeComboBox);
            this.Controls.Add(this.stationFontSizeComboBox);
            this.Controls.Add(this.stationLinesCheckBox);
            this.Controls.Add(this.trainFontLabel);
            this.Controls.Add(this.trainFontComboBox);
            this.Controls.Add(this.timeFontLabel);
            this.Controls.Add(this.timeFontComboBox);
            this.Controls.Add(this.timeColorLabel);
            this.Controls.Add(this.timeColorComboBox);
            this.Controls.Add(this.trainColorLabel);
            this.Controls.Add(this.trainColorComboBox);
            this.Controls.Add(this.stationColorLabel);
            this.Controls.Add(this.stationColorComboBox);
            this.Controls.Add(this.bgColorLabel);
            this.Controls.Add(this.bgColorComboBox);
            this.Controls.Add(this.stationFontLabel);
            this.Controls.Add(this.stationFontComboBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Darstellung ändern";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ComboBox stationFontComboBox;
        private System.Windows.Forms.Label stationFontLabel;
        private System.Windows.Forms.Label timeColorLabel;
        private System.Windows.Forms.ComboBox timeColorComboBox;
        private System.Windows.Forms.Label trainColorLabel;
        private System.Windows.Forms.ComboBox trainColorComboBox;
        private System.Windows.Forms.Label stationColorLabel;
        private System.Windows.Forms.ComboBox stationColorComboBox;
        private System.Windows.Forms.Label bgColorLabel;
        private System.Windows.Forms.ComboBox bgColorComboBox;
        private System.Windows.Forms.Label timeFontLabel;
        private System.Windows.Forms.ComboBox timeFontComboBox;
        private System.Windows.Forms.Label trainFontLabel;
        private System.Windows.Forms.ComboBox trainFontComboBox;
        private System.Windows.Forms.CheckBox stationLinesCheckBox;
        private System.Windows.Forms.ComboBox stationFontSizeComboBox;
        private System.Windows.Forms.ComboBox timeFontSizeComboBox;
        private System.Windows.Forms.ComboBox trainFontSizeComboBox;
        private System.Windows.Forms.ComboBox stationWidthComboBox;
        private System.Windows.Forms.ComboBox hourTimeWidthComboBox;
        private System.Windows.Forms.ComboBox trainWidthComboBox;
        private System.Windows.Forms.TextBox heightPerHourTextBox;
        private System.Windows.Forms.Label endTimeLabel;
        private System.Windows.Forms.Label startTimeLabel;
        private System.Windows.Forms.Label heightPerHourLabel;
        private System.Windows.Forms.ComboBox minuteTimeWidthComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox layoutTextBox1;
        private System.Windows.Forms.Button defaultValuesButton;
        private System.Windows.Forms.MaskedTextBox startTimeTextBox;
        private System.Windows.Forms.MaskedTextBox endTimeTextBox;
        private System.Windows.Forms.TextBox layoutTextBox2;
        private System.Windows.Forms.CheckBox includeKilometreCheckBox;
        private System.Windows.Forms.TextBox layoutTextBox3;
        private System.Windows.Forms.CheckBox drawStationNamesCheckBox;
        private Shared.Validators.NumberValidator heightPerHourValidator;
    }
}