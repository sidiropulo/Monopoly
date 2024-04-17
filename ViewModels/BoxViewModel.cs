using MVVM_Mon.Models;
using System.Xml.Linq;

namespace MVVM_Mon.ViewModels
{
	public class BoxViewModel
	{
        public string Name { get; set; }
        public string ProductionDate { get; set; }
        public string ExpiryDate { get; set; }

        public double Volume { get; set; }
        internal BoxViewModel(Box box)
        {
            Name = box.Id;
            ProductionDate = box.ProductionDate.ToString("dd.MM.yyyy");
            ExpiryDate = box.ExpiryDate.ToString("dd.MM.yyyy");
            Volume = box.Width * box.Height * box.Depth;
        }
    }

	 
}
