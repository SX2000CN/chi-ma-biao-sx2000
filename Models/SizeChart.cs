using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SizeChartGenerator.Models
{
    public class SizeChart
    {
        public ObservableCollection<SizeCategory> Categories { get; set; }
        public List<string> Sizes { get; set; }
        public Dictionary<string, Dictionary<string, int>> Values { get; set; }

        public SizeChart()
        {
            Categories = new ObservableCollection<SizeCategory>();
            Sizes = new List<string>();
            Values = new Dictionary<string, Dictionary<string, int>>();
        }
    }

    public static class SizeDefinitions
    {
        public static readonly List<string> AllSizes = new List<string>
        {
            "均码", "XS", "S", "M", "L", "XL", "2XL", "3XL", "4XL"
        };

        public static List<string> GetSizeRange(string startSize, int count)
        {
            var result = new List<string>();
            int startIndex = AllSizes.IndexOf(startSize);
            
            if (startIndex == -1) return result;
            
            for (int i = 0; i < count && startIndex + i < AllSizes.Count; i++)
            {
                result.Add(AllSizes[startIndex + i]);
            }
            
            return result;
        }
    }
}