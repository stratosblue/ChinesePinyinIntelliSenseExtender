using System.Windows.Forms;

namespace ChinesePinyinIntelliSenseExtender.Options;

internal partial class DictionaryCombinationCreateForm : Form
{
    public DictionaryDescriptor[] DictionaryDescriptors { get; }

    public List<DictionaryDescriptor> SelectedDictionaryDescriptors { get; private set; } = new();

    public DictionaryCombinationCreateForm(DictionaryDescriptor[] dictionaryDescriptors)
    {
        InitializeComponent();
        DictionaryDescriptors = dictionaryDescriptors;

        buttonSelect.Enabled = false;
        buttonUnselected.Enabled = false;

        listBoxUnselected.Items.AddRange(dictionaryDescriptors);
    }

    private void ButtonCreate_Click(object sender, EventArgs e)
    {
        SelectedDictionaryDescriptors = listBoxSelected.Items.Cast<DictionaryDescriptor>().ToList();
        if (SelectedDictionaryDescriptors.Count == 0)
        {
            MessageBox.Show(this, "请正确选择字典", "无效输入", MessageBoxButtons.OK);
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ButtonSelect_Click(object sender, EventArgs e)
    {
        if (listBoxUnselected.SelectedItem is not null)
        {
            var item = listBoxUnselected.SelectedItem;
            listBoxUnselected.Items.Remove(item);
            listBoxSelected.Items.Add(item);
        }
    }

    private void ButtonUnselected_Click(object sender, EventArgs e)
    {
        if (listBoxSelected.SelectedItem is not null)
        {
            var item = listBoxSelected.SelectedItem;
            listBoxSelected.Items.Remove(item);
            listBoxUnselected.Items.Add(item);
        }
    }

    private void ListBoxSelected_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        ButtonUnselected_Click(sender, e);
    }

    private void ListBoxSelected_SelectedValueChanged(object sender, EventArgs e)
    {
        buttonUnselected.Enabled = listBoxSelected.SelectedItem is not null;
    }

    private void ListBoxUnselected_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        ButtonSelect_Click(sender, e);
    }

    private void ListBoxUnselected_SelectedValueChanged(object sender, EventArgs e)
    {
        buttonSelect.Enabled = listBoxUnselected.SelectedItem is not null;
    }
}
