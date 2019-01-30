namespace AppNetworkBlocker
{
    partial class Form1
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
            this.processes = new System.Windows.Forms.ComboBox();
            this.hotkey_key = new System.Windows.Forms.TextBox();
            this.hotkey_shift = new System.Windows.Forms.CheckBox();
            this.hotkey_ctrl = new System.Windows.Forms.CheckBox();
            this.hotkey_alt = new System.Windows.Forms.CheckBox();
            this.hotkey_group = new System.Windows.Forms.GroupBox();
            this.hotkey_register_status = new System.Windows.Forms.Label();
            this.hotkey_status_label = new System.Windows.Forms.Label();
            this.process_group = new System.Windows.Forms.GroupBox();
            this.refresh = new System.Windows.Forms.Button();
            this.blockStatus = new System.Windows.Forms.Label();
            this.hotkey_group.SuspendLayout();
            this.process_group.SuspendLayout();
            this.SuspendLayout();
            // 
            // processes
            // 
            this.processes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.processes.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.processes.FormattingEnabled = true;
            this.processes.Location = new System.Drawing.Point(9, 19);
            this.processes.Name = "processes";
            this.processes.Size = new System.Drawing.Size(247, 22);
            this.processes.TabIndex = 10;
            this.processes.SelectedIndexChanged += new System.EventHandler(this.processes_SelectedIndexChanged);
            // 
            // hotkey_key
            // 
            this.hotkey_key.Location = new System.Drawing.Point(16, 70);
            this.hotkey_key.Name = "hotkey_key";
            this.hotkey_key.Size = new System.Drawing.Size(215, 20);
            this.hotkey_key.TabIndex = 12;
            this.hotkey_key.KeyDown += new System.Windows.Forms.KeyEventHandler(this.hotkey_KeyDown);
            // 
            // hotkey_shift
            // 
            this.hotkey_shift.AutoSize = true;
            this.hotkey_shift.Location = new System.Drawing.Point(102, 43);
            this.hotkey_shift.Name = "hotkey_shift";
            this.hotkey_shift.Size = new System.Drawing.Size(57, 17);
            this.hotkey_shift.TabIndex = 13;
            this.hotkey_shift.Text = "SHIFT";
            this.hotkey_shift.UseVisualStyleBackColor = true;
            this.hotkey_shift.CheckedChanged += new System.EventHandler(this.hotkey_shift_checked);
            // 
            // hotkey_ctrl
            // 
            this.hotkey_ctrl.AutoSize = true;
            this.hotkey_ctrl.Location = new System.Drawing.Point(9, 43);
            this.hotkey_ctrl.Name = "hotkey_ctrl";
            this.hotkey_ctrl.Size = new System.Drawing.Size(54, 17);
            this.hotkey_ctrl.TabIndex = 14;
            this.hotkey_ctrl.Text = "CTRL";
            this.hotkey_ctrl.UseVisualStyleBackColor = true;
            this.hotkey_ctrl.CheckedChanged += new System.EventHandler(this.hotkey_ctrl_checked);
            // 
            // hotkey_alt
            // 
            this.hotkey_alt.AutoSize = true;
            this.hotkey_alt.Location = new System.Drawing.Point(201, 43);
            this.hotkey_alt.Name = "hotkey_alt";
            this.hotkey_alt.Size = new System.Drawing.Size(46, 17);
            this.hotkey_alt.TabIndex = 15;
            this.hotkey_alt.Text = "ALT";
            this.hotkey_alt.UseVisualStyleBackColor = true;
            this.hotkey_alt.CheckedChanged += new System.EventHandler(this.hotkey_alt_checked);
            // 
            // hotkey_group
            // 
            this.hotkey_group.Controls.Add(this.hotkey_register_status);
            this.hotkey_group.Controls.Add(this.hotkey_status_label);
            this.hotkey_group.Controls.Add(this.hotkey_key);
            this.hotkey_group.Controls.Add(this.hotkey_shift);
            this.hotkey_group.Controls.Add(this.hotkey_alt);
            this.hotkey_group.Controls.Add(this.hotkey_ctrl);
            this.hotkey_group.Location = new System.Drawing.Point(12, 97);
            this.hotkey_group.Name = "hotkey_group";
            this.hotkey_group.Size = new System.Drawing.Size(262, 102);
            this.hotkey_group.TabIndex = 17;
            this.hotkey_group.TabStop = false;
            this.hotkey_group.Text = "Hotkey";
            // 
            // hotkey_register_status
            // 
            this.hotkey_register_status.AutoSize = true;
            this.hotkey_register_status.ForeColor = System.Drawing.Color.Red;
            this.hotkey_register_status.Location = new System.Drawing.Point(43, 21);
            this.hotkey_register_status.Name = "hotkey_register_status";
            this.hotkey_register_status.Size = new System.Drawing.Size(73, 13);
            this.hotkey_register_status.TabIndex = 17;
            this.hotkey_register_status.Text = "Not registered";
            // 
            // hotkey_status_label
            // 
            this.hotkey_status_label.AutoSize = true;
            this.hotkey_status_label.Location = new System.Drawing.Point(6, 21);
            this.hotkey_status_label.Name = "hotkey_status_label";
            this.hotkey_status_label.Size = new System.Drawing.Size(40, 13);
            this.hotkey_status_label.TabIndex = 16;
            this.hotkey_status_label.Text = "Status:";
            // 
            // process_group
            // 
            this.process_group.Controls.Add(this.refresh);
            this.process_group.Controls.Add(this.processes);
            this.process_group.Location = new System.Drawing.Point(12, 7);
            this.process_group.Name = "process_group";
            this.process_group.Size = new System.Drawing.Size(262, 84);
            this.process_group.TabIndex = 18;
            this.process_group.TabStop = false;
            this.process_group.Text = "Process List";
            // 
            // refresh
            // 
            this.refresh.Location = new System.Drawing.Point(9, 47);
            this.refresh.Name = "refresh";
            this.refresh.Size = new System.Drawing.Size(247, 24);
            this.refresh.TabIndex = 11;
            this.refresh.Text = "Refresh";
            this.refresh.UseVisualStyleBackColor = true;
            this.refresh.Click += new System.EventHandler(this.refresh_click);
            // 
            // blockStatus
            // 
            this.blockStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.blockStatus.Location = new System.Drawing.Point(88, 212);
            this.blockStatus.Name = "blockStatus";
            this.blockStatus.Size = new System.Drawing.Size(100, 20);
            this.blockStatus.TabIndex = 20;
            this.blockStatus.Text = "Packets blocked";
            this.blockStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.blockStatus.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 241);
            this.Controls.Add(this.blockStatus);
            this.Controls.Add(this.process_group);
            this.Controls.Add(this.hotkey_group);
            this.Name = "Form1";
            this.Text = "Application Network Blocker";
            this.hotkey_group.ResumeLayout(false);
            this.hotkey_group.PerformLayout();
            this.process_group.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox processes;
        private System.Windows.Forms.TextBox hotkey_key;
        private System.Windows.Forms.CheckBox hotkey_shift;
        private System.Windows.Forms.CheckBox hotkey_ctrl;
        private System.Windows.Forms.CheckBox hotkey_alt;
        private System.Windows.Forms.GroupBox hotkey_group;
        private System.Windows.Forms.GroupBox process_group;
        private System.Windows.Forms.Label hotkey_register_status;
        private System.Windows.Forms.Label hotkey_status_label;
        private System.Windows.Forms.Button refresh;
        private System.Windows.Forms.Label blockStatus;
    }
}

