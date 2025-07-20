using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
namespace Sistema_de_Biblioteca.Objetos
{
    public class Préstamo
    {
        public int IdPréstamo { get; private set; }
        public string IdEstudiante { get; set; }
        public string ISBMLibro { get; set; }
        [JsonIgnore]
        public Estudiante Alumno { get; private set; }
        public Libro LibroPrestado { get; private set; }
        [JsonIgnore]
        public DateTime FechaPréstamo { get; private set; }
        [JsonIgnore]
        public DateTime FechaLimite { get; private set; }
        [JsonIgnore]
        public DateTime? FechaDevoluciónReal { get; set; }
        [JsonIgnore]
        public string Estado { get; private set; }
        [JsonIgnore]
        public decimal Multa { get; private set; }

        private static int nextIdPréstamo = 1;
        public static int TotalPrestamos { get; private set; } = 0;
        public Préstamo() { }

        public Préstamo(Estudiante alumno, Libro libro)
        {
            if (alumno == null)
            {
                throw new ArgumentNullException(nameof(alumno), "El estudiante no puede ser nulo para un préstamo.");
            }
            if (libro == null)
            {
                throw new ArgumentNullException(nameof(libro), "El libro no puede ser nulo para un préstamo.");
            }

            IdPréstamo = nextIdPréstamo++;
            Alumno = alumno;
            LibroPrestado = libro;
            IdEstudiante = alumno.Identidad;
            ISBMLibro = libro.ISBN;
            FechaPréstamo = DateTime.Now;
            FechaLimite = FechaPréstamo.AddDays(7);
            Estado = "Activo";
            Multa = 0m;
            FechaDevoluciónReal = null;

            TotalPrestamos++;
        }

        public void Confirmar()
        {
            if (Estado == "Activo")
            {
                Estado = "Confirmado";
                WriteLine($"Préstamo {IdPréstamo} confirmado: '{LibroPrestado.Titulo}' para {Alumno.Nombre}");
            }
            else
            {
                WriteLine($"El préstamo {IdPréstamo} ya está en estado '{Estado}' y no puede ser confirmado.");
            }
        }

        public void Cancelar()
        {
            if (Estado == "Activo")
            {
                Estado = "Cancelado";
                WriteLine($"Préstamo {IdPréstamo} cancelado: '{LibroPrestado.Titulo}'.");
            }
            else
            {
                WriteLine($"El préstamo {IdPréstamo} no puede ser cancelado en estado '{Estado}'.");
            }
        }

        public int DiasVencidos()
        {
            if (FechaDevoluciónReal.HasValue)
            {
                return (FechaDevoluciónReal.Value > FechaLimite) ? (FechaDevoluciónReal.Value - FechaLimite).Days : 0;
            }
            else
            {
                return (DateTime.Now > FechaLimite) ? (DateTime.Now - FechaLimite).Days : 0;
            }
        }

        public void RegistrarDevolución()
        {
            if (Estado != "Activo")
            {
                WriteLine($"Error: El préstamo {IdPréstamo} no está activo para ser devuelto (Estado: {Estado}).");
                return;
            }

            FechaDevoluciónReal = DateTime.Now;
            Estado = "Devuelto";

            int díasVencidos = DiasVencidos();
            if (díasVencidos > 0)
            {
                Multa = díasVencidos * 0.50m;
                WriteLine($"Libro '{LibroPrestado.Titulo}' devuelto con {díasVencidos} días de retraso. Multa generada: {Multa:C}.");
            }
            else
            {
                WriteLine($"Libro '{LibroPrestado.Titulo}' devuelto a tiempo.");
            }
        }
        public void VincularDatos(List<Estudiante> estudiantes, List<Libro> libros)
        {
            Alumno = estudiantes.FirstOrDefault(e => e.Identidad == IdEstudiante);
            LibroPrestado = libros.FirstOrDefault(l => l.ISBN == ISBMLibro);
        }

        public void MostrarDetalle()
        {
            string infoDevolución = FechaDevoluciónReal.HasValue ? $" (Devuelto el: {FechaDevoluciónReal.Value.ToShortDateString()})" : "";
            string infoMulta = (Multa > 0) ? $" (Multa: {Multa:C})" : "";
            string díasInfo = (Estado == "Activo" && !FechaDevoluciónReal.HasValue) ?
                              (DiasVencidos() > 0 ? $"Vencido por {DiasVencidos()} días" : $"Restan {(FechaLimite - DateTime.Now).Days} días") : "";

            WriteLine($"\n--- Detalles del Préstamo {IdPréstamo} ---");
            WriteLine($"Estudiante: {Alumno.Nombre} (ID: {Alumno.Identidad})");
            WriteLine($"Libro: '{LibroPrestado.Titulo}' (ISBN: {LibroPrestado.ISBN})");
            WriteLine($"Fecha de Préstamo: {FechaPréstamo.ToShortDateString()}");
            WriteLine($"Fecha Límite: {FechaLimite.ToShortDateString()}");
            WriteLine($"Estado: {Estado}{infoDevolución}{infoMulta} {díasInfo}");
        }
    }
}