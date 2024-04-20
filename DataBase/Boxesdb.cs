using System;
using System.Collections.Generic;

namespace MonopolyDatabaseContext.DatabaseContext;
public partial class Boxesdb
{
    public int BoxId { get; set; }

    public string BoxName { get; set; } = null!;

    public double Width { get; set; }

    public double Height { get; set; }

    public double Depth { get; set; }

    public double Weight { get; set; }

    public DateOnly ProductionDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public int? PalletId { get; set; }

	public double? Volume { get; set; }

	public virtual Palletsdb? Pallet { get; set; }
}
