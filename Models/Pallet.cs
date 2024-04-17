using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM_Mon.Models
{
	internal class Pallet : StockItem
	{
        //- Помимо общего набора стандартных свойств(ID, ширина, высота, глубина, вес),
        //паллета может содержать в себе коробки
        internal List<Box> boxes = new List<Box>();

		private static Dictionary<string, Pallet> existingPallets = new Dictionary<string, Pallet>();

		public Pallet(string id, double width, double height, double depth)
		{
			if (existingPallets.ContainsKey(id))
			{
				throw new ArgumentException();
			}

			existingPallets.Add(id, this);

			Id = id;
			Width = width;
			Height = height;
			Depth = depth;
            //Вес паллеты вычисляется из суммы веса вложенных коробок + 30кг.
            Weight = 30;
        }

		// Добавление коробки
        public void AddBox(Box box)
		{
            // -Каждая коробка не должна превышать по размерам паллету(по ширине и глубине).
            if (box.Width <= Width && box.Depth <= Depth)
			{
				boxes.Add(box);
				UpdateAttributes();
			}
			else
			{
				Console.WriteLine($"Ошибка: Коробка {box.Id} не помещается на паллету {Id}");
			}
		}

		// Пересчитываем атрибуты паллеты при добавлении новой коробки
		private void UpdateAttributes()
		{
			double totalBoxWeight = boxes.Sum(box => box.Weight);
			// Вес паллеты вычисляется из суммы веса вложенных коробок + 30кг.
            Weight += totalBoxWeight;  

			double totalBoxVolume = boxes.Sum(box => box.CalculateVolume());
			double palletVolume = Width * Height * Depth;
            // - Объем паллеты вычисляется как сумма объема всех находящихся на ней коробок
			//  и произведения ширины, высоты и глубины паллеты.
            Volume = totalBoxVolume + palletVolume; 

            //		 - Срок годности паллеты вычисляется из наименьшего срока годности коробки, вложенной в паллету. 
            ExpiryDate = boxes.Min(box => box.ExpiryDate);
		}

		public bool IsPalletValid()
		{
			// Проверяем каждую коробку на паллете
			foreach (var box in boxes)
			{
				// Если хотя бы одна коробка просрочена, то паллета не годится
				if (box.ExpiryDate < DateOnly.FromDateTime(DateTime.Now))
				{
					return false;
				}
			}
			// Если все коробки в порядке, то паллета годится
			return true;
		}

		public double Volume { get; private set; }
		public DateOnly ExpiryDate { get; private set; }
	}
}
