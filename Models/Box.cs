using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM_Mon.Models
{
	internal class Box : StockItem
	{
		public DateOnly ProductionDate { get; set; }
		public DateOnly ExpiryDate { get; set; }

		public Box(string id, double width, double height, double depth, double weight, DateOnly productionDate)
		{
			Id = id;
			Width = width;
			Height = height;
			Depth = depth;
			Weight = weight;
			ProductionDate = productionDate;
            // У коробки должен быть указан срок годности или дата производства.
			// Если указана дата производства, то срок годности вычисляется из даты производства плюс 100 дней.
            ExpiryDate = productionDate.AddDays(100); }


		// Вычисляем объем коробки
		public double CalculateVolume()
		{
			return Width * Height * Depth;
		}
	}
}
