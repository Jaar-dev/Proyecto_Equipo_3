using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sistema_de_Biblioteca.Objetos
{
    public class GestorPersistenciaJSON<T>
    {
        private readonly string rutaArchivo;

        public GestorPersistenciaJSON(string rutaCompletaArchivo)
        {
            string directorio = Path.GetDirectoryName(rutaCompletaArchivo);
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }
            this.rutaArchivo = rutaCompletaArchivo;
        }

        public void Guardar(List<T> datos)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                string json = JsonConvert.SerializeObject(datos, settings);
                File.WriteAllText(rutaArchivo, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar datos en {rutaArchivo}: {ex.Message}");
            }
        }

        public List<T> Cargar()
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                    return new List<T>();

                string json = File.ReadAllText(rutaArchivo);
                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        Console.WriteLine($"Error de deserialización: {args.ErrorContext.Error.Message}");
                        args.ErrorContext.Handled = true;
                    },
                    MissingMemberHandling = MissingMemberHandling.Error
                };

                if (typeof(T) == typeof(Empleado))
                    settings.Converters.Add(new EmpleadoJsonConverter());
                else if (typeof(T) == typeof(Estudiante))
                    settings.Converters.Add(new EstudianteJsonConverter());
                else if (typeof(T) == typeof(Libro))
                    settings.Converters.Add(new LibroJsonConverter());
                else if (typeof(T) == typeof(Préstamo))
                    settings.Converters.Add(new PrestamoJsonConverter());

                var resultado = JsonConvert.DeserializeObject<List<T>>(json, settings);

                // Filtrar elementos nulos (que fallaron en la deserialización)
                resultado = resultado?.Where(x => x != null).ToList() ?? new List<T>();

                if (resultado.Count == 0)
                    Console.WriteLine("Advertencia: No se pudieron cargar elementos válidos del archivo.");

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos de {rutaArchivo}: {ex.Message}");
                return new List<T>();
            }
        }
    }
}