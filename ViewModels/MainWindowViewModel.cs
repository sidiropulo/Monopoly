using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using MonopolyDatabaseContext.DatabaseContext;
using MVVM_Mon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MVVM_Mon.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public ObservableCollection<PalletViewModel> Pallets { get; set; }

		public TreeView DbTreeView { get; set; }

		public MainWindowViewModel()
		{
			Pallets = new ObservableCollection<PalletViewModel>();

			DbTreeView = new TreeView() { 
	             Width =  700
			};

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
        //  Получение данных для приложения можно организовать одним из способов:
        // Реализация: Получение из БД (postgres)
        internal async Task InitializeFromDbAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                using (var context = new VpmContext())
                {
                    var palletsFromDb = await context.Palletsdbs.Include(p => p.Boxesdbs).ToListAsync();

                    var groupedPallets = palletsFromDb
                        .OrderBy(p => p.ExpiryDate)
                        .GroupBy(p => p.ExpiryDate);

                    DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                    foreach (var expiryGroup in groupedPallets)
                    {
                        DateOnly expiryDatePlus100Days = expiryGroup.Key.AddDays(100);
                        bool isGroupValid = expiryDatePlus100Days > today;

                        TreeViewItem mainNode = new TreeViewItem();
                        mainNode.Header = $"Группа по сроку годности: {expiryGroup.Key} (Состояние: {(isGroupValid ? "Годен 🍔" : "Просрочен 🚩")})";

                        var sortedPallets = expiryGroup.OrderBy(p => p.Weight);

                        var topThreePallets = sortedPallets.OrderByDescending(p => p.Volume).Take(3).ToList();
                        var rankNames = new string[] { "🏆", "🥈", "🥉" };
                        int palletIndex = 0;

                        foreach (var palletViewModel in topThreePallets)
                        {
                            TreeViewItem palletNode = CreatePalletNode(palletViewModel, rankNames[palletIndex]);
                            mainNode.Items.Add(palletNode);
                            palletIndex++;
                        }

                        foreach (var palletDbViewModel in sortedPallets.Except(topThreePallets))
                        {
                            TreeViewItem palletNode = CreatePalletNode(palletDbViewModel);
                            mainNode.Items.Add(palletNode);
                        }

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            DbTreeView.Items.Add(mainNode);
                        });
                    }
                }
            });
        }
        private TreeViewItem CreatePalletNode(Palletsdb palletDbViewModel, string rankName = "")
        {
            TreeViewItem palletNode = new TreeViewItem();
            palletNode.Header = $"Палет: {palletDbViewModel.PalletName} Годен до: {palletDbViewModel.ExpiryDate}, Вес: {palletDbViewModel.Weight}, Объем: {palletDbViewModel.Volume} у.е.  {rankName}";

            foreach (var boxDbViewModel in palletDbViewModel.Boxesdbs)
            {
                TreeViewItem boxNode = CreateBoxNode(boxDbViewModel);
                palletNode.Items.Add(boxNode);
            }

            return palletNode;
        }

        private TreeViewItem CreateBoxNode(Boxesdb boxDbViewModel)
        {
            TreeViewItem boxNode = new TreeViewItem();
            boxNode.Header = $"Коробка: {boxDbViewModel.BoxName} Дата производства: {boxDbViewModel.ProductionDate}, Годен до: {boxDbViewModel.ExpiryDate}, Объем: {boxDbViewModel.Volume} у.е.";
            return boxNode;
        }
         
	}
}
