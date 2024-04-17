﻿using MVVM_Mon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MVVM_Mon.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public ObservableCollection<PalletViewModel> Pallets { get; set; }

		public MainWindowViewModel()
		{
			Pallets = new ObservableCollection<PalletViewModel>();
        }

        //  Получение данных для приложения можно организовать одним из способов:
        // Реализация: Генерация прямо в приложении
        internal void InitializeInApp()
		{
            // Создание паллетов и коробок
            List<Pallet> pallets = new List<Pallet>();
            Random random = new Random();

            for (int i = 1; i <= 25; i++)
            {
                // Генерация рандомных параметров для палета 
                double width = random.Next(80, 121); // Ширина от 80 до 120
                double height = random.Next(80, 121); // Высота от 80 до 120
                double depth = random.Next(80, 121); // Глубина от 80 до 120

                Pallet pallet = new Pallet($"P{i}", width, height, depth);

                // Генерация случайного количества коробок от 2 до 5
                int numBoxes = random.Next(5, 15);

                for (int j = 1; j <= numBoxes; j++)
                {
                    // Генерация рандомных параметров для коробки
                    double boxWidth = random.Next(50, 71); // Ширина коробки от 50 до 70
                    double boxHeight = random.Next(50, 71); // Высота коробки от 50 до 70
                    double boxDepth = random.Next(50, 71); // Глубина коробки от 50 до 70
                    double weight = random.Next(10, 56); // Вес коробки от 10 до 55

                    Box box = new Box($"B{i}{j}", boxWidth, boxHeight, boxDepth, weight, new DateOnly(random.Next(2022, 2036), 1, 1));
                    pallet.AddBox(box);
                }

                pallets.Add(pallet);
            }

            foreach (Pallet pallet in pallets)
            {
                Pallets.Add(new PalletViewModel(pallet));
            }


        }
    }
}
