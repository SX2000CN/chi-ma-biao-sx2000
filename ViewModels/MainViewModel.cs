using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SizeChartGenerator.Commands;
using SizeChartGenerator.Models;
using SizeChartGenerator.Services;
using SizeChartGenerator.Dialogs;

namespace SizeChartGenerator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SizeCategory> _categories;
        private ClothingMode _currentMode;
        private string _selectedStartSize;
        private int _sizeCount;
        private SizeChart _currentChart;

        public MainViewModel()
        {
            InitializeCommands();
            LoadCategories();
            SelectedStartSize = "S";
            SizeCount = 4;
            CurrentMode = ClothingMode.Normal;
            UpdatePreview();
        }

        public ObservableCollection<SizeCategory> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public ClothingMode CurrentMode
        {
            get => _currentMode;
            set 
            { 
                _currentMode = value; 
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public string SelectedStartSize
        {
            get => _selectedStartSize;
            set 
            { 
                _selectedStartSize = value; 
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public int SizeCount
        {
            get => _sizeCount;
            set 
            { 
                _sizeCount = value; 
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public SizeChart CurrentChart
        {
            get => _currentChart;
            set { _currentChart = value; OnPropertyChanged(); }
        }

        public ICommand AddCategoryCommand { get; private set; }
        public ICommand EditCategoryCommand { get; private set; }
        public ICommand DeleteCategoryCommand { get; private set; }
        public ICommand SaveChartCommand { get; private set; }
        public ICommand CategorySelectionChangedCommand { get; private set; }
        public ICommand ManageCategoriesCommand { get; private set; }
        public ICommand SwitchModeCommand { get; private set; }

        private void InitializeCommands()
        {
            AddCategoryCommand = new RelayCommand(ExecuteAddCategory);
            EditCategoryCommand = new RelayCommand<SizeCategory>(ExecuteEditCategory);
            DeleteCategoryCommand = new RelayCommand<SizeCategory>(ExecuteDeleteCategory);
            SaveChartCommand = new RelayCommand(ExecuteSaveChart, CanExecuteSaveChart);
            CategorySelectionChangedCommand = new RelayCommand(ExecuteCategorySelectionChanged);
            ManageCategoriesCommand = new RelayCommand(ExecuteManageCategories);
            SwitchModeCommand = new RelayCommand<string>(ExecuteSwitchMode);
        }

        private void LoadCategories()
        {
            Categories = new ObservableCollection<SizeCategory>();
            
            // 加载预设类别
            Categories.Add(new SizeCategory { Name = "胸围", BaseIncrement = 4, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "腰围", BaseIncrement = 4, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "臀围", BaseIncrement = 4, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "下摆围", BaseIncrement = 4, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "肩宽", BaseIncrement = 1, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "袖长", BaseIncrement = 1, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "衣长", BaseIncrement = 1, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "裤长", BaseIncrement = 1, IsCustom = false });
            Categories.Add(new SizeCategory { Name = "中后长", BaseIncrement = 1, IsCustom = false });
            
            // 从数据库加载自定义类别
            var customCategories = DatabaseService.Instance.GetCustomCategories();
            foreach (var category in customCategories)
            {
                Categories.Add(category);
            }

            // 订阅属性变化事件
            foreach (var category in Categories)
            {
                category.PropertyChanged += Category_PropertyChanged;
            }
        }

        private void Category_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SizeCategory.StartValue))
            {
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            if (string.IsNullOrEmpty(SelectedStartSize) || SizeCount <= 0)
                return;

            var chart = new SizeChart();
            
            // 获取选中的类别（按选择顺序）
            var selectedCategories = Categories
                .Where(c => c.IsSelected)
                .OrderBy(c => c.SortOrder)
                .ToList();

            if (!selectedCategories.Any())
                return;

            // 设置尺码范围
            chart.Sizes = SizeDefinitions.GetSizeRange(SelectedStartSize, SizeCount);
            
            // 计算每个类别的值
            foreach (var category in selectedCategories)
            {
                chart.Categories.Add(category);
                chart.Values[category.Name] = new Dictionary<string, int>();
                
                int increment = category.GetCurrentIncrement(CurrentMode);
                int currentValue = category.StartValue;
                
                foreach (var size in chart.Sizes)
                {
                    chart.Values[category.Name][size] = currentValue;
                    currentValue += increment;
                }
            }
            
            CurrentChart = chart;
        }

        private void ExecuteAddCategory(object parameter)
        {
            var dialog = new AddCategoryDialog();
            if (dialog.ShowDialog() == true)
            {
                var newCategory = dialog.Category;
                newCategory.IsCustom = true;
                Categories.Add(newCategory);
                DatabaseService.Instance.SaveCategory(newCategory);
                newCategory.PropertyChanged += Category_PropertyChanged;
            }
        }

        private void ExecuteEditCategory(SizeCategory category)
        {
            if (category == null || !category.IsCustom)
                return;

            var dialog = new AddCategoryDialog(category);
            if (dialog.ShowDialog() == true)
            {
                DatabaseService.Instance.UpdateCategory(category);
                UpdatePreview();
            }
        }

        private void ExecuteDeleteCategory(SizeCategory category)
        {
            if (category == null || !category.IsCustom)
                return;

            Categories.Remove(category);
            DatabaseService.Instance.DeleteCategory(category.Id);
            category.PropertyChanged -= Category_PropertyChanged;
            UpdatePreview();
        }

        private void ExecuteSaveChart(object parameter)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JPEG图片|*.jpg",
                DefaultExt = ".jpg",
                FileName = $"尺码表_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                ChartExportService.ExportToJpg(CurrentChart, dialog.FileName);
                System.Windows.MessageBox.Show("尺码表已保存成功！", "保存成功", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
            }
        }

        private bool CanExecuteSaveChart(object parameter)
        {
            return CurrentChart?.Categories?.Count > 0;
        }

        private void ExecuteCategorySelectionChanged(object parameter)
        {
            // 更新选中类别的排序顺序
            int sortOrder = 0;
            foreach (var category in Categories.Where(c => c.IsSelected))
            {
                category.SortOrder = sortOrder++;
            }
            UpdatePreview();
        }

        private void ExecuteManageCategories(object parameter)
        {
            var dialog = new CategoryManagementDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadCategories(); // 重新加载类别
            }
        }

        private void ExecuteSwitchMode(string mode)
        {
            CurrentMode = mode == "Sweater" ? ClothingMode.Sweater : ClothingMode.Normal;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}