using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SizeChartGenerator.Models
{
    public class SizeCategory : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private int _baseIncrement;
        private bool _isCustom;
        private bool _isSelected;
        private int _startValue;
        private int _sortOrder;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int BaseIncrement
        {
            get => _baseIncrement;
            set { _baseIncrement = value; OnPropertyChanged(); }
        }

        public bool IsCustom
        {
            get => _isCustom;
            set { _isCustom = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public int StartValue
        {
            get => _startValue;
            set { _startValue = value; OnPropertyChanged(); }
        }

        public int SortOrder
        {
            get => _sortOrder;
            set { _sortOrder = value; OnPropertyChanged(); }
        }

        public int GetCurrentIncrement(ClothingMode mode)
        {
            if (mode == ClothingMode.Sweater && BaseIncrement == 4)
                return 2;
            return BaseIncrement;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ClothingMode
    {
        Normal,
        Sweater
    }
}