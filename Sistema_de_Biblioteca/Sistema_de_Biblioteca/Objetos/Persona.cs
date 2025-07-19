using System;
using System.Text.RegularExpressions;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public abstract class Persona
    {
        private string nombre;
        public string Nombre
        {
            get => nombre;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío.");
                if (value.Length < 5)
                    throw new ArgumentException("El nombre debe tener al menos 5 caracteres.");
                if (value.Length > 40)
                    throw new ArgumentException("El nombre no puede exceder 40 caracteres.");
                nombre = value.Trim();
            }
        }

        public int Edad => CalcularEdad();

        private string identidad;
        public string Identidad
        {
            get => identidad;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La identidad no puede estar vacía.");
                if (value.Length < 13)
                    throw new ArgumentException("La identidad debe tener al menos 13 caracteres.");
                identidad = value.Trim();
            }
        }
        
        private string correoElectrónico;
        public string CorreoElectrónico
        {
            get => correoElectrónico;
            set
            {
                if (!IsValidEmail(value))
                    throw new ArgumentException("Formato de correo electrónico inválido.");
                correoElectrónico = value.Trim().ToLower();
            }
        }

        private string teléfono;
        public string Teléfono
        {
            get => teléfono;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El teléfono no puede estar vacío.");

                string telefonoLimpio = Regex.Replace(value, @"[^\d]", "");

                if (!Regex.IsMatch(telefonoLimpio, @"^\d{8,12}$"))
                    throw new ArgumentException("El teléfono debe contener entre 8 y 12 dígitos.");

                teléfono = telefonoLimpio;
            }
        }

        private DateTime fechaNacimiento;
        public DateTime FechaNacimiento
        {
            get => fechaNacimiento;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("La fecha de nacimiento no puede ser en el futuro.");

                if (value < new DateTime(1900, 1, 1))
                    throw new ArgumentException("La fecha de nacimiento no puede ser anterior al año 1900.");

                int edadCalculada = DateTime.Now.Year - value.Year;
                if (DateTime.Now < value.AddYears(edadCalculada))
                    edadCalculada--;

                if (edadCalculada < 7)
                    throw new ArgumentException("La persona debe tener al menos 7 años de edad.");

                if (edadCalculada > 90)
                    throw new ArgumentException("La edad no puede ser mayor a 90 años.");

                fechaNacimiento = value;
            }
        }

        private string dirección;
        public string Dirección
        {
            get => dirección;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La dirección no puede estar vacía.");
                if (value.Length < 5)
                    throw new ArgumentException("La dirección debe tener al menos 5 caracteres.");
                dirección = value.Trim();
            }
        }

        public Persona(string nombre, string identidad, string correoElectrónico,
                      string teléfono, DateTime fechaNacimiento, string dirección)
        {
            Nombre = nombre;
            Identidad = identidad;
            CorreoElectrónico = correoElectrónico;
            Teléfono = teléfono;
            FechaNacimiento = fechaNacimiento;
            Dirección = dirección;
        }

        public abstract void MostrarInformación();

        public int CalcularEdad()
        {
            int edad = DateTime.Now.Year - FechaNacimiento.Year;

            if (DateTime.Now < FechaNacimiento.AddYears(edad))
            {
                edad--;
            }
            return edad;
        }

        public int DiasHastaCumpleaños()
        {
            DateTime proximoCumpleaños = FechaNacimiento.AddYears(Edad + 1);
            if (proximoCumpleaños.Date < DateTime.Now.Date)
            {
                proximoCumpleaños = FechaNacimiento.AddYears(Edad + 2);
            }
            return (proximoCumpleaños.Date - DateTime.Now.Date).Days;
        }

        public bool EsMayorDeEdad(int edadMinima = 18)
        {
            return Edad >= edadMinima;
        }

        public bool ActualizarCorreo(string nuevoCorreo)
        {
            try
            {
                string correoAnterior = CorreoElectrónico;
                CorreoElectrónico = nuevoCorreo;
                WriteLine($" Correo actualizado de '{correoAnterior}' a '{nuevoCorreo}'");
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteLine($" Error al actualizar correo: {ex.Message}");
                return false;
            }
        }

        public bool ActualizarTeléfono(string nuevoTeléfono)
        {
            try
            {
                string telefonoAnterior = Teléfono;
                Teléfono = nuevoTeléfono;
                WriteLine($" Teléfono actualizado de '{telefonoAnterior}' a '{nuevoTeléfono}'");
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteLine($" Error al actualizar teléfono: {ex.Message}");
                return false;
            }
        }

        public bool ActualizarDirección(string nuevaDirección)
        {
            try
            {
                string direcciónAnterior = Dirección;
                Dirección = nuevaDirección;
                WriteLine($" Dirección actualizada de '{direcciónAnterior}' a '{nuevaDirección}'");
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteLine($" Error al actualizar dirección: {ex.Message}");
                return false;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public virtual string ObtenerResumen()
        {
            return $"{Nombre} (ID: {Identidad}, {Edad} años, {CorreoElectrónico})";
        }

        public override string ToString()
        {
            return ObtenerResumen();
        }
    }
}