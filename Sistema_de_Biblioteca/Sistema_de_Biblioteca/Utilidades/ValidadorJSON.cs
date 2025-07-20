using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sistema_de_Biblioteca.Objetos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Console;

namespace Sistema_de_Biblioteca.Utilidades
{
    public static class ValidadorJSON
    {
        public static bool ValidarArchivoEmpleados(string rutaArchivo)
        {
            try
            {
                WriteLine($"\nValidando archivo: {rutaArchivo}");

                if (!File.Exists(rutaArchivo))
                {
                    WriteLine("El archivo no existe.");
                    return false;
                }

                string json = File.ReadAllText(rutaArchivo);

                // Validar que sea JSON válido
                try
                {
                    var jArray = JArray.Parse(json);

                    bool hayErrores = false;
                    int índice = 0;

                    foreach (var empleado in jArray)
                    {
                        índice++;
                        var errores = ValidarEmpleado(empleado, índice);
                        if (errores.Any())
                        {
                            hayErrores = true;
                            WriteLine($"\nErrores en empleado #{índice}:");
                            foreach (var error in errores)
                            {
                                WriteLine($" • {error}");
                            }
                        }
                    }

                    if (!hayErrores)
                    {
                        WriteLine("✓ El archivo es válido y todos los empleados tienen datos correctos.");
                        return true;
                    }
                }
                catch (JsonReaderException ex)
                {
                    WriteLine($"El archivo no contiene JSON válido: {ex.Message}");
                }

                return false;
            }
            catch (Exception ex)
            {
                WriteLine($"Error al validar archivo: {ex.Message}");
                return false;
            }
        }

        private static string[] ValidarEmpleado(JToken empleado, int índice)
        {
            var errores = new List<string>();

            // Validar propiedades requeridas
            var propiedadesRequeridas = new[]
            {
                "Nombre", "Identidad", "CorreoElectrónico", "Teléfono",
                "FechaNacimiento", "Dirección", "Puesto", "Turno",
                "FechaContratación", "Salario"
            };

            foreach (var propiedad in propiedadesRequeridas)
            {
                if (empleado[propiedad] == null || string.IsNullOrWhiteSpace(empleado[propiedad].ToString()))
                {
                    errores.Add($"Falta la propiedad requerida: {propiedad}");
                }
            }

            // Validar fechas
            if (empleado["FechaNacimiento"] != null)
            {
                if (!DateTime.TryParse(empleado["FechaNacimiento"].ToString(), out DateTime fechaNac))
                    errores.Add("Fecha de nacimiento inválida");
            }

            if (empleado["FechaContratación"] != null)
            {
                if (!DateTime.TryParse(empleado["FechaContratación"].ToString(), out DateTime fechaCont))
                    errores.Add("Fecha de contratación inválida");
            }

            // Validar salario
            if (empleado["Salario"] != null)
            {
                if (!decimal.TryParse(empleado["Salario"].ToString(), out decimal salario) || salario <= 0)
                    errores.Add("Salario debe ser un número positivo");
            }

            // Validar tipo de empleado
            if (empleado["Tipo"] != null)
            {
                string tipo = empleado["Tipo"].ToString();
                if (!Enum.TryParse<Empleado.TipoEmpleado>(tipo, out _))
                    errores.Add($"Tipo de empleado inválido: {tipo}");
            }

            return errores.ToArray();
        }
    }
}