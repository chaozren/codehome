namespace mmc_production
{
    partial class pwdwindow
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
            this.pwd = new System.Windows.Forms.TextBox();
            this.cancel = new System.Windows.Forms.Button();
            this.confirm = new System.Windows.Forms.Button();
            this.prompt = new System.Windows.Forms.Label();
            this.verify = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pwd
            // 
            this.pwd.Location = new System.Drawing.Point(24, 33);
            this.pwd.Name = "pwd";
            this.pwd.PasswordChar = '*';
            this.pwd.Size = new System.Drawing.Size(178, 21);
            this.pwd.TabIndex = 0;
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(24, 70);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(61, 23);
            this.cancel.TabIndex = 1;
            this.cancel.Text = "取消";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // confirm
            // 
            this.confirm.Location = new System.Drawing.Point(141, 70);
            this.confirm.Name = "confirm";
            this.confirm.Size = new System.Drawing.Size(61, 23);
            this.confirm.TabIndex = 2;
            this.confirm.Text = "确定";
            this.confirm.UseVisualStyleBackColor = true;
            this.confirm.Click += new System.EventHandler(this.confirm_Click);
            // 
            // prompt
            // 
            this.prompt.AutoSize = true;
            this.prompt.ForeColor = System.Drawing.Color.Red;
            this.prompt.Location = new System.Drawing.Point(23, 9);
            this.prompt.Name = "prompt";
            this.prompt.Size = new System.Drawing.Size(125, 12);
            this.prompt.TabIndex = 3;
            this.prompt.Text = "密码错误，请重新输入";
            this.prompt.Visible = false;
            // 
            // verify
            // 
            this.verify.Location = new System.Drawing.Point(141, 70);
            this.verify.Name = "verify";
            this.verify.Size = new System.Drawing.Size(61, 23);
            this.verify.TabIndex = 4;
            this.verify.Text = "确定";
            this.verify.UseVisualStyleBackColor = true;
            this.verify.Click += new System.EventHandler(this.verify_Click);
            // 
            // pwdwindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 105);
            this.Controls.Add(this.verify);
            this.Controls.Add(this.prompt);
            this.Controls.Add(this.confirm);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.pwd);
            this.Name = "pwdwindow";
            this.Text = "请输入密码";
            this.Load += new System.EventHandler(this.PwdInput_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox pwd;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button confirm;
        private System.Windows.Forms.Label prompt;
        private System.Windows.Forms.Button verify;
    }
}