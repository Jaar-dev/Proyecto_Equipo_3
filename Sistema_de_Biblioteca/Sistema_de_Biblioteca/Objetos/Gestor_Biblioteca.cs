using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public abstract class Gestor_Biblioteca : Empleado
    {
        protected Gestor_Biblioteca() : base() { }
        public Gestor_Biblioteca(string nombre, string identidad, string correoElectrónico,
                                string teléfono, DateTime fechaNacimiento, string dirección)
            : base(nombre, identidad, correoElectrónico, teléfono, fechaNacimiento, dirección)
        {
        }

        public bool RegistrarNuevoLibro(List<Libro> libros, Libro nuevoLibro)
        {
            if (nuevoLibro == null)
            {
                WriteLine("Error: No se puede registrar un libro nulo.");
                return false;
            }

            if (libros.Any(l => l.ISBN.Equals(nuevoLibro.ISBN, StringComparison.OrdinalIgnoreCase)))
            {
                WriteLine($"Error: Ya existe un libro con ISBN '{nuevoLibro.ISBN}'.");
                return false;
            }

            libros.Add(nuevoLibro);
            WriteLine($" {Nombre} ha registrado el nuevo libro: '{nuevoLibro.Titulo}' (ISBN: {nuevoLibro.ISBN})");
            return true;
        }

        public bool ProcesarDevolución(Préstamo préstamo)
        {
            if (préstamo == null)
            {
                WriteLine("Error: No se puede procesar una devolución de préstamo nula.");
                return false;
            }

            if (préstamo.Estado == "Devuelto")
            {
                WriteLine($"El préstamo {préstamo.IdPréstamo} ya ha sido devuelto.");
                return false;
            }

            if (préstamo.Estado == "Cancelado")
            {
                WriteLine($"Error: El préstamo {préstamo.IdPréstamo} fue cancelado y no puede ser devuelto.");
                return false;
            }

            préstamo.RegistrarDevolución();
            WriteLine($" {Nombre} ha procesado la devolución del préstamo {préstamo.IdPréstamo}.");

            if (préstamo.Multa > 0)
            {
                WriteLine($" Multa generada: {préstamo.Multa:C} por {préstamo.DiasVencidos()} días de retraso.");
            }

            return true;
        }

        public void GenerarReporteBásico(string tipoReporte)
        {
            WriteLine($" {Nombre} está generando un reporte de tipo: {tipoReporte}");
            WriteLine($"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        }

        public void GenerarReporteCompleto(List<Libro> libros, List<Préstamo> prestamos,
                                         List<Estudiante> estudiantes, string tipoReporte = "General")
        {
            string separador = new string('=', 50);

            WriteLine($"\n{separador}");
            WriteLine($" REPORTE {tipoReporte.ToUpper()} - BIBLIOTECA");
            WriteLine($"Generado por: {Nombre} ({GetType().Name})");
            WriteLine($"Fecha: {DateTime.Now:dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm:ss}");
            WriteLine($"{separador}");

            WriteLine($"\n INVENTARIO DE LIBROS:");
            WriteLine($"   • Total de títulos registrados: {libros.Count}");
            WriteLine($"   • Total de copias en biblioteca: {libros.Sum(l => l.NumeroCopiasTotales)}");
            WriteLine($"   • Copias disponibles: {libros.Sum(l => l.NumeroCopiasDisponibles)}");
            WriteLine($"   • Copias prestadas: {libros.Sum(l => l.NumeroCopiasTotales - l.NumeroCopiasDisponibles)}");

            WriteLine($"\n ESTUDIANTES:");
            WriteLine($"   • Total de estudiantes registrados: {estudiantes.Count}");
            WriteLine($"   • Estudiantes con préstamos activos: {estudiantes.Count(e => e.LibrosPrestados.Any())}");

            WriteLine($"\n PRÉSTAMOS:");
            WriteLine($"   • Total de préstamos realizados: {prestamos.Count}");
            WriteLine($"   • Préstamos activos: {prestamos.Count(p => p.Estado == "Activo" || p.Estado == "Confirmado")}");
            WriteLine($"   • Préstamos devueltos: {prestamos.Count(p => p.Estado == "Devuelto")}");
            WriteLine($"   • Préstamos cancelados: {prestamos.Count(p => p.Estado == "Cancelado")}");
            WriteLine($"   • Préstamos vencidos: {prestamos.Count(p => p.Estado == "Activo" && p.DiasVencidos() > 0)}");

            var multaTotal = prestamos.Where(p => p.Multa > 0).Sum(p => p.Multa);
            if (multaTotal > 0)
            {
                WriteLine($"\n MULTAS:");
                WriteLine($"   • Total de multas generadas: {multaTotal:C}");
                WriteLine($"   • Préstamos con multa: {prestamos.Count(p => p.Multa > 0)}");
            }

            if (prestamos.Any())
            {
                var topLibros = prestamos.GroupBy(p => p.LibroPrestado.ISBN)
                                        .Select(g => new
                                        {
                                            ISBN = g.Key,
                                            Titulo = g.First().LibroPrestado.Titulo,
                                            Cantidad = g.Count()
                                        })
                                        .OrderByDescending(x => x.Cantidad)
                                        .Take(5);

                WriteLine($"\n TOP 5 LIBROS MÁS PRESTADOS:");
                int posicion = 1;
                foreach (var item in topLibros)
                {
                    WriteLine($"   {posicion}. '{item.Titulo}' - {item.Cantidad} préstamos");
                    posicion++;
                }
            }

            WriteLine($"\n{separador}");
            WriteLine($"Fin del reporte - {DateTime.Now:HH:mm:ss}");
        }

        public virtual bool GestionarMulta(Préstamo préstamo, bool pagada = false)
        {
            if (préstamo?.Multa <= 0) return false;

            if (pagada)
            {
                WriteLine($" Multa de {préstamo.Multa:C} marcada como pagada por {Nombre}");
                WriteLine($"   Préstamo: {préstamo.IdPréstamo} - '{préstamo.LibroPrestado.Titulo}'");
                return true;
            }
            else
            {
                WriteLine($"  Multa pendiente: {préstamo.Multa:C} - Préstamo {préstamo.IdPréstamo}");
                return false;
            }
        }

        public abstract bool PuedeRealizarOperación(string tipoOperación);

        protected void RegistrarActividad(string actividad)
        {
            WriteLine($" [{DateTime.Now:HH:mm:ss}] {Nombre}: {actividad}");
        }
    }
}