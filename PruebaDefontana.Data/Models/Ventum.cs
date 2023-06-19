﻿using System;
using System.Collections.Generic;

namespace PruebaDefontana.Data.Models;

public partial class Ventum
{
    public long IdVenta { get; set; }

    public int Total { get; set; }

    public DateTime Fecha { get; set; }

    public long IdLocal { get; set; }

    public virtual Local IdLocalAux { get; set; } = null!;

    public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
}
