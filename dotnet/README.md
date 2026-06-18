# Version C# / .NET 8

Esta carpeta contiene una version ASP.NET Core MVC de la app para comparar:

- Excel: columna `Evaluacion 2`
- PDF: campo/texto `Evaluado`

La primera version funciona en modo local: subes un Excel `.xlsx` y un ZIP con PDFs, y la app genera un reporte Excel con las diferencias.

## Requisitos

- Visual Studio 2022
- .NET 8 SDK

## Ejecutar desde terminal

```bash
cd dotnet/FormaPdfExcelComparerDotNet
dotnet restore
dotnet run
```

Abre la URL que muestre la consola, normalmente:

```text
https://localhost:5001
```

o:

```text
http://localhost:5000
```

## Paquetes usados

- `ClosedXML` para leer y generar Excel.
- `UglyToad.PdfPig` para leer texto de PDFs.

## Flujo

1. Subir Excel `.xlsx`.
2. Subir ZIP con PDFs.
3. Indicar columna del Excel: por defecto `Evaluacion 2`.
4. Opcional: indicar columna clave para relacionar cada fila con un PDF.
5. Descargar `reporte_comparacion.xlsx`.

## Autodesk APS

La integracion directa con Autodesk Forma / Autodesk Docs queda como siguiente etapa. Esta version deja la base separada para agregar servicios APS sin mezclar la logica de comparacion.
