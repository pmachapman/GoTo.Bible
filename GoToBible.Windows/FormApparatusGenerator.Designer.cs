// -----------------------------------------------------------------------
// <copyright file="FormApparatusGenerator.Designer.cs" company="Conglomo">
// Copyright 2020-2022 Conglomo Limited. Please see LICENSE.md for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace GoToBible.Windows
{
    partial class FormApparatusGenerator
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormApparatusGenerator));
            this.ComboBoxBaseText = new System.Windows.Forms.ComboBox();
            this.CheckedListBoxComparisonTexts = new System.Windows.Forms.CheckedListBox();
            this.TextBoxIncludeApparatusData = new System.Windows.Forms.TextBox();
            this.ButtonBrowse = new System.Windows.Forms.Button();
            this.RadioButtonCsv = new System.Windows.Forms.RadioButton();
            this.RadioButtonHtml = new System.Windows.Forms.RadioButton();
            this.CheckedListBoxBooks = new System.Windows.Forms.CheckedListBox();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.LabelBaseText = new System.Windows.Forms.Label();
            this.LabelComparisonTexts = new System.Windows.Forms.Label();
            this.LabelBooks = new System.Windows.Forms.Label();
            this.LabelIncludeApparatusData = new System.Windows.Forms.Label();
            this.CheckBoxSelectAllBooks = new System.Windows.Forms.CheckBox();
            this.CheckBoxSelectAllComparisonTexts = new System.Windows.Forms.CheckBox();
            this.ToolTipItem = new System.Windows.Forms.ToolTip(this.components);
            this.OpenFileDialogMain = new System.Windows.Forms.OpenFileDialog();
            this.ProgressBarMain = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // ComboBoxBaseText
            // 
            this.ComboBoxBaseText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboBoxBaseText.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.ComboBoxBaseText.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBoxBaseText.FormattingEnabled = true;
            this.ComboBoxBaseText.Location = new System.Drawing.Point(164, 12);
            this.ComboBoxBaseText.Name = "ComboBoxBaseText";
            this.ComboBoxBaseText.Size = new System.Drawing.Size(294, 24);
            this.ComboBoxBaseText.TabIndex = 1;
            this.ComboBoxBaseText.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBoxBaseText_DrawItem);
            this.ComboBoxBaseText.SelectedIndexChanged += new System.EventHandler(this.ComboBoxBaseText_SelectedIndexChanged);
            // 
            // CheckedListBoxComparisonTexts
            // 
            this.CheckedListBoxComparisonTexts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckedListBoxComparisonTexts.FormattingEnabled = true;
            this.CheckedListBoxComparisonTexts.Location = new System.Drawing.Point(164, 41);
            this.CheckedListBoxComparisonTexts.Name = "CheckedListBoxComparisonTexts";
            this.CheckedListBoxComparisonTexts.Size = new System.Drawing.Size(294, 112);
            this.CheckedListBoxComparisonTexts.TabIndex = 3;
            this.CheckedListBoxComparisonTexts.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxComparisonTexts_ItemCheck);
            // 
            // TextBoxIncludeApparatusData
            // 
            this.TextBoxIncludeApparatusData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxIncludeApparatusData.Location = new System.Drawing.Point(164, 327);
            this.TextBoxIncludeApparatusData.Name = "TextBoxIncludeApparatusData";
            this.TextBoxIncludeApparatusData.ReadOnly = true;
            this.TextBoxIncludeApparatusData.Size = new System.Drawing.Size(174, 23);
            this.TextBoxIncludeApparatusData.TabIndex = 9;
            this.TextBoxIncludeApparatusData.Click += new System.EventHandler(this.TextBoxIncludeApparatusData_Click);
            // 
            // ButtonBrowse
            // 
            this.ButtonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonBrowse.Location = new System.Drawing.Point(344, 327);
            this.ButtonBrowse.Name = "ButtonBrowse";
            this.ButtonBrowse.Size = new System.Drawing.Size(114, 23);
            this.ButtonBrowse.TabIndex = 10;
            this.ButtonBrowse.Text = "&Browse...";
            this.ButtonBrowse.UseVisualStyleBackColor = true;
            this.ButtonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // RadioButtonCsv
            // 
            this.RadioButtonCsv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RadioButtonCsv.AutoSize = true;
            this.RadioButtonCsv.Checked = true;
            this.RadioButtonCsv.Location = new System.Drawing.Point(162, 356);
            this.RadioButtonCsv.Name = "RadioButtonCsv";
            this.RadioButtonCsv.Size = new System.Drawing.Size(163, 19);
            this.RadioButtonCsv.TabIndex = 11;
            this.RadioButtonCsv.TabStop = true;
            this.RadioButtonCsv.Text = "Export as CSV &spreadsheet";
            this.RadioButtonCsv.UseVisualStyleBackColor = true;
            // 
            // RadioButtonHtml
            // 
            this.RadioButtonHtml.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RadioButtonHtml.AutoSize = true;
            this.RadioButtonHtml.Location = new System.Drawing.Point(331, 356);
            this.RadioButtonHtml.Name = "RadioButtonHtml";
            this.RadioButtonHtml.Size = new System.Drawing.Size(127, 19);
            this.RadioButtonHtml.TabIndex = 12;
            this.RadioButtonHtml.Text = "Export as &HTML file";
            this.RadioButtonHtml.UseVisualStyleBackColor = true;
            // 
            // CheckedListBoxBooks
            // 
            this.CheckedListBoxBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckedListBoxBooks.FormattingEnabled = true;
            this.CheckedListBoxBooks.Location = new System.Drawing.Point(164, 184);
            this.CheckedListBoxBooks.Name = "CheckedListBoxBooks";
            this.CheckedListBoxBooks.Size = new System.Drawing.Size(294, 112);
            this.CheckedListBoxBooks.TabIndex = 6;
            this.CheckedListBoxBooks.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxBooks_ItemCheck);
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.Location = new System.Drawing.Point(302, 381);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
            this.ButtonOk.TabIndex = 13;
            this.ButtonOk.Text = "&Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.Location = new System.Drawing.Point(383, 381);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 14;
            this.ButtonCancel.Text = "&Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // LabelBaseText
            // 
            this.LabelBaseText.AutoSize = true;
            this.LabelBaseText.Location = new System.Drawing.Point(12, 15);
            this.LabelBaseText.Name = "LabelBaseText";
            this.LabelBaseText.Size = new System.Drawing.Size(101, 15);
            this.LabelBaseText.TabIndex = 0;
            this.LabelBaseText.Text = "&1. Select Base Text";
            // 
            // LabelComparisonTexts
            // 
            this.LabelComparisonTexts.AutoSize = true;
            this.LabelComparisonTexts.Location = new System.Drawing.Point(12, 41);
            this.LabelComparisonTexts.Name = "LabelComparisonTexts";
            this.LabelComparisonTexts.Size = new System.Drawing.Size(147, 15);
            this.LabelComparisonTexts.TabIndex = 2;
            this.LabelComparisonTexts.Text = "&2. Select Comparison Texts";
            // 
            // LabelBooks
            // 
            this.LabelBooks.AutoSize = true;
            this.LabelBooks.Location = new System.Drawing.Point(12, 184);
            this.LabelBooks.Name = "LabelBooks";
            this.LabelBooks.Size = new System.Drawing.Size(85, 15);
            this.LabelBooks.TabIndex = 5;
            this.LabelBooks.Text = "&3. Select Books";
            // 
            // LabelIncludeApparatusData
            // 
            this.LabelIncludeApparatusData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelIncludeApparatusData.AutoSize = true;
            this.LabelIncludeApparatusData.Location = new System.Drawing.Point(12, 331);
            this.LabelIncludeApparatusData.Name = "LabelIncludeApparatusData";
            this.LabelIncludeApparatusData.Size = new System.Drawing.Size(142, 15);
            this.LabelIncludeApparatusData.TabIndex = 8;
            this.LabelIncludeApparatusData.Text = "&4. Include Apparatus Data";
            // 
            // CheckBoxSelectAllBooks
            // 
            this.CheckBoxSelectAllBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckBoxSelectAllBooks.AutoSize = true;
            this.CheckBoxSelectAllBooks.Location = new System.Drawing.Point(384, 302);
            this.CheckBoxSelectAllBooks.Name = "CheckBoxSelectAllBooks";
            this.CheckBoxSelectAllBooks.Size = new System.Drawing.Size(74, 19);
            this.CheckBoxSelectAllBooks.TabIndex = 7;
            this.CheckBoxSelectAllBooks.Text = "Select A&ll";
            this.CheckBoxSelectAllBooks.UseVisualStyleBackColor = true;
            this.CheckBoxSelectAllBooks.CheckedChanged += new System.EventHandler(this.CheckBoxSelectAllBooks_CheckedChanged);
            // 
            // CheckBoxSelectAllComparisonTexts
            // 
            this.CheckBoxSelectAllComparisonTexts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CheckBoxSelectAllComparisonTexts.AutoSize = true;
            this.CheckBoxSelectAllComparisonTexts.Location = new System.Drawing.Point(384, 159);
            this.CheckBoxSelectAllComparisonTexts.Name = "CheckBoxSelectAllComparisonTexts";
            this.CheckBoxSelectAllComparisonTexts.Size = new System.Drawing.Size(74, 19);
            this.CheckBoxSelectAllComparisonTexts.TabIndex = 4;
            this.CheckBoxSelectAllComparisonTexts.Text = "Select &All";
            this.CheckBoxSelectAllComparisonTexts.UseVisualStyleBackColor = true;
            this.CheckBoxSelectAllComparisonTexts.CheckedChanged += new System.EventHandler(this.CheckBoxSelectAllComparisonTexts_CheckedChanged);
            // 
            // OpenFileDialogMain
            // 
            this.OpenFileDialogMain.DefaultExt = "*.csv";
            this.OpenFileDialogMain.Filter = "CSV Spreadsheet (*.csv)|*.csv|All Files (*.*)|*.*";
            this.OpenFileDialogMain.Multiselect = true;
            // 
            // ProgressBarMain
            // 
            this.ProgressBarMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBarMain.Location = new System.Drawing.Point(12, 381);
            this.ProgressBarMain.Name = "ProgressBarMain";
            this.ProgressBarMain.Size = new System.Drawing.Size(284, 23);
            this.ProgressBarMain.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBarMain.TabIndex = 15;
            this.ProgressBarMain.Visible = false;
            // 
            // FormApparatusGenerator
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(470, 416);
            this.Controls.Add(this.ProgressBarMain);
            this.Controls.Add(this.CheckBoxSelectAllComparisonTexts);
            this.Controls.Add(this.CheckBoxSelectAllBooks);
            this.Controls.Add(this.LabelIncludeApparatusData);
            this.Controls.Add(this.LabelBooks);
            this.Controls.Add(this.LabelComparisonTexts);
            this.Controls.Add(this.LabelBaseText);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.CheckedListBoxBooks);
            this.Controls.Add(this.RadioButtonHtml);
            this.Controls.Add(this.RadioButtonCsv);
            this.Controls.Add(this.ButtonBrowse);
            this.Controls.Add(this.TextBoxIncludeApparatusData);
            this.Controls.Add(this.CheckedListBoxComparisonTexts);
            this.Controls.Add(this.ComboBoxBaseText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(486, 367);
            this.Name = "FormApparatusGenerator";
            this.Text = "Apparatus Generator";
            this.Load += new System.EventHandler(this.FormApparatusGenerator_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ComboBoxBaseText;
        private System.Windows.Forms.CheckedListBox CheckedListBoxComparisonTexts;
        private System.Windows.Forms.TextBox TextBoxIncludeApparatusData;
        private System.Windows.Forms.Button ButtonBrowse;
        private System.Windows.Forms.RadioButton RadioButtonCsv;
        private System.Windows.Forms.RadioButton RadioButtonHtml;
        private System.Windows.Forms.CheckedListBox CheckedListBoxBooks;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label LabelBaseText;
        private System.Windows.Forms.Label LabelComparisonTexts;
        private System.Windows.Forms.Label LabelBooks;
        private System.Windows.Forms.Label LabelIncludeApparatusData;
        private System.Windows.Forms.CheckBox CheckBoxSelectAllBooks;
        private System.Windows.Forms.CheckBox CheckBoxSelectAllComparisonTexts;
        private System.Windows.Forms.ToolTip ToolTipItem;
        private System.Windows.Forms.OpenFileDialog OpenFileDialogMain;
        private System.Windows.Forms.ProgressBar ProgressBarMain;
    }
}