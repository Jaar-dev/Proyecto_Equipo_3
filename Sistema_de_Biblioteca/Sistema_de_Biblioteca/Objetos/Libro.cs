using Sistema_de_Biblioteca.Interfaces;
using System;
using System.Collections.Generic;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public class Libro : IPrestable, IValidable, IReporteable
    {
        public Libro() { }
        private string titulo;
        public string Titulo
        {
            get => titulo;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El título del libro no puede estar vacío.");
                if (value.Length < 2)
                    throw new ArgumentException("El título debe tener al menos 2 caracteres.");
                if (value.Length > 100)
                    throw new ArgumentException("El título no puede exceder 100 caracteres.");
                titulo = value.Trim();
            }
        }

        private string autor;
        public string Autor
        {
            get => autor;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El autor del libro no puede estar vacío.");
                if (value.Length < 3)
                    throw new ArgumentException("El nombre del autor debe tener al menos 3 caracteres.");
                if (value.Length > 50)
                    throw new ArgumentException("El nombre del autor no puede exceder 50 caracteres.");
                autor = value.Trim();
            }
        }

        private string isbn;
        public string ISBN
        {
            get => isbn;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El ISBN no puede estar vacío.");
                string isbnLimpio = value.Replace("-", "").Replace(" ", "");
                if (isbnLimpio.Length != 10 && isbnLimpio.Length != 13)
                    throw new ArgumentException("El ISBN debe tener 10 o 13 dígitos.");
                isbn = value.Trim();
            }
        }

        private string genero;
        public string Genero
        {
            get => genero;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El género del libro no puede estar vacío.");
                if (value.Length < 3)
                    throw new ArgumentException("El género debe tener al menos 3 caracteres.");
                genero = value.Trim();
            }
        }

        private int añoPublicación;
        public int AñoPublicación
        {
            get => añoPublicación;
            set
            {
                if (value < 1450)
                    throw new ArgumentException("El año de publicación no puede ser anterior a 1450.");
                if (value > DateTime.Now.Year + 1)
                    throw new ArgumentException($"El año de publicación no puede ser posterior a {DateTime.Now.Year + 1}.");
                añoPublicación = value;
            }
        }

        private string editorial;
        public string Editorial
        {
            get => editorial;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La editorial del libro no puede estar vacía.");
                if (value.Length < 2)
                    throw new ArgumentException("La editorial debe tener al menos 2 caracteres.");
                editorial = value.Trim();
            }
        }

        private int numeroCopiasTotales;
        public int NumeroCopiasTotales
        {
            get => numeroCopiasTotales;
            set
            {
                if (value < 0)
                    throw new ArgumentException("El número total de copias no puede ser negativo.");
                if (value > 1000)
                    throw new ArgumentException("El número de copias no puede exceder 1000.");
                numeroCopiasTotales = value;
            }
        }

        private int numeroCopiasDisponibles;
        public int NumeroCopiasDisponibles
        {
            get => numeroCopiasDisponibles;
            private set
            {
                if (value < 0 || value > NumeroCopiasTotales)
                    throw new ArgumentException("El número de copias disponibles es inválido.");
                numeroCopiasDisponibles = value;
            }
        }

        private string idioma = "Español";
        public string Idioma
        {
            get => idioma;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El idioma no puede estar vacío.");
                idioma = value.Trim();
            }
        }

        public bool Disponible => NumeroCopiasDisponibles > 0;

        public static int TotalLibrosCreados { get; private set; } = 0;

        public Libro(string titulo, string autor, string isbn, string genero, int añoPublicación, string editorial, int numeroCopiasTotales)
        {
            Titulo = titulo;
            Autor = autor;
            ISBN = isbn;
            Genero = genero;
            AñoPublicación = añoPublicación;
            Editorial = editorial;
            NumeroCopiasTotales = numeroCopiasTotales;
            NumeroCopiasDisponibles = numeroCopiasTotales;

            TotalLibrosCreados++;
        }

        public void MostrarInfo()
        {
            WriteLine($"--- Información del Libro ---");
            WriteLine($"Título: {Titulo}");
            WriteLine($"Autor: {Autor}");
            WriteLine($"ISBN: {ISBN}");
            WriteLine($"Género: {Genero},\n Año: {AñoPublicación}, Editorial: {Editorial},\n Idioma: {idioma}");
            WriteLine($"Copias Disponibles: {NumeroCopiasDisponibles}/{NumeroCopiasTotales}");
            WriteLine($"Estado: {(Disponible ? "Disponible" : "No Disponible")}");
        }

        public bool PrestarCopia()
        {
            if (NumeroCopiasDisponibles > 0)
            {
                NumeroCopiasDisponibles--;
                WriteLine($"Copia de '{Titulo}' prestada. Quedan {NumeroCopiasDisponibles} disponibles.");
                return true;
            }
            WriteLine($"No hay copias disponibles de '{Titulo}' para prestar.");
            return false;
        }

        public void RecibirCopia()
        {
            if (NumeroCopiasDisponibles < NumeroCopiasTotales)
            {
                NumeroCopiasDisponibles++;
                WriteLine($"Copia de '{Titulo}' recibida. Ahora hay {NumeroCopiasDisponibles} disponibles.");
            }
            else
            {
                WriteLine($"Error: Ya se han recibido todas las copias de '{Titulo}'.");
            }
        }
        public bool EstaDisponible()
        {
            return NumeroCopiasDisponibles > 0;
        }

        public bool RealizarPrestamo()
        {
            return PrestarCopia();
        }

        public bool RealizarDevolucion()
        {
            if (NumeroCopiasDisponibles < NumeroCopiasTotales)
            {
                RecibirCopia();
                return true;
            }
            return false;
        }

        public string ObtenerEstadoPrestamo()
        {
            if (NumeroCopiasDisponibles == NumeroCopiasTotales)
                return "Todas las copias disponibles";
            if (NumeroCopiasDisponibles == 0)
                return "Sin copias disponibles";
            return $"{NumeroCopiasDisponibles} de {NumeroCopiasTotales} copias disponibles";
        }

        public bool ValidarDatos()
        {
            var errores = ObtenerErroresValidacion();
            return errores.Count == 0;
        }

        public List<string> ObtenerErroresValidacion()
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(Titulo))
                errores.Add("El título está vacío");
            if (string.IsNullOrWhiteSpace(Autor))
                errores.Add("El autor está vacío");
            if (string.IsNullOrWhiteSpace(ISBN))
                errores.Add("El ISBN está vacío");
            if (NumeroCopiasDisponibles > NumeroCopiasTotales)
                errores.Add("Las copias disponibles exceden el total");
            if (AñoPublicación > DateTime.Now.Year + 1)
                errores.Add("El año de publicación es futuro");

            return errores;
        }

        public void GenerarReporte()
        {
            WriteLine("\n═══ REPORTE DE LIBRO ═══");
            MostrarInfo();
            WriteLine($"\nEstadísticas:");
            WriteLine($"• Copias prestadas: {NumeroCopiasTotales - NumeroCopiasDisponibles}");
        }

        public string ObtenerResumenActividad()
        {
            return $"{Titulo} - {Autor} ({ISBN}) - {ObtenerEstadoPrestamo()}";
        }

        public Dictionary<string, object> ObtenerEstadisticas()
        {
            return new Dictionary<string, object>
            {
                ["Título"] = Titulo,
                ["Autor"] = Autor,
                ["ISBN"] = ISBN,
                ["Copias Totales"] = NumeroCopiasTotales,
                ["Copias Disponibles"] = NumeroCopiasDisponibles,
                ["Copias Prestadas"] = NumeroCopiasTotales - NumeroCopiasDisponibles,
            };
        }
    }
}