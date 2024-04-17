using MVVM_Mon.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MVVM_Mon.ViewModels
{
    public class PalletViewModel
    {
        public string Name { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public double Volume { get; set; }

        public double Weight { get; set; }
        public ObservableCollection<BoxViewModel> Boxes { get; set; }

        internal PalletViewModel(Pallet pallet)
        {
            Name = pallet.Id;
            Boxes = new ObservableCollection<BoxViewModel>();

            double totalBoxVolume = pallet.boxes.Sum(box => box.Width * box.Height * box.Depth);
            Volume = totalBoxVolume + (pallet.Width * pallet.Height * pallet.Depth);

            DateOnly minExpiryDate = pallet.boxes.Min(box => box.ExpiryDate);
            ExpiryDate = minExpiryDate;

            Weight = pallet.Weight;

            foreach (var box in pallet.boxes)
            {
                Boxes.Add(new BoxViewModel(box));
            }
        }
    }
}
