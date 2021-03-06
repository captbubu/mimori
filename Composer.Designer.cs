﻿namespace mimori
{
    partial class Composer
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
            this.label1 = new System.Windows.Forms.Label();
            this.to_textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cc_textBox = new System.Windows.Forms.TextBox();
            this.subject_textBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.message_textBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.bcc_textBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "To";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // to_textBox
            // 
            this.to_textBox.Location = new System.Drawing.Point(57, 27);
            this.to_textBox.Name = "to_textBox";
            this.to_textBox.Size = new System.Drawing.Size(297, 20);
            this.to_textBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Cc";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // cc_textBox
            // 
            this.cc_textBox.Location = new System.Drawing.Point(57, 57);
            this.cc_textBox.Name = "cc_textBox";
            this.cc_textBox.Size = new System.Drawing.Size(297, 20);
            this.cc_textBox.TabIndex = 2;
            // 
            // subject_textBox
            // 
            this.subject_textBox.Location = new System.Drawing.Point(57, 113);
            this.subject_textBox.Name = "subject_textBox";
            this.subject_textBox.Size = new System.Drawing.Size(896, 20);
            this.subject_textBox.TabIndex = 4;
            this.subject_textBox.TextChanged += new System.EventHandler(this.subject_textBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Subject";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // message_textBox
            // 
            this.message_textBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.message_textBox.Location = new System.Drawing.Point(0, 148);
            this.message_textBox.Multiline = true;
            this.message_textBox.Name = "message_textBox";
            this.message_textBox.Size = new System.Drawing.Size(979, 483);
            this.message_textBox.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(453, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 23);
            this.button1.TabIndex = 5;
            this.button1.TabStop = false;
            this.button1.Text = "Send message";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // bcc_textBox
            // 
            this.bcc_textBox.Location = new System.Drawing.Point(57, 87);
            this.bcc_textBox.Name = "bcc_textBox";
            this.bcc_textBox.Size = new System.Drawing.Size(297, 20);
            this.bcc_textBox.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Bcc";
            // 
            // Composer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(979, 631);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.bcc_textBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.message_textBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.subject_textBox);
            this.Controls.Add(this.cc_textBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.to_textBox);
            this.Controls.Add(this.label1);
            this.Name = "Composer";
            this.Text = "Composer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox to_textBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox cc_textBox;
        private System.Windows.Forms.TextBox subject_textBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox message_textBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox bcc_textBox;
        private System.Windows.Forms.Label label4;
    }
}