using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using MVVM_Mon.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MVVM_Mon.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

		public MainWindow()
		{
			AvaloniaXamlLoader.Load(this);
			_viewModel = new MainWindowViewModel();
			DataContext = _viewModel;

            // Заполнение TreeView данными из базы данных если нужно
            var config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json")
             .Build();
            var appSettings = config.GetSection("ConnectionStrings")["LoadFromDb"];
			if(appSettings == "on")
			{
				Task.Run(async () => await _viewModel.InitializeFromDbAsync());
            }

			InitInterfaceFromApp();
		}

		private void InitInterfaceFromApp()
        {
			_viewModel.InitializeInApp();

			// Вывести на экран:
			// - Сгруппировать все паллеты по сроку годности, отсортировать по возрастанию срока годности

			var groupedPallets = _viewModel.Pallets
				.OrderBy(p => p.ExpiryDate)
				.GroupBy(p => p.ExpiryDate);

			TreeView itemsTreeView = new TreeView()
			{
				Width = 750
			};
			DateOnly today = DateOnly.FromDateTime(DateTime.Now);
			foreach (var expiryGroup in groupedPallets)
			{
				//проверка на негодность
				DateOnly expiryDatePlus100Days = expiryGroup.Key.AddDays(100);
				bool isGroupValid = expiryDatePlus100Days > today;
				// Создание узла для группы срока годности
				TreeViewItem mainNode = new TreeViewItem();
				mainNode.Header = $"Группа по сроку годности: {expiryGroup.Key} (Состояние: {(isGroupValid ? "Годен 🍔" : "Просрочен 🚩")})";

				// Вывести на экран:
				// в каждой группе отсортировать паллеты по весу
				var sortedPallets = expiryGroup
					.OrderBy(p => p.Weight);

				//        - 3 паллеты, которые содержат коробки с наибольшим сроком годности,
				//        отсортированные по возрастанию объема.
				var topThreePallets = sortedPallets.OrderByDescending(p => p.Volume).Take(3).ToList();

				sortedPallets = sortedPallets.OrderBy(w => w.Weight);
                // раздам медальки)))
                var rankNames = new string[] { "🏆", "🥈", "🥉" };
				int palletIndex = 0;

				foreach (var palletViewModel in topThreePallets)
				{
					// Создание узла для палеты
					TreeViewItem palletNode = new TreeViewItem();
					palletNode.Header = $"Палет: {palletViewModel.Name} Годен до: {palletViewModel.ExpiryDate}, Вес: {palletViewModel.Weight}, Объем: {palletViewModel.Volume} у.е.  {rankNames[palletIndex]}  ";
					palletIndex++;
					foreach (var boxViewModel in palletViewModel.Boxes)
					{
						// Создание узла для коробки
						TreeViewItem boxNode = new TreeViewItem();
						boxNode.Header = $"Коробка: {boxViewModel.Name} Дата производства: {boxViewModel.ProductionDate}, Годен до: {boxViewModel.ExpiryDate}, Объем: {boxViewModel.Volume} у.е.";
						palletNode.Items.Add(boxNode);
					}

					// Добавление узла палеты к узлу группы срока годности
					mainNode.Items.Add(palletNode);
				}

				// Добавление оставшихся палет из sortedPallets
				foreach (var palletViewModel in sortedPallets.Except(topThreePallets))
				{
					// Создание узла для палеты
					TreeViewItem palletNode = new TreeViewItem();
					palletNode.Header = $"Палет: {palletViewModel.Name} Годен до: {palletViewModel.ExpiryDate}, Вес: {palletViewModel.Weight}, Объем: {palletViewModel.Volume} у.е.";

					foreach (var boxViewModel in palletViewModel.Boxes)
					{
						// Создание узла для коробки
						TreeViewItem boxNode = new TreeViewItem();
						boxNode.Header = $"Коробка: {boxViewModel.Name} Дата производства: {boxViewModel.ProductionDate}, Годен до: {boxViewModel.ExpiryDate}, Объем: {boxViewModel.Volume} у.е.";
						palletNode.Items.Add(boxNode);
					}

					// Добавление узла палеты к узлу группы срока годности
					mainNode.Items.Add(palletNode);
				}

				// Добавление узла группы срока годности к TreeView
				itemsTreeView.Items.Add(mainNode);
			}

			Content = new StackPanel
			{
				Orientation = Avalonia.Layout.Orientation.Horizontal,
				Children =
	             {
		             _viewModel.DbTreeView,
		             itemsTreeView
	             }
			};
		}
    }
}
