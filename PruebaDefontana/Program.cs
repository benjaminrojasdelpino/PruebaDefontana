using PruebaDefontana.Data.Models;
using PruebaDefontana.Data;
using PruebaDefontana.Business.Services;
using PruebaDefontana.Business;

namespace PruebaDefontana.Presentacion;

class Program
{
    private readonly QueryService _queryService;

    static void Main(string[] args)
    {
        using (var dbContext = new Context())
        {
            var program = new Program(new QueryService(dbContext));
            bool regresar = false;

            while (!regresar)
            {
                Console.WriteLine("PRUEBA DEFONTANA - DESARROLLADOR BACKEND .NET Y SQL");
                Console.WriteLine();
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("1. Total de ventas de los últimos 30 días");
                Console.WriteLine("2. Venta con el monto más alto");
                Console.WriteLine("3. Producto con mayor monto total de ventas");
                Console.WriteLine("4. Local con mayor monto de ventas");
                Console.WriteLine("5. Marca con mayor margen de ganancias");
                Console.WriteLine("6. Productos más vendidos por local");
                Console.WriteLine("0. Salir");

                Console.Write("Opción seleccionada: ");

                Console.WriteLine();

                switch (Console.ReadLine())
                {
                    case "1":
                        var totales = program.TotalVentas30Dias();
                        Console.WriteLine("Total de ventas en los últimos 30 días:");
                        Console.WriteLine("Monto total: " + totales.MontoTotal.ToString("N0"));
                        Console.WriteLine("Cantidad total: " + totales.CantidadTotal);
                        break;

                    case "2":
                        var ventaMasAlta = program.ObtenerVentaConMontoMasAlto();
                        if (ventaMasAlta.FechaVenta != DateTime.MinValue)
                        {
                            Console.WriteLine("Venta con el monto más alto:");
                            Console.WriteLine("Fecha y hora: " + ventaMasAlta.FechaVenta);
                            Console.WriteLine("Monto: " + ventaMasAlta.MontoVenta.ToString("N0"));
                        }
                        else
                        {
                            Console.WriteLine("No se encontraron ventas en los últimos 30 días.");
                        }
                        break;

                    case "3":
                        var (productoConMayorMontoVentas, montoVentas) = program.ObtenerProductoConMayorMontoVentas();
                        if (productoConMayorMontoVentas != null)
                        {
                            Console.WriteLine("Producto con mayor monto de ventas:");
                            Console.WriteLine("Nombre: " + productoConMayorMontoVentas.Nombre);
                            Console.WriteLine("Monto: " + montoVentas.ToString("N0"));
                        }
                        else
                        {
                            Console.WriteLine("No se encontraron productos con ventas en los últimos 30 días.");
                        }
                        break;

                    case "4":
                        var (localConMayorMontoVentas, montoVentasLocal) = program.ObtenerLocalConMayorMontoVentas();
                        if (localConMayorMontoVentas != null)
                        {
                            Console.WriteLine("Local con mayor monto de ventas:");
                            Console.WriteLine("Nombre: " + localConMayorMontoVentas.Nombre);
                            Console.WriteLine("Dirección: " + localConMayorMontoVentas.Direccion);
                            Console.WriteLine("Monto: " + montoVentasLocal.ToString("N0"));
                        }
                        else
                        {
                            Console.WriteLine("No se encontraron locales con ventas en los últimos 30 días.");
                        }
                        break;

                    case "5":
                        var (marcaConMayorMargen, margenGanancias) = program.ObtenerMarcaConMayorMargenGanancias();
                        if (marcaConMayorMargen != null)
                        {
                            Console.WriteLine("Marca con mayor margen de ganancias:");
                            Console.WriteLine("Nombre: " + marcaConMayorMargen.Nombre);
                            Console.WriteLine("Margen Ganancias: " + margenGanancias.ToString("N0"));
                        }
                        else
                        {
                            Console.WriteLine("No se encontró ninguna marca con margen de ganancias.");
                        }
                        break;

                    case "6":
                        var productoMasVendidoPorLocal = program.ObtenerProductoMasVendidoPorLocal();

                        Console.WriteLine("--------------------------------------------------------------");
                        Console.WriteLine("|   Local            |   Producto más vendido  |   Total   |");
                        Console.WriteLine("--------------------------------------------------------------");

                        foreach (var prod in productoMasVendidoPorLocal)
                        {
                            string local = prod.Key.Nombre;

                            if (prod.Value.Count > 0)
                            {
                                foreach (var productoVenta in prod.Value)
                                {
                                    string producto = productoVenta.Item1.Nombre;
                                    int totalVentas = productoVenta.Item2;

                                    Console.WriteLine("|{0,-20}|{1,-25}|{2,-11}|", local, producto, totalVentas);
                                }
                            }
                            else
                            {
                                Console.WriteLine("|{0,-20}|{1,-25}|{2,-11}|", local, "No se encontró producto más vendido para este local.", "");
                            }

                            Console.WriteLine("--------------------------------------------------------------");
                        }
                        break;

                    case "0":
                        regresar = true;
                        break;

                    default:
                        Console.WriteLine("Opción inválida. Por favor, seleccione una opción válida.");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Presione una tecla para continuar...");
                Console.ReadKey();
                Console.Clear();
            }






        }
    }

    public Program(QueryService queryService)
    {
        _queryService = queryService;
    }

    public List<Ventum> GetDataVentas(int dias)
    {
        var ventas = _queryService.ObtenerVentasUltimosXDias(dias);
        return ventas;
    }

    public (decimal MontoTotal, int CantidadTotal) TotalVentas30Dias()
    {
        var ventas = GetDataVentas(30);

        decimal montoTotal = ventas.Sum(v => v.Total);
        int cantidadTotal = ventas.Count;

        return (montoTotal, cantidadTotal);
    }

    public (DateTime FechaVenta, decimal MontoVenta) ObtenerVentaConMontoMasAlto()
    {
        var ventas = GetDataVentas(30);

        var ventaMasAlta = ventas.OrderByDescending(v => v.Total).FirstOrDefault();

        if (ventaMasAlta != null)
        {
            return (ventaMasAlta.Fecha, ventaMasAlta.Total);
        }

        return (DateTime.MinValue, 0);
    }

    public (Producto?, int) ObtenerProductoConMayorMontoVentas()
    {
        var ventas = GetDataVentas(30);

        var totalVentas = 0;

        var ventasPorProducto = ventas
            .SelectMany(v => v.VentaDetalles)
            .GroupBy(d => d.IdProducto)
            .Select(g => new
            {
                IdProducto = g.Key,
                MontoTotalVentas = g.Sum(d => d.TotalLinea)
            });

        var productoConMayorMontoVentas = ventasPorProducto
            .OrderByDescending(p => p.MontoTotalVentas)
            .FirstOrDefault();

        if (productoConMayorMontoVentas != null)
        {
            var producto = ventas
                .SelectMany(v => v.VentaDetalles)
                .Where(d => d.IdProducto == productoConMayorMontoVentas.IdProducto)
                .Select(d => new Producto
                {
                    IdProducto = d.IdProducto,
                    Nombre = d.IdProductoAux.Nombre
                })
                .FirstOrDefault();


            totalVentas = productoConMayorMontoVentas.MontoTotalVentas;

            return (producto, totalVentas);
        }

        return (null, 0);
    }

    public (Local?, decimal) ObtenerLocalConMayorMontoVentas()
    {
        var ventas = GetDataVentas(30);

        var localesConMontoVentas = ventas
            .GroupBy(v => v.IdLocal)
            .Select(g => new
            {
                IdLocal = g.Key,
                MontoVentas = g.Sum(v => v.Total)
            });

        var localConMayorMontoVentas = localesConMontoVentas
            .OrderByDescending(l => l.MontoVentas)
            .FirstOrDefault();

        if (localConMayorMontoVentas != null)
        {
            var local = ventas
                .Where(v => v.IdLocal == localConMayorMontoVentas.IdLocal)
                .Select(v => new Local
                {
                    IdLocal = v.IdLocal,
                    Nombre = v.IdLocalAux.Nombre,
                    Direccion = v.IdLocalAux.Direccion
                })
                .FirstOrDefault();

            decimal montoVentas = localConMayorMontoVentas.MontoVentas;

            return (local, montoVentas);
        }

        return (null, 0);
    }


    public (Marca?, decimal) ObtenerMarcaConMayorMargenGanancias()
    {
        var ventas = GetDataVentas(30);

        var margenGananciasPorMarca = ventas
            .SelectMany(v => v.VentaDetalles)
            .GroupBy(d => d.IdProductoAux.IdMarca)
            .Select(g => new
            {
                IdMarca = g.Key,
                MargenGanancias = g.Sum(d => (d.PrecioUnitario - d.IdProductoAux.CostoUnitario) * d.Cantidad)
            });

        var marcaConMayorMargenGanancias = margenGananciasPorMarca
            .OrderByDescending(m => m.MargenGanancias)
            .FirstOrDefault();

        if (marcaConMayorMargenGanancias != null)
        {
            var marca = ventas
                .SelectMany(v => v.VentaDetalles)
                .FirstOrDefault(d => d.IdProductoAux.IdMarca == marcaConMayorMargenGanancias.IdMarca)
                ?.IdProductoAux.IdMarcaAux;

            decimal margenGanancias = marcaConMayorMargenGanancias.MargenGanancias;

            return (marca, margenGanancias);
        }

        return (null, 0);
    }

    public Dictionary<Local, List<(Producto, int)>> ObtenerProductoMasVendidoPorLocal()
    {
        var ventas = GetDataVentas(30);

        var productoMasVendidoPorLocal = ventas
            .GroupBy(v => v.IdLocalAux)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var productosMasVendidos = g
                        .SelectMany(v => v.VentaDetalles)
                        .GroupBy(d => d.IdProductoAux)
                        .Select(g2 => new
                        {
                            Producto = g2.Key,
                            TotalVentas = g2.Sum(d => d.Cantidad)
                        })
                        .ToList();

                    var maxVentas = productosMasVendidos.Max(p => p.TotalVentas);

                    var productosMaxVentas = productosMasVendidos
                        .Where(p => p.TotalVentas == maxVentas)
                        .Select(p => (p.Producto, p.TotalVentas))
                        .ToList();

                    return productosMaxVentas;
                });

        return productoMasVendidoPorLocal;
    }




}