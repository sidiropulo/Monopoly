using System;
using System.Collections.Generic;

namespace MonopolyDatabaseContext.DatabaseContext;

public partial class Palletsdb
{
    public int PalletId { get; set; }

    public string PalletName { get; set; } = null!;

    public double Width { get; set; }

    public double Height { get; set; }

    public double Depth { get; set; }

    public double Weight { get; set; }

    public double Volume { get; set; }

    public DateOnly ExpiryDate { get; set; }

	public virtual ICollection<Boxesdb> Boxesdbs { get; set; } = new List<Boxesdb>();
}
