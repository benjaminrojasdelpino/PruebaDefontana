-- El total de ventas de los últimos 30 días (monto total y cantidad total de ventas).
SELECT
    SUM(Total) AS MontoTotal,
    COUNT(*) AS CantidadTotal
FROM
    Venta WITH(NOLOCK)
WHERE
    Fecha >= DATEADD(day, -30, GETDATE());


-- El día y hora en que se realizó la venta con el monto más alto (y cuál es aquel monto).
SELECT TOP 1
    Fecha AS FechaYHora,
    Total AS Monto
FROM
    Venta WITH(NOLOCK)
WHERE
    Fecha >= DATEADD(day, -30, GETDATE())
ORDER BY
    Total DESC;

-- Indicar cuál es el producto con mayor monto total de ventas.
SELECT TOP 1
    D.ID_Producto,
    P.Nombre,
    SUM(D.TotalLinea) AS MontoTotalVentas
FROM
    Venta V WITH(NOLOCK)
    INNER JOIN VentaDetalle D WITH(NOLOCK) ON V.ID_Venta = D.ID_Venta
    INNER JOIN Producto P WITH(NOLOCK) ON D.ID_Producto = P.ID_Producto
WHERE
    V.Fecha >= DATEADD(day, -30, GETDATE()) -- Filtrar ventas de los últimos 30 días
GROUP BY
    D.ID_Producto, P.Nombre
ORDER BY
    MontoTotalVentas DESC;

-- Indicar el local con mayor monto de ventas.
SELECT TOP 1
    V.ID_Local,
    L.Nombre AS NombreLocal,
    L.Direccion AS Direccion,
    SUM(V.Total) AS MontoTotalVentas
FROM
    Venta V WITH(NOLOCK)
INNER JOIN
    Local L WITH(NOLOCK) ON V.ID_Local = L.ID_Local
WHERE
    V.Fecha >= DATEADD(day, -30, GETDATE()) -- Filtrar ventas de los últimos 30 días
GROUP BY
    V.ID_Local, L.Nombre, L.Direccion
ORDER BY
    MontoTotalVentas DESC;

-- ¿Cuál es la marca con mayor margen de ganancias?
SELECT TOP 1
    M.ID_Marca,
    M.Nombre,
    SUM((D.Precio_Unitario - P.Costo_Unitario) * D.Cantidad) AS MargenGanancias
FROM
    Venta V WITH(NOLOCK)
    INNER JOIN VentaDetalle D WITH(NOLOCK) ON V.ID_Venta = D.ID_Venta
    INNER JOIN Producto P WITH(NOLOCK) ON D.ID_Producto = P.ID_Producto
    INNER JOIN Marca M WITH(NOLOCK) ON P.ID_Marca = M.ID_Marca
WHERE
    V.Fecha >= DATEADD(day, -30, GETDATE()) -- Filtrar ventas de los últimos 30 días
GROUP BY
    M.ID_Marca,
    M.Nombre
ORDER BY
    MargenGanancias DESC;


-- ¿Cómo obtendrías cuál es el producto que más se vende en cada local?
SELECT l.ID_Local, l.Nombre AS Local, p.ID_Producto, p.Nombre AS Producto, vp.TotalVentasPorProducto AS TotalVentas
FROM (
    SELECT v.ID_Local, p.ID_Producto, SUM(vd.Cantidad) AS TotalVentasPorProducto
    FROM VentaDetalle vd WITH(NOLOCK)
    JOIN Producto p WITH(NOLOCK) ON vd.ID_Producto = p.ID_Producto
    JOIN Venta v WITH(NOLOCK) ON v.ID_Venta = vd.ID_Venta
    WHERE v.Fecha >= DATEADD(DAY, -30, GETDATE())
    GROUP BY v.ID_Local, p.ID_Producto
) AS vp
JOIN (
    SELECT vp.ID_Local, MAX(vp.TotalVentasPorProducto) AS MaxTotalVentasPorLocal
    FROM (
        SELECT v.ID_Local, p.ID_Producto, SUM(vd.Cantidad) AS TotalVentasPorProducto
        FROM VentaDetalle vd WITH(NOLOCK)
        JOIN Producto p WITH(NOLOCK) ON vd.ID_Producto = p.ID_Producto
        JOIN Venta v WITH(NOLOCK) ON v.ID_Venta = vd.ID_Venta
        WHERE v.Fecha >= DATEADD(DAY, -30, GETDATE())
        GROUP BY v.ID_Local, p.ID_Producto
    ) AS vp
    GROUP BY vp.ID_Local
) AS mv ON vp.ID_Local = mv.ID_Local AND vp.TotalVentasPorProducto = mv.MaxTotalVentasPorLocal
JOIN Local l WITH(NOLOCK) ON l.ID_Local = vp.ID_Local
JOIN Producto p WITH(NOLOCK) ON p.ID_Producto = vp.ID_Producto
ORDER BY l.ID_Local, TotalVentas DESC;

