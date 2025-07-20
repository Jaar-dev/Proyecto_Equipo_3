using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sistema_de_Biblioteca.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sistema_de_Biblioteca
{
    public class EstudianteJsonConverter : JsonConverter<Estudiante>
    {
        public override void WriteJson(JsonWriter writer, Estudiante value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            // Propiedades de Persona
            writer.WritePropertyName("Nombre");
            writer.WriteValue(value.Nombre);

            writer.WritePropertyName("Identidad");
            writer.WriteValue(value.Identidad);

            writer.WritePropertyName("CorreoElectrónico");
            writer.WriteValue(value.CorreoElectrónico);

            writer.WritePropertyName("Teléfono");
            writer.WriteValue(value.Teléfono);

            writer.WritePropertyName("FechaNacimiento");
            writer.WriteValue(value.FechaNacimiento);

            writer.WritePropertyName("Dirección");
            writer.WriteValue(value.Dirección);

            // Propiedad específica de Estudiante
            writer.WritePropertyName("Carrera");
            writer.WriteValue(value.Carrera);

            // No guardamos LibrosPrestados porque se reconstruye desde los préstamos

            writer.WriteEndObject();
        }

        public override Estudiante ReadJson(JsonReader reader, Type objectType, Estudiante existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jo = JObject.Load(reader);

                // Validar que tenemos todos los campos necesarios
                if (jo["Nombre"] == null || jo["Identidad"] == null || jo["CorreoElectrónico"] == null ||
                    jo["Teléfono"] == null || jo["FechaNacimiento"] == null || jo["Dirección"] == null ||
                    jo["Carrera"] == null)
                {
                    Console.WriteLine("Error: Faltan campos requeridos en el JSON del estudiante");
                    return null;
                }

                // Crear el estudiante con el constructor
                var estudiante = new Estudiante(
                    jo["Nombre"].ToString(),
                    jo["Identidad"].ToString(),
                    jo["CorreoElectrónico"].ToString(),
                    jo["Teléfono"].ToString(),
                    jo["FechaNacimiento"].ToObject<DateTime>(),
                    jo["Dirección"].ToString(),
                    jo["Carrera"].ToString()
                );

                // Los libros prestados se vincularán después mediante VincularDatos()

                return estudiante;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al deserializar estudiante: {ex.Message}");
                return null;
            }
        }
    }
}