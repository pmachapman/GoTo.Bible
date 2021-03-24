// Copyright 2020-2021 Conglomo Limited. Please see LICENSE for license details.

namespace GoToBible.Windows
{

    partial class FormApiKey
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
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.LabelEnterKey = new System.Windows.Forms.Label();
            this.TextBoxKey = new System.Windows.Forms.TextBox();
            this.LinkLabelSignup = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.Location = new System.Drawing.Point(362, 61);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "&Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.Location = new System.Drawing.Point(281, 61);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
            this.ButtonOk.TabIndex = 1;
            this.ButtonOk.Text = "&Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // LabelEnterKey
            // 
            this.LabelEnterKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelEnterKey.AutoSize = true;
            this.LabelEnterKey.Location = new System.Drawing.Point(12, 9);
            this.LabelEnterKey.Name = "LabelEnterKey";
            this.LabelEnterKey.Size = new System.Drawing.Size(363, 15);
            this.LabelEnterKey.TabIndex = 4;
            this.LabelEnterKey.Text = "Enter your API key below to use your API resources with GoToBible.";
            // 
            // TextBoxKey
            // 
            this.TextBoxKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxKey.Location = new System.Drawing.Point(12, 27);
            this.TextBoxKey.Name = "TextBoxKey";
            this.TextBoxKey.Size = new System.Drawing.Size(425, 23);
            this.TextBoxKey.TabIndex = 0;
            // 
            // LinkLabelSignup
            // 
            this.LinkLabelSignup.AutoSize = true;
            this.LinkLabelSignup.Location = new System.Drawing.Point(12, 65);
            this.LinkLabelSignup.Name = "LinkLabelSignup";
            this.LinkLabelSignup.Size = new System.Drawing.Size(123, 15);
            this.LinkLabelSignup.TabIndex = 3;
            this.LinkLabelSignup.TabStop = true;
            this.LinkLabelSignup.Text = "&Sign up for an API key";
            this.LinkLabelSignup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelSignup_LinkClicked);
            // 
            // FormApiKey
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(449, 96);
            this.Controls.Add(this.LinkLabelSignup);
            this.Controls.Add(this.TextBoxKey);
            this.Controls.Add(this.LabelEnterKey);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormApiKey";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter API Key";
            this.Load += new System.EventHandler(this.FormBibleApi_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Label LabelEnterKey;
        private System.Windows.Forms.TextBox TextBoxKey;
        private System.Windows.Forms.LinkLabel LinkLabelSignup;
    }
}