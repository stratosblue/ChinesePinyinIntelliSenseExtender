namespace ChinesePinyinIntelliSenseExtender.Options;

partial class DictionaryCombinationCreateForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DictionaryCombinationCreateForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBoxSelected = new System.Windows.Forms.GroupBox();
            this.listBoxSelected = new System.Windows.Forms.ListBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.buttonUnselected = new System.Windows.Forms.Button();
            this.groupBoxUnselected = new System.Windows.Forms.GroupBox();
            this.listBoxUnselected = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBoxSelected.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBoxUnselected.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonCreate);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 350);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(743, 100);
            this.panel1.TabIndex = 0;
            // 
            // buttonCreate
            // 
            this.buttonCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCreate.Location = new System.Drawing.Point(615, 43);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(75, 23);
            this.buttonCreate.TabIndex = 1;
            this.buttonCreate.Text = "创建";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.ButtonCreate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(377, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "插件会将已选字典按从上往下的顺序加载，将所有字典合并后进行使用";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBoxSelected);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.groupBoxUnselected);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(743, 350);
            this.panel2.TabIndex = 1;
            // 
            // groupBoxSelected
            // 
            this.groupBoxSelected.Controls.Add(this.listBoxSelected);
            this.groupBoxSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSelected.Location = new System.Drawing.Point(409, 0);
            this.groupBoxSelected.Name = "groupBoxSelected";
            this.groupBoxSelected.Size = new System.Drawing.Size(334, 350);
            this.groupBoxSelected.TabIndex = 5;
            this.groupBoxSelected.TabStop = false;
            this.groupBoxSelected.Text = "已选字典";
            // 
            // listBoxSelected
            // 
            this.listBoxSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxSelected.FormattingEnabled = true;
            this.listBoxSelected.ItemHeight = 12;
            this.listBoxSelected.Location = new System.Drawing.Point(3, 17);
            this.listBoxSelected.Name = "listBoxSelected";
            this.listBoxSelected.Size = new System.Drawing.Size(328, 330);
            this.listBoxSelected.TabIndex = 0;
            this.listBoxSelected.SelectedValueChanged += new System.EventHandler(this.ListBoxSelected_SelectedValueChanged);
            this.listBoxSelected.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBoxSelected_MouseDoubleClick);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonSelect);
            this.panel3.Controls.Add(this.buttonUnselected);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(297, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(112, 350);
            this.panel3.TabIndex = 4;
            // 
            // buttonSelect
            // 
            this.buttonSelect.Location = new System.Drawing.Point(22, 125);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(75, 23);
            this.buttonSelect.TabIndex = 1;
            this.buttonSelect.Text = "-->";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.ButtonSelect_Click);
            // 
            // buttonUnselected
            // 
            this.buttonUnselected.Location = new System.Drawing.Point(22, 162);
            this.buttonUnselected.Name = "buttonUnselected";
            this.buttonUnselected.Size = new System.Drawing.Size(75, 23);
            this.buttonUnselected.TabIndex = 1;
            this.buttonUnselected.Text = "<--";
            this.buttonUnselected.UseVisualStyleBackColor = true;
            this.buttonUnselected.Click += new System.EventHandler(this.ButtonUnselected_Click);
            // 
            // groupBoxUnselected
            // 
            this.groupBoxUnselected.Controls.Add(this.listBoxUnselected);
            this.groupBoxUnselected.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBoxUnselected.Location = new System.Drawing.Point(0, 0);
            this.groupBoxUnselected.Name = "groupBoxUnselected";
            this.groupBoxUnselected.Size = new System.Drawing.Size(297, 350);
            this.groupBoxUnselected.TabIndex = 2;
            this.groupBoxUnselected.TabStop = false;
            this.groupBoxUnselected.Text = "可选字典";
            // 
            // listBoxUnselected
            // 
            this.listBoxUnselected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxUnselected.FormattingEnabled = true;
            this.listBoxUnselected.ItemHeight = 12;
            this.listBoxUnselected.Location = new System.Drawing.Point(3, 17);
            this.listBoxUnselected.Name = "listBoxUnselected";
            this.listBoxUnselected.Size = new System.Drawing.Size(291, 330);
            this.listBoxUnselected.TabIndex = 0;
            this.listBoxUnselected.SelectedValueChanged += new System.EventHandler(this.ListBoxUnselected_SelectedValueChanged);
            this.listBoxUnselected.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBoxUnselected_MouseDoubleClick);
            // 
            // DictionaryCombinationCreateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 450);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DictionaryCombinationCreateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "创建字典应用组合";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.groupBoxSelected.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBoxUnselected.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button buttonCreate;
    private System.Windows.Forms.Button buttonSelect;
    private System.Windows.Forms.ListBox listBoxUnselected;
    private System.Windows.Forms.Button buttonUnselected;
    private System.Windows.Forms.GroupBox groupBoxUnselected;
    private System.Windows.Forms.GroupBox groupBoxSelected;
    private System.Windows.Forms.ListBox listBoxSelected;
    private System.Windows.Forms.Panel panel3;
}