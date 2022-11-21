namespace Lead.Process.Manager
{
    partial class ProcessStateManagerUI
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.名称 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.当前步骤 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.未初始化 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.初始化 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.运行 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.暂停 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.终止 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.名称,
            this.ID,
            this.当前步骤,
            this.未初始化,
            this.初始化,
            this.运行,
            this.暂停,
            this.终止});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(902, 425);
            this.dataGridView1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // 名称
            // 
            this.名称.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.名称.HeaderText = "名称";
            this.名称.Name = "名称";
            this.名称.Width = 66;
            // 
            // ID
            // 
            this.ID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.Width = 52;
            // 
            // 当前步骤
            // 
            this.当前步骤.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.当前步骤.HeaderText = "当前步骤";
            this.当前步骤.Name = "当前步骤";
            // 
            // 未初始化
            // 
            this.未初始化.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.未初始化.HeaderText = "未初始化";
            this.未初始化.Name = "未初始化";
            this.未初始化.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // 初始化
            // 
            this.初始化.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.初始化.HeaderText = "初始化";
            this.初始化.Name = "初始化";
            // 
            // 运行
            // 
            this.运行.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.运行.HeaderText = "运行";
            this.运行.Name = "运行";
            // 
            // 暂停
            // 
            this.暂停.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.暂停.HeaderText = "暂停";
            this.暂停.Name = "暂停";
            // 
            // 终止
            // 
            this.终止.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.终止.HeaderText = "终止";
            this.终止.Name = "终止";
            // 
            // ProcessStateManagerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Name = "ProcessStateManagerUI";
            this.Size = new System.Drawing.Size(902, 425);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 名称;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn 当前步骤;
        private System.Windows.Forms.DataGridViewTextBoxColumn 未初始化;
        private System.Windows.Forms.DataGridViewTextBoxColumn 初始化;
        private System.Windows.Forms.DataGridViewTextBoxColumn 运行;
        private System.Windows.Forms.DataGridViewTextBoxColumn 暂停;
        private System.Windows.Forms.DataGridViewTextBoxColumn 终止;
    }
}
