using System.Windows.Forms;

namespace ChinesePinyinIntelliSenseExtender.Options;

internal partial class DictionaryManagePage : UserControl
{
    internal DictionaryManageOptions Options { get; set; }

    public DictionaryManagePage()
    {
        InitializeComponent();

        toolStripMenuItemDeleteSelectedDictionary.Enabled = false;
        toolStripMenuItemDeleteSelectedDictionaryCombination.Enabled = false;
    }

    public void Initialize()
    {
        listBoxDictionaries.Items.AddRange(DictionaryDescriptor.BuiltInDictionaries.Concat(Options.CustomDictionaries).ToArray());
        LoadDictionaryCombinationTree();
    }

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
    }

    private void ButtonAddDictionary_Click(object sender, EventArgs e)
    {
        var name = textBoxNewDictionaryName.Text;
        var path = textBoxNewDictionaryPath.Text?.Trim('\"');

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "请输入有效的字典名称", "无效输入", MessageBoxButtons.OK);
            return;
        }
        if (string.IsNullOrWhiteSpace(path)
            || !File.Exists(path))
        {
            MessageBox.Show(this, "请输入有效的字典路径", "无效输入", MessageBoxButtons.OK);
            return;
        }

        var newItem = new DictionaryDescriptor(path, name);

        if (Options.CustomDictionaries.Contains(newItem)
            || DictionaryDescriptor.BuiltInDictionaries.Contains(newItem))
        {
            MessageBox.Show(this, "不要重复添加字典", "无效输入", MessageBoxButtons.OK);
            return;
        }

        Options.CustomDictionaries.Add(newItem);
        listBoxDictionaries.Items.Add(newItem);
    }

    private void ButtonAddDictionaryApplyCombination_Click(object sender, EventArgs e)
    {
        using var form = new DictionaryCombinationCreateForm(DictionaryDescriptor.BuiltInDictionaries.Concat(Options.CustomDictionaries).ToArray());

        var dialogResult = form.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
            var newCombination = new DictionaryCombination(string.Join("、", form.SelectedDictionaryDescriptors.Select(m => m.Name)))
            {
                OrderedDictionaries = new(form.SelectedDictionaryDescriptors)
            };

            Options.DictionaryCombinations.Add(newCombination);
            LoadDictionaryCombinationTree();
        }
    }

    private void ButtonResetDictionaryApplyCombination_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("确定重置字典组合为默认值吗？", ChinesePinyinIntelliSenseExtenderPackage.PackageName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            Options.DictionaryCombinations.Clear();
            LoadDictionaryCombinationTree();
        }
    }

    private bool DeleteableDictionary(object sender)
    {
        return sender is ListBox listBox
               && listBox.SelectedItem is DictionaryDescriptor dictionaryDescriptor
               && !DictionaryDescriptor.BuiltInDictionaries.Contains(dictionaryDescriptor);
    }

    private void ListBoxDictionaries_SelectedValueChanged(object sender, EventArgs e)
    {
        toolStripMenuItemDeleteSelectedDictionary.Enabled = DeleteableDictionary(sender);
    }

    private void LoadDictionaryCombinationTree()
    {
        treeViewDictionaryCombination.Nodes.Clear();

        foreach (var item in Options.DictionaryCombinations)
        {
            var node = new TreeNode(item.Name)
            {
                Tag = item,
            };

            foreach (var dictionary in item.OrderedDictionaries)
            {
                node.Nodes.Add(new TreeNode(dictionary.ToString()) { Tag = dictionary });
            }

            treeViewDictionaryCombination.Nodes.Add(node);
        }

        treeViewDictionaryCombination.ExpandAll();
    }

    private void ToolStripMenuItemDeleteSelectedDictionary_Click(object sender, EventArgs e)
    {
        if (!DeleteableDictionary(listBoxDictionaries)
            || listBoxDictionaries.SelectedItem is not DictionaryDescriptor dictionaryDescriptor)
        {
            return;
        }

        listBoxDictionaries.Items.Remove(dictionaryDescriptor);
        Options.CustomDictionaries.Remove(dictionaryDescriptor);
    }

    private void ToolStripMenuItemDeleteSelectedDictionaryCombination_Click(object sender, EventArgs e)
    {
        if (treeViewDictionaryCombination.SelectedNode is not TreeNode treeNode
            || (treeNode.Parent?.Tag ?? treeNode.Tag) is not DictionaryCombination dictionaryCombination)
        {
            return;
        }

        treeViewDictionaryCombination.Nodes.Remove(treeNode.Parent ?? treeNode);
        TryReloadDictionaryCombinationTree(Options.DictionaryCombinations.Count <= 1 & Options.DictionaryCombinations.Remove(dictionaryCombination));
    }

    private void TreeViewDictionaryCombination_AfterSelect(object sender, TreeViewEventArgs e)
    {
        toolStripMenuItemDeleteSelectedDictionaryCombination.Enabled = treeViewDictionaryCombination.SelectedNode is not null;
        TryReloadDictionaryCombinationTree(false);
    }

    private void TryReloadDictionaryCombinationTree(bool force)
    {
        if (force
            || Options.DictionaryCombinations.Count == 0)
        {
            LoadDictionaryCombinationTree();
        }
    }
}
