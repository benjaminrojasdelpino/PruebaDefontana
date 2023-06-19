using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PruebaDefontana.Data.Models;

namespace PruebaDefontana.Business.Services;

public class QueryService
{
    private readonly Context dbContext;

    public QueryService(Context dbContext)
    {
        this.dbContext = dbContext;
    }
    public List<Ventum> ObtenerVentasUltimosXDias(int numeroDias)
    {
        DateTime fechaLimite = DateTime.Now.AddDays(-numeroDias);

        var ventas = dbContext.Venta
            .Include(v => v.IdLocalAux)
            .Include(v => v.VentaDetalles).ThenInclude(d => d.IdProductoAux)
            .ThenInclude(p => p.IdMarcaAux)
            .Where(venta => venta.Fecha >= fechaLimite)
            .ToList();

        return ventas;
    }

}