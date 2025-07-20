using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public class Préstamo
    {
        private static int nextIdPréstamo = 1;
        public static int TotalPrestamos { get; private set; } = 0;

        private const int DIAS_PRESTAMO_DEFAULT = 7;
        private const decimal MULTA_POR_DIA = 0.50m;
        private const int MAX_LIBROS_POR_PRESTAMO = 5;
        private const int MAX_RENOVACIONES = 2;

        public int IdPréstamo { get; private set; }
        public string IdEstudiante { get; set; }

        [JsonIgnore]
        public Estudiante Alumno { get; private set; }

        public List<string> ISBNLibros { get; set; } = new List<string>();

        [JsonIgnore]
        public List<Libro> LibrosPrestados { get; private set; } = new List<Libro>();

        private DateTime fechaPréstamo;
        public DateTime FechaPréstamo
        {
            get => fechaPréstamo;
            private set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("La fecha de préstamo no puede ser futura.");
                fechaPréstamo = value;
            }
        }

        private DateTime fechaLimite;
        public DateTime FechaLimite
        {
            get => fechaLimite;
            private set
            {
                if (value <= FechaPréstamo)
                    throw new ArgumentException("La fecha límite debe ser posterior a la fecha de préstamo.");
                fechaLimite = value;
            }
        }

        public DateTime? FechaDevoluciónReal { get; set; }

        private string estado;
        public string Estado
        {
            get => estado;
            private set
            {
                string[] estadosValidos = { "Activo", "Confirmado", "Devuelto", "Cancelado", "Vencido", "Parcialmente Devuelto" };
                if (!estadosValidos.Contains(value))
                    throw new ArgumentException($"Estado inválido. Debe ser uno de: {string.Join(", ", estadosValidos)}");
                estado = value;
            }
        }

        public decimal Multa { get; private set; }
        public int Renovaciones { get; private set; } = 0;
        public string NotasAdicionales { get; set; }
        public DateTime? FechaUltimaRenovacion { get; private set; }

        // Para devoluciones parciales
        public List<string> ISBNLibrosDevueltos { get; set; } = new List<string>();

        public Préstamo()
        {
            ISBNLibros = new List<string>();
            LibrosPrestados = new List<Libro>();
            ISBNLibrosDevueltos = new List<string>();
        }

        public Préstamo(Estudiante alumno, Libro libro) : this(alumno, new List<Libro> { libro })
        {
        }

        // Constructor para múltiples libros
        public Préstamo(Estudiante alumno, List<Libro> libros)
        {
            ValidarParametros(alumno, libros);

            IdPréstamo = nextIdPréstamo++;
            Alumno = alumno;
            LibrosPrestados = new List<Libro>(libros);
            IdEstudiante = alumno.Identidad;
            ISBNLibros = libros.Select(l => l.ISBN).ToList();
            FechaPréstamo = DateTime.Now;
            FechaLimite = FechaPréstamo.AddDays(DIAS_PRESTAMO_DEFAULT);
            Estado = "Activo";
            Multa = 0m;
            FechaDevoluciónReal = null;

            TotalPrestamos++;
        }

        private void ValidarParametros(Estudiante alumno, List<Libro> libros)
        {
            if (alumno == null)
                throw new ArgumentNullException(nameof(alumno), "El estudiante no puede ser nulo para un préstamo.");

            if (libros == null || !libros.Any())
                throw new ArgumentException("Debe prestar al menos un libro.");

            if (libros.Count > MAX_LIBROS_POR_PRESTAMO)
                throw new ArgumentException($"No se pueden prestar más de {MAX_LIBROS_POR_PRESTAMO} libros en un solo préstamo.");

            var librosUnicos = libros.Select(l => l.ISBN).Distinct().Count();
            if (librosUnicos != libros.Count)
                throw new ArgumentException("No se pueden prestar copias duplicadas del mismo libro en un préstamo.");

            var librosNoDisponibles = libros.Where(l => !l.Disponible).ToList();
            if (librosNoDisponibles.Any())
            {
                var titulos = string.Join(", ", librosNoDisponibles.Select(l => l.Titulo));
                throw new ArgumentException($"Los siguientes libros no están disponibles: {titulos}");
            }

            int librosActuales = alumno.LibrosPrestados.Count;
            int librosSolicitados = libros.Count;
            if (librosActuales + librosSolicitados > alumno.MaxLibrosPermitidos)
            {
                throw new ArgumentException($"El estudiante solo puede tener {alumno.MaxLibrosPermitidos} libros prestados. " +
                                          $"Actualmente tiene {librosActuales} y solicita {librosSolicitados} más.");
            }
        }

        public void Confirmar()
        {
            if (Estado != "Activo")
            {
                WriteLine($"Error: El préstamo {IdPréstamo} ya está en estado '{Estado}' y no puede ser confirmado.");
                return;
            }

            Estado = "Confirmado";
            WriteLine($" Préstamo {IdPréstamo} confirmado:");
            WriteLine($" Estudiante: {Alumno?.Nombre ?? IdEstudiante}");
            WriteLine($" Libros prestados ({LibrosPrestados?.Count ?? ISBNLibros?.Count ?? 0}):");

            if (LibrosPrestados != null && LibrosPrestados.Any())
            {
                foreach (var libro in LibrosPrestados)
                {
                    WriteLine($"    • '{libro.Titulo}' - ISBN: {libro.ISBN}");
                }
            }
            else if (ISBNLibros != null && ISBNLibros.Any())
            {
                WriteLine("    • (Libros pendientes de vincular)");
                foreach (var isbn in ISBNLibros)
                {
                    WriteLine($"      ISBN: {isbn}");
                }
            }
        }

        public void Cancelar()
        {
            if (Estado != "Activo")
            {
                WriteLine($"Error: El préstamo {IdPréstamo} no puede ser cancelado en estado '{Estado}'.");
                return;
            }

            Estado = "Cancelado";
            WriteLine($" Préstamo {IdPréstamo} cancelado.");
            WriteLine($" Se canceló el préstamo de {LibrosPrestados.Count} libro(s).");
        }

        public bool Renovar(int diasAdicionales = 7)
        {
            if (Estado != "Activo" && Estado != "Confirmado")
            {
                WriteLine($"Error: No se puede renovar un préstamo en estado '{Estado}'.");
                return false;
            }

            if (Renovaciones >= MAX_RENOVACIONES)
            {
                WriteLine($"Error: Este préstamo ya ha sido renovado {MAX_RENOVACIONES} veces (máximo permitido).");
                return false;
            }

            if (DiasVencidos() > 0)
            {
                WriteLine($"Error: No se puede renovar un préstamo vencido. Días vencidos: {DiasVencidos()}");
                return false;
            }

            FechaLimite = FechaLimite.AddDays(diasAdicionales);
            Renovaciones++;
            FechaUltimaRenovacion = DateTime.Now;

            WriteLine($" Préstamo {IdPréstamo} renovado exitosamente.");
            WriteLine($" Nueva fecha límite: {FechaLimite:dd/MM/yyyy}");
            WriteLine($" Renovaciones utilizadas: {Renovaciones}/{MAX_RENOVACIONES}");

            return true;
        }

        public int DiasVencidos()
        {
            if (FechaDevoluciónReal.HasValue)
            {
                return (FechaDevoluciónReal.Value > FechaLimite) ?
                       (FechaDevoluciónReal.Value - FechaLimite).Days : 0;
            }
            else
            {
                return (DateTime.Now > FechaLimite) ?
                       (DateTime.Now - FechaLimite).Days : 0;
            }
        }

        public int DiasRestantes()
        {
            if (Estado == "Devuelto" || Estado == "Cancelado")
                return 0;

            int dias = (FechaLimite - DateTime.Now).Days;
            return dias > 0 ? dias : 0;
        }

        public void RegistrarDevolución()
        {
            if (Estado != "Activo" && Estado != "Confirmado" && Estado != "Parcialmente Devuelto")
            {
                WriteLine($"Error: El préstamo {IdPréstamo} no está activo para ser devuelto (Estado: {Estado}).");
                return;
            }

            FechaDevoluciónReal = DateTime.Now;
            Estado = "Devuelto";

            int díasVencidos = DiasVencidos();
            int cantidadLibros = LibrosPrestados?.Count ?? ISBNLibros?.Count ?? 1;

            if (díasVencidos > 0)
            {
                Multa = díasVencidos * MULTA_POR_DIA * cantidadLibros;
                WriteLine($" Préstamo {IdPréstamo} devuelto con {díasVencidos} días de retraso.");
                WriteLine($" Multa generada: {Multa:C} ({díasVencidos} días × {MULTA_POR_DIA:C} × {cantidadLibros} libros)");
            }
            else
            {
                WriteLine($" Préstamo {IdPréstamo} devuelto a tiempo.");
            }

            if (díasVencidos > 0 && Estado == "Activo")
            {
                Estado = "Vencido";
            }
        }

        public bool RegistrarDevolucionParcial(List<string> isbnsADevolver)
        {
            if (Estado != "Activo" && Estado != "Confirmado" && Estado != "Parcialmente Devuelto")
            {
                WriteLine($"Error: El préstamo {IdPréstamo} no permite devoluciones parciales en estado '{Estado}'.");
                return false;
            }

            if (LibrosPrestados == null || !LibrosPrestados.Any())
            {
                WriteLine("Error: No hay libros vinculados para devolver.");
                return false;
            }

            var librosADevolver = LibrosPrestados.Where(l => isbnsADevolver.Contains(l.ISBN)).ToList();

            if (!librosADevolver.Any())
            {
                WriteLine("Error: No se encontraron libros válidos para devolver.");
                return false;
            }

            foreach (var isbn in isbnsADevolver)
            {
                if (!ISBNLibrosDevueltos.Contains(isbn))
                {
                    ISBNLibrosDevueltos.Add(isbn);
                }
            }

            if (ISBNLibrosDevueltos.Count == ISBNLibros.Count)
            {
                RegistrarDevolución();
            }
            else
            {
                Estado = "Parcialmente Devuelto";
                WriteLine($" Devolución parcial registrada. Libros devueltos: {ISBNLibrosDevueltos.Count}/{ISBNLibros.Count}");

                var librosPendientes = LibrosPrestados.Where(l => !ISBNLibrosDevueltos.Contains(l.ISBN)).ToList();
                WriteLine("Libros pendientes de devolver:");
                foreach (var libro in librosPendientes)
                {
                    WriteLine($"  • '{libro.Titulo}' - ISBN: {libro.ISBN}");
                }
            }

            return true;
        }

        public void VincularDatos(List<Estudiante> estudiantes, List<Libro> libros)
        {
            Alumno = estudiantes.FirstOrDefault(e => e.Identidad == IdEstudiante);

            LibrosPrestados.Clear();
            foreach (var isbn in ISBNLibros)
            {
                var libro = libros.FirstOrDefault(l => l.ISBN == isbn);
                if (libro != null)
                {
                    LibrosPrestados.Add(libro);
                }
            }
        }

        public void MostrarDetalle()
        {
            string infoDevolución = FechaDevoluciónReal.HasValue ?
                                   $" (Devuelto el: {FechaDevoluciónReal.Value:dd/MM/yyyy HH:mm})" : "";
            string infoMulta = (Multa > 0) ? $" (Multa: {Multa:C})" : "";
            string díasInfo = "";

            if ((Estado == "Activo" || Estado == "Confirmado") && !FechaDevoluciónReal.HasValue)
            {
                if (DiasVencidos() > 0)
                    díasInfo = $" VENCIDO por {DiasVencidos()} días";
                else
                    díasInfo = $" Restan {DiasRestantes()} días";
            }

            WriteLine($"\n{new string('-', 60)}");
            WriteLine($" DETALLES DEL PRÉSTAMO #{IdPréstamo}");
            WriteLine($"{new string('-', 60)}");
            WriteLine($"Estudiante: {Alumno?.Nombre ?? "N/A"} (ID: {IdEstudiante})");
            WriteLine($"Estado: {Estado}{infoDevolución}{infoMulta}");
            WriteLine($"Fecha de Préstamo: {FechaPréstamo:dd/MM/yyyy HH:mm}");
            WriteLine($"Fecha Límite: {FechaLimite:dd/MM/yyyy}");
            if (FechaUltimaRenovacion.HasValue)
                WriteLine($"Última Renovación: {FechaUltimaRenovacion:dd/MM/yyyy} (Renovaciones: {Renovaciones}/{MAX_RENOVACIONES})");
            WriteLine($"Estado temporal: {díasInfo}");

            WriteLine($"\n Libros prestados ({LibrosPrestados?.Count ?? 0}):");
            if (LibrosPrestados != null && LibrosPrestados.Any())
            {
                foreach (var libro in LibrosPrestados)
                {
                    string estadoLibro = ISBNLibrosDevueltos.Contains(libro.ISBN) ? " DEVUELTO" : "";
                    WriteLine($"  • '{libro?.Titulo ?? "N/A"}' - ISBN: {libro?.ISBN ?? "N/A"}{estadoLibro}");
                }
            }
            else if (ISBNLibros != null && ISBNLibros.Any())
            {
                WriteLine("  (Libros pendientes de vincular)");
                foreach (var isbn in ISBNLibros)
                {
                    string estadoLibro = ISBNLibrosDevueltos.Contains(isbn) ? " DEVUELTO" : "";
                    WriteLine($"  • ISBN: {isbn}{estadoLibro}");
                }
            }
            else
            {
                WriteLine("  No hay información de libros disponible");
            }

            if (!string.IsNullOrWhiteSpace(NotasAdicionales))
                WriteLine($"\n Notas: {NotasAdicionales}");

            WriteLine($"{new string('-', 60)}");
        }

        public decimal CalcularMultaPotencial()
        {
            if (Estado == "Devuelto" || Estado == "Cancelado")
                return Multa;

            int diasVencidos = DiasVencidos();
            if (diasVencidos > 0)
            {
                int librosPendientes = LibrosPrestados.Count - ISBNLibrosDevueltos.Count;
                return diasVencidos * MULTA_POR_DIA * librosPendientes;
            }

            return 0;
        }

        public bool PuedeSerRenovado()
        {
            return (Estado == "Activo" || Estado == "Confirmado") &&
                   Renovaciones < MAX_RENOVACIONES &&
                   DiasVencidos() == 0;
        }

        public string ObtenerResumenEstado()
        {
            if (Estado == "Devuelto")
                return $"Devuelto el {FechaDevoluciónReal:dd/MM/yyyy}";
            else if (Estado == "Cancelado")
                return "Cancelado";
            else if (Estado == "Parcialmente Devuelto")
                return $"Parcialmente devuelto ({ISBNLibrosDevueltos.Count}/{ISBNLibros.Count} libros)";
            else if (DiasVencidos() > 0)
                return $"VENCIDO ({DiasVencidos()} días) - Multa: {CalcularMultaPotencial():C}";
            else
                return $"Activo - Vence en {DiasRestantes()} días";
        }

        [JsonIgnore]
        public Libro LibroPrestado => LibrosPrestados.FirstOrDefault();

        [JsonIgnore]
        public string ISBMLibro => ISBNLibros.FirstOrDefault();
    }
}