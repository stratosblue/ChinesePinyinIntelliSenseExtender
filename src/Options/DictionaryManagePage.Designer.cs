namespace ChinesePinyinIntelliSenseExtender.Options;

partial class DictionaryManagePage
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.groupBoxDictionaryManage = new System.Windows.Forms.GroupBox();
            this.panelDictionaryView = new System.Windows.Forms.Panel();
            this.listBoxDictionaries = new System.Windows.Forms.ListBox();
            this.contextMenuStripDictionaries = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemDeleteSelectedDictionary = new System.Windows.Forms.ToolStripMenuItem();
            this.panelDictionaryManage = new System.Windows.Forms.Panel();
            this.textBoxNewDictionaryPath = new System.Windows.Forms.TextBox();
            this.textBoxNewDictionaryName = new System.Windows.Forms.TextBox();
            this.buttonAddDictionary = new System.Windows.Forms.Button();
            this.labelNewDictionaryPath = new System.Windows.Forms.Label();
            this.labelNewDictionaryName = new System.Windows.Forms.Label();
            this.groupBoxDictionaryApply = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.treeViewDictionaryCombination = new System.Windows.Forms.TreeView();
            this.contextMenuStripDictionaryCombination = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemDeleteSelectedDictionaryCombination = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonResetDictionaryApplyCombination = new System.Windows.Forms.Button();
            this.buttonAddDictionaryApplyCombination = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.groupBoxDictionaryManage.SuspendLayout();
            this.panelDictionaryView.SuspendLayout();
            this.contextMenuStripDictionaries.SuspendLayout();
            this.panelDictionaryManage.SuspendLayout();
            this.groupBoxDictionaryApply.SuspendLayout();
            this.panel2.SuspendLayout();
            this.contextMenuStripDictionaryCombination.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.groupBoxDictionaryManage);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.groupBoxDictionaryApply);
            this.splitContainer.Size = new System.Drawing.Size(801, 481);
            this.splitContainer.SplitterDistance = 352;
            this.splitContainer.TabIndex = 0;
            // 
            // groupBoxDictionaryManage
            // 
            this.groupBoxDictionaryManage.Controls.Add(this.panelDictionaryView);
            this.groupBoxDictionaryManage.Controls.Add(this.panelDictionaryManage);
            this.groupBoxDictionaryManage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDictionaryManage.Location = new System.Drawing.Point(0, 0);
            this.groupBoxDictionaryManage.Name = "groupBoxDictionaryManage";
            this.groupBoxDictionaryManage.Size = new System.Drawing.Size(352, 481);
            this.groupBoxDictionaryManage.TabIndex = 0;
            this.groupBoxDictionaryManage.TabStop = false;
            this.groupBoxDictionaryManage.Text = "字典管理";
            // 
            // panelDictionaryView
            // 
            this.panelDictionaryView.Controls.Add(this.listBoxDictionaries);
            this.panelDictionaryView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDictionaryView.Location = new System.Drawing.Point(3, 17);
            this.panelDictionaryView.Name = "panelDictionaryView";
            this.panelDictionaryView.Size = new System.Drawing.Size(346, 361);
            this.panelDictionaryView.TabIndex = 1;
            // 
            // listBoxDictionaries
            // 
            this.listBoxDictionaries.ContextMenuStrip = this.contextMenuStripDictionaries;
            this.listBoxDictionaries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDictionaries.FormattingEnabled = true;
            this.listBoxDictionaries.ItemHeight = 12;
            this.listBoxDictionaries.Location = new System.Drawing.Point(0, 0);
            this.listBoxDictionaries.Name = "listBoxDictionaries";
            this.listBoxDictionaries.Size = new System.Drawing.Size(346, 361);
            this.listBoxDictionaries.TabIndex = 0;
            this.listBoxDictionaries.SelectedValueChanged += new System.EventHandler(this.ListBoxDictionaries_SelectedValueChanged);
            // 
            // contextMenuStripDictionaries
            // 
            this.contextMenuStripDictionaries.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemDeleteSelectedDictionary});
            this.contextMenuStripDictionaries.Name = "contextMenuStripDictionaries";
            this.contextMenuStripDictionaries.Size = new System.Drawing.Size(149, 26);
            // 
            // toolStripMenuItemDeleteSelectedDictionary
            // 
            this.toolStripMenuItemDeleteSelectedDictionary.Name = "toolStripMenuItemDeleteSelectedDictionary";
            this.toolStripMenuItemDeleteSelectedDictionary.Size = new System.Drawing.Size(148, 22);
            this.toolStripMenuItemDeleteSelectedDictionary.Text = "删除选中字典";
            this.toolStripMenuItemDeleteSelectedDictionary.Click += new System.EventHandler(this.ToolStripMenuItemDeleteSelectedDictionary_Click);
            // 
            // panelDictionaryManage
            // 
            this.panelDictionaryManage.Controls.Add(this.textBoxNewDictionaryPath);
            this.panelDictionaryManage.Controls.Add(this.textBoxNewDictionaryName);
            this.panelDictionaryManage.Controls.Add(this.buttonAddDictionary);
            this.panelDictionaryManage.Controls.Add(this.labelNewDictionaryPath);
            this.panelDictionaryManage.Controls.Add(this.labelNewDictionaryName);
            this.panelDictionaryManage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelDictionaryManage.Location = new System.Drawing.Point(3, 378);
            this.panelDictionaryManage.Name = "panelDictionaryManage";
            this.panelDictionaryManage.Size = new System.Drawing.Size(346, 100);
            this.panelDictionaryManage.TabIndex = 0;
            // 
            // textBoxNewDictionaryPath
            // 
            this.textBoxNewDictionaryPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNewDictionaryPath.Location = new System.Drawing.Point(63, 33);
            this.textBoxNewDictionaryPath.Name = "textBoxNewDictionaryPath";
            this.textBoxNewDictionaryPath.Size = new System.Drawing.Size(280, 21);
            this.textBoxNewDictionaryPath.TabIndex = 2;
            // 
            // textBoxNewDictionaryName
            // 
            this.textBoxNewDictionaryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNewDictionaryName.Location = new System.Drawing.Point(63, 6);
            this.textBoxNewDictionaryName.Name = "textBoxNewDictionaryName";
            this.textBoxNewDictionaryName.Size = new System.Drawing.Size(280, 21);
            this.textBoxNewDictionaryName.TabIndex = 2;
            // 
            // buttonAddDictionary
            // 
            this.buttonAddDictionary.Location = new System.Drawing.Point(41, 60);
            this.buttonAddDictionary.Name = "buttonAddDictionary";
            this.buttonAddDictionary.Size = new System.Drawing.Size(75, 23);
            this.buttonAddDictionary.TabIndex = 1;
            this.buttonAddDictionary.Text = "添加字典";
            this.buttonAddDictionary.UseVisualStyleBackColor = true;
            this.buttonAddDictionary.Click += new System.EventHandler(this.ButtonAddDictionary_Click);
            // 
            // labelNewDictionaryPath
            // 
            this.labelNewDictionaryPath.AutoSize = true;
            this.labelNewDictionaryPath.Location = new System.Drawing.Point(4, 37);
            this.labelNewDictionaryPath.Name = "labelNewDictionaryPath";
            this.labelNewDictionaryPath.Size = new System.Drawing.Size(65, 12);
            this.labelNewDictionaryPath.TabIndex = 0;
            this.labelNewDictionaryPath.Text = "文件路径：";
            // 
            // labelNewDictionaryName
            // 
            this.labelNewDictionaryName.AutoSize = true;
            this.labelNewDictionaryName.Location = new System.Drawing.Point(16, 10);
            this.labelNewDictionaryName.Name = "labelNewDictionaryName";
            this.labelNewDictionaryName.Size = new System.Drawing.Size(41, 12);
            this.labelNewDictionaryName.TabIndex = 0;
            this.labelNewDictionaryName.Text = "名称：";
            // 
            // groupBoxDictionaryApply
            // 
            this.groupBoxDictionaryApply.Controls.Add(this.panel2);
            this.groupBoxDictionaryApply.Controls.Add(this.panel1);
            this.groupBoxDictionaryApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDictionaryApply.Location = new System.Drawing.Point(0, 0);
            this.groupBoxDictionaryApply.Name = "groupBoxDictionaryApply";
            this.groupBoxDictionaryApply.Size = new System.Drawing.Size(445, 481);
            this.groupBoxDictionaryApply.TabIndex = 0;
            this.groupBoxDictionaryApply.TabStop = false;
            this.groupBoxDictionaryApply.Text = "字典应用";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.treeViewDictionaryCombination);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 17);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(439, 361);
            this.panel2.TabIndex = 1;
            // 
            // treeViewDictionaryCombination
            // 
            this.treeViewDictionaryCombination.ContextMenuStrip = this.contextMenuStripDictionaryCombination;
            this.treeViewDictionaryCombination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewDictionaryCombination.Location = new System.Drawing.Point(0, 0);
            this.treeViewDictionaryCombination.Name = "treeViewDictionaryCombination";
            this.treeViewDictionaryCombination.Size = new System.Drawing.Size(439, 361);
            this.treeViewDictionaryCombination.TabIndex = 0;
            this.treeViewDictionaryCombination.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewDictionaryCombination_AfterSelect);
            // 
            // contextMenuStripDictionaryCombination
            // 
            this.contextMenuStripDictionaryCombination.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemDeleteSelectedDictionaryCombination});
            this.contextMenuStripDictionaryCombination.Name = "contextMenuStripDictionaryCombination";
            this.contextMenuStripDictionaryCombination.Size = new System.Drawing.Size(149, 26);
            // 
            // toolStripMenuItemDeleteSelectedDictionaryCombination
            // 
            this.toolStripMenuItemDeleteSelectedDictionaryCombination.Name = "toolStripMenuItemDeleteSelectedDictionaryCombination";
            this.toolStripMenuItemDeleteSelectedDictionaryCombination.Size = new System.Drawing.Size(148, 22);
            this.toolStripMenuItemDeleteSelectedDictionaryCombination.Text = "删除选中组合";
            this.toolStripMenuItemDeleteSelectedDictionaryCombination.Click += new System.EventHandler(this.ToolStripMenuItemDeleteSelectedDictionaryCombination_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonResetDictionaryApplyCombination);
            this.panel1.Controls.Add(this.buttonAddDictionaryApplyCombination);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 378);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(439, 100);
            this.panel1.TabIndex = 0;
            // 
            // buttonResetDictionaryApplyCombination
            // 
            this.buttonResetDictionaryApplyCombination.Location = new System.Drawing.Point(19, 60);
            this.buttonResetDictionaryApplyCombination.Name = "buttonResetDictionaryApplyCombination";
            this.buttonResetDictionaryApplyCombination.Size = new System.Drawing.Size(113, 23);
            this.buttonResetDictionaryApplyCombination.TabIndex = 1;
            this.buttonResetDictionaryApplyCombination.Text = "重置为默认";
            this.buttonResetDictionaryApplyCombination.UseVisualStyleBackColor = true;
            this.buttonResetDictionaryApplyCombination.Click += new System.EventHandler(this.ButtonResetDictionaryApplyCombination_Click);
            // 
            // buttonAddDictionaryApplyCombination
            // 
            this.buttonAddDictionaryApplyCombination.Location = new System.Drawing.Point(19, 19);
            this.buttonAddDictionaryApplyCombination.Name = "buttonAddDictionaryApplyCombination";
            this.buttonAddDictionaryApplyCombination.Size = new System.Drawing.Size(113, 23);
            this.buttonAddDictionaryApplyCombination.TabIndex = 1;
            this.buttonAddDictionaryApplyCombination.Text = "创建字典应用组合";
            this.buttonAddDictionaryApplyCombination.UseVisualStyleBackColor = true;
            this.buttonAddDictionaryApplyCombination.Click += new System.EventHandler(this.ButtonAddDictionaryApplyCombination_Click);
            // 
            // DictionaryManagePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "DictionaryManagePage";
            this.Size = new System.Drawing.Size(801, 481);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.groupBoxDictionaryManage.ResumeLayout(false);
            this.panelDictionaryView.ResumeLayout(false);
            this.contextMenuStripDictionaries.ResumeLayout(false);
            this.panelDictionaryManage.ResumeLayout(false);
            this.panelDictionaryManage.PerformLayout();
            this.groupBoxDictionaryApply.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.contextMenuStripDictionaryCombination.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer;
    private System.Windows.Forms.GroupBox groupBoxDictionaryManage;
    private System.Windows.Forms.GroupBox groupBoxDictionaryApply;
    private System.Windows.Forms.Panel panelDictionaryView;
    private System.Windows.Forms.Panel panelDictionaryManage;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.ListBox listBoxDictionaries;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripDictionaries;
    private System.Windows.Forms.Label labelNewDictionaryPath;
    private System.Windows.Forms.Label labelNewDictionaryName;
    private System.Windows.Forms.Button buttonAddDictionary;
    private System.Windows.Forms.TextBox textBoxNewDictionaryName;
    private System.Windows.Forms.TextBox textBoxNewDictionaryPath;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteSelectedDictionary;
    private System.Windows.Forms.TreeView treeViewDictionaryCombination;
    private System.Windows.Forms.Button buttonAddDictionaryApplyCombination;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripDictionaryCombination;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteSelectedDictionaryCombination;
    private System.Windows.Forms.Button buttonResetDictionaryApplyCombination;
}
