using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public class Estudiante : Persona
    {
        private string carrera;
        public string Carrera
        {
            get => carrera;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La carrera no puede estar vacía.");
                if (value.Length < 3)
                    throw new ArgumentException("La carrera debe tener al menos 3 caracteres.");
                carrera = value.Trim();
            }
        }

        public List<Libro> LibrosPrestados { get; private set; } = new List<Libro>();

        public int MaxLibrosPermitidos
        {
            get
            {
                // Estudiantes menores tienen menos límite
                if (Edad < 16) return 2;
                if (Edad < 18) return 3;

                // Estudiantes universitarios pueden tener más libros
                if (Carrera.ToLower().Contains("postgrado") || Carrera.ToLower().Contains("maestría") || Carrera.ToLower().Contains("doctorado"))
                    return 5;

                return 3;
            }
        }

        public Estudiante(string nombre, string identidad, string correoElectrónico,
                         string teléfono, DateTime fechaNacimiento, string dirección, string carrera)
            : base(nombre, identidad, correoElectrónico, teléfono, fechaNacimiento, dirección)
        {
            Carrera = carrera;

            ValidarEdadParaCarrera();
        }

        public override void MostrarInformación()
        {
            WriteLine($"\n{new string('=', 45)}");
            WriteLine($"    INFORMACIÓN DEL ESTUDIANTE");
            WriteLine($"{new string('=', 45)}");
            WriteLine($"Nombre: {Nombre}");
            WriteLine($"Identidad: {Identidad}");
            WriteLine($"Edad: {Edad} años (Nacimiento: {FechaNacimiento:dd/MM/yyyy})");
            WriteLine($"Carrera: {Carrera}");
            WriteLine($"Nivel: {ObtenerNivelAcadémico()}");
            WriteLine($"Contacto: {CorreoElectrónico} | {Teléfono}");
            WriteLine($"Dirección: {Dirección}");
            WriteLine($"Estado académico: {(EsMayorDeEdad() ? "Universitario" : "Menor de edad")}");
            WriteLine($"\n  PRÉSTAMOS:");
            WriteLine($"Libros prestados: {LibrosPrestados.Count}/{MaxLibrosPermitidos}");
            WriteLine($"Capacidad disponible: {MaxLibrosPermitidos - LibrosPrestados.Count} libros");

            if (LibrosPrestados.Any())
            {
                WriteLine($"Libros actuales:");
                foreach (var libro in LibrosPrestados)
                {
                    WriteLine($"     {libro.Titulo} (ISBN: {libro.ISBN})");
                }
            }
            else
            {
                WriteLine($"     No tiene libros prestados actualmente");
            }

            WriteLine($"{new string('=', 45)}");
        }

        public bool PuedePrestar()
        {
            bool tieneCupo = LibrosPrestados.Count < MaxLibrosPermitidos;

            if (!tieneCupo)
            {
                WriteLine($"  {Nombre} ya tiene el máximo de {MaxLibrosPermitidos} libros prestados.");
                WriteLine($"   Debe devolver al menos un libro antes de prestar otro.");
            }
            return tieneCupo;
        }

        public bool AgregarLibroPrestado(Libro libro)
        {
            if (libro == null)
            {
                WriteLine("  Error: No se puede agregar un libro nulo a la lista de prestados.");
                return false;
            }

            if (LibrosPrestados.Contains(libro))
            {
                WriteLine($"  Error: El estudiante {Nombre} ya tiene prestado el libro '{libro.Titulo}'.");
                return false;
            }

            if (!PuedePrestar())
            {
                return false;
            }

            LibrosPrestados.Add(libro);
            WriteLine($"  Libro '{libro.Titulo}' agregado a los préstamos de {Nombre}.");
            WriteLine($"     Libros prestados: {LibrosPrestados.Count}/{MaxLibrosPermitidos}");
            return true;
        }

        public bool RemoverLibroPrestado(Libro libro)
        {
            if (libro == null)
            {
                WriteLine("  Error: No se puede remover un libro nulo de la lista de prestados.");
                return false;
            }

            if (!LibrosPrestados.Contains(libro))
            {
                WriteLine($"  Error: El estudiante {Nombre} no tiene prestado el libro '{libro.Titulo}'.");
                return false;
            }

            LibrosPrestados.Remove(libro);
            WriteLine($"  Libro '{libro.Titulo}' removido de los préstamos de {Nombre}.");
            WriteLine($"     Libros prestados: {LibrosPrestados.Count}/{MaxLibrosPermitidos}");
            return true;
        }

        public string ObtenerNivelAcadémico()
        {
            if (Edad < 12) return "Primaria";
            if (Edad < 15) return "Secundaria Básica";
            if (Edad < 18) return "Secundaria Superior";

            string carreraLower = Carrera.ToLower();

            if (carreraLower.Contains("doctorado") || carreraLower.Contains("phd"))
                return "Doctorado";
            if (carreraLower.Contains("maestría") || carreraLower.Contains("master"))
                return "Maestría";
            if (carreraLower.Contains("postgrado") || carreraLower.Contains("especialización"))
                return "Postgrado";

            return "Universitario";
        }

        private void ValidarEdadParaCarrera()
        {
            string nivel = ObtenerNivelAcadémico();

            if (nivel == "Doctorado" && Edad < 24)
            {
                WriteLine($"   Advertencia: {Nombre} es joven para un doctorado (edad: {Edad})");
            }
            else if (nivel == "Maestría" && Edad < 22)
            {
                WriteLine($"   Advertencia: {Nombre} es joven para maestría (edad: {Edad})");
            }
            else if (nivel == "Universitario" && Edad < 17)
            {
                WriteLine($"   Advertencia: {Nombre} es joven para universidad (edad: {Edad})");
            }
            else if (nivel == "Secundaria Superior" && Edad > 20)
            {
                WriteLine($"   Nota: {Nombre} está en secundaria a los {Edad} años");
            }
        }

        public bool PuedeGraduarse()
        {
            string nivel = ObtenerNivelAcadémico();

            if (nivel == "Primaria") return Edad >= 11;
            if (nivel == "Secundaria Básica") return Edad >= 14;
            if (nivel == "Secundaria Superior") return Edad >= 17;
            if (nivel == "Universitario") return Edad >= 21;
            if (nivel == "Postgrado") return Edad >= 23;
            if (nivel == "Maestría") return Edad >= 24;
            if (nivel == "Doctorado") return Edad >= 26;

            return true;
        }

        public List<string> ObtenerRecomendacionesPorEdad()
        {
            if (Edad < 12)
            {
                return new List<string> { "Literatura infantil", "Cuentos", "Libros ilustrados", "Ciencias básicas" };
            }
            else if (Edad < 15)
            {
                return new List<string> { "Literatura juvenil", "Historia", "Ciencias", "Biografías" };
            }
            else if (Edad < 18)
            {
                return new List<string> { "Literatura clásica", "Filosofía básica", "Ciencias avanzadas", "Preparación universitaria" };
            }
            else
            {
                return new List<string> { "Literatura especializada", "Textos académicos", "Investigación", "Referencias profesionales" };
            }
        }

        public double CalcularTiempoPromedioConLibros(List<Préstamo> historialPrestamos)
        {
            var prestamosEstudiante = historialPrestamos.Where(p => p.Alumno.Identidad == this.Identidad &&
                                                                  p.FechaDevoluciónReal.HasValue).ToList();

            if (!prestamosEstudiante.Any()) return 0;

            var tiempoTotal = prestamosEstudiante.Sum(p =>
                (p.FechaDevoluciónReal.Value - p.FechaPréstamo).TotalDays);

            return tiempoTotal / prestamosEstudiante.Count;
        }

        public override string ObtenerResumen()
        {
            return $"{Nombre} - {Carrera} ({Edad} años, {LibrosPrestados.Count}/{MaxLibrosPermitidos} libros)";
        }

        public bool EsBuenLector(List<Préstamo> historialPrestamos)
        {
            var prestamosEstudiante = historialPrestamos.Count(p => p.Alumno.Identidad == this.Identidad);
            int librosEsperados = Edad < 18 ? Edad - 10 : (Edad - 17) * 2;
            return prestamosEstudiante >= Math.Max(librosEsperados, 1);
        }
    }
}