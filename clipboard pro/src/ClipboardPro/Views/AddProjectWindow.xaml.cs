using System.Windows;

namespace ClipboardPro.Views;

public partial class AddProjectWindow : Window
{
    public string ProjectName => ProjectNameBox.Text;

    public AddProjectWindow()
    {
        InitializeComponent();
        ProjectNameBox.Focus();
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProjectNameBox.Text))
        {
            MessageBox.Show("Please enter a project name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
