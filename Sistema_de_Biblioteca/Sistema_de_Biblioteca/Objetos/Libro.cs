using System;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public class Libro
    {
        private string titulo;
        public string Titulo
        {
            get => titulo;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("El título del libro no puede estar vacío.");
                titulo = value;
            }
        }

        private string autor;
        public string Autor
        {
            get => autor;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("El autor del libro no puede estar vacío.");
                autor = value;
            }
        }

        private string isbn;
        public string ISBN
        {
            get => isbn;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El ISBN no puede estar vacío. ");
                isbn = value;
            }
        }

        private string genero;
        public string Genero
        {
            get => genero;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("El género del libro no puede estar vacío.");
                genero = value;
            }
        }

        private int añoPublicación;
        public int AñoPublicación
        {
            get => añoPublicación;
            set
            {
                if (value <= 0 || value > DateTime.Now.Year) throw new ArgumentException("El año de publicación es inválido.");
                añoPublicación = value;
            }
        }

        private string editorial;
        public string Editorial
        {
            get => editorial;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("La editorial del libro no puede estar vacía.");
                editorial = value;
            }
        }

        private int numeroCopiasTotales;
        public int NumeroCopiasTotales
        {
            get => numeroCopiasTotales;
            set
            {
                if (value < 0) throw new ArgumentException("El número total de copias no puede ser negativo.");
                numeroCopiasTotales = value;
            }
        }

        private int numeroCopiasDisponibles;
        public int NumeroCopiasDisponibles
        {
            get => numeroCopiasDisponibles;
            private set
            {
                if (value < 0 || value > NumeroCopiasTotales) throw new ArgumentException("El número de copias disponibles es inválido.");
                numeroCopiasDisponibles = value;
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
            WriteLine($"Género: {Genero}, Año: {AñoPublicación}, Editorial: {Editorial}");
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
    }
}