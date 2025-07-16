using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SizeChartGenerator.Models;
using SizeChartGenerator.Services;

namespace SizeChartGenerator.Dialogs
{
    public partial class CategoryManagementDialog : Window
    {
        private ObservableCollection<SizeCategory> _customCategories;

        public CategoryManagementDialog()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            _customCategories = new ObservableCollection<SizeCategory>(
                DatabaseService.Instance.GetCustomCategories());
            CategoriesList.ItemsSource = _customCategories;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddCategoryDialog();
            if (dialog.ShowDialog() == true)
            {
                var newCategory = dialog.Category;
                newCategory.IsCustom = true;
                DatabaseService.Instance.SaveCategory(newCategory);
                _customCategories.Add(newCategory);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var category = button?.Tag as SizeCategory;
            
            if (category != null)
            {
                var dialog = new AddCategoryDialog(category);
                if (dialog.ShowDialog() == true)
                {
                    DatabaseService.Instance.UpdateCategory(category);
                    LoadCategories(); // 刷新列表
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var category = button?.Tag as SizeCategory;
            
            if (category != null)
            {
                var result = MessageBox.Show($"确定要删除类别 \"{category.Name}\" 吗？", 
                    "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    DatabaseService.Instance.DeleteCategory(category.Id);
                    _customCategories.Remove(category);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}