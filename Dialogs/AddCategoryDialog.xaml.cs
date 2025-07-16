using System.Windows;
using SizeChartGenerator.Models;

namespace SizeChartGenerator.Dialogs
{
    public partial class AddCategoryDialog : Window
    {
        public SizeCategory Category { get; private set; }

        public AddCategoryDialog()
        {
            InitializeComponent();
            Category = new SizeCategory();
        }

        public AddCategoryDialog(SizeCategory existingCategory) : this()
        {
            Category = existingCategory;
            CategoryNameTextBox.Text = existingCategory.Name;
            if (existingCategory.BaseIncrement == 4)
            {
                Increment4Radio.IsChecked = true;
            }
            else
            {
                Increment1Radio.IsChecked = true;
            }
            Title = "编辑自定义类别";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
            {
                MessageBox.Show("请输入类别名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Category.Name = CategoryNameTextBox.Text.Trim();
            Category.BaseIncrement = Increment4Radio.IsChecked == true ? 4 : 1;
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}