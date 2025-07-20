using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sistema_de_Biblioteca.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sistema_de_Biblioteca
{
    public class LibroJsonConverter : JsonConverter<Libro>
    {
        public override void WriteJson(JsonWriter writer, Libro value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Titulo");
            writer.WriteValue(value.Titulo);

            writer.WritePropertyName("Autor");
            writer.WriteValue(value.Autor);

            writer.WritePropertyName("ISBN");
            writer.WriteValue(value.ISBN);

            writer.WritePropertyName("Genero");
            writer.WriteValue(value.Genero);

            writer.WritePropertyName("AñoPublicación");
            writer.WriteValue(value.AñoPublicación);

            writer.WritePropertyName("Editorial");
            writer.WriteValue(value.Editorial);

            writer.WritePropertyName("NumeroCopiasTotales");
            writer.WriteValue(value.NumeroCopiasTotales);

            writer.WritePropertyName("NumeroCopiasDisponibles");
            writer.WriteValue(value.NumeroCopiasDisponibles);

            writer.WritePropertyName("Idioma");
            writer.WriteValue(value.Idioma);

            writer.WriteEndObject();
        }

        public override Libro ReadJson(JsonReader reader, Type objectType, Libro existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jo = JObject.Load(reader);

                // Validar que tenemos todos los campos necesarios
                if (jo["Titulo"] == null || jo["Autor"] == null || jo["ISBN"] == null ||
                    jo["Genero"] == null || jo["AñoPublicación"] == null || jo["Editorial"] == null ||
                    jo["NumeroCopiasTotales"] == null)
                {
                    Console.WriteLine("Error: Faltan campos requeridos en el JSON del libro");
                    return null;
                }

                // Crear el libro con el constructor
                var libro = new Libro(
                    jo["Titulo"].ToString(),
                    jo["Autor"].ToString(),
                    jo["ISBN"].ToString(),
                    jo["Genero"].ToString(),
                    jo["AñoPublicación"].ToObject<int>(),
                    jo["Editorial"].ToString(),
                    jo["NumeroCopiasTotales"].ToObject<int>()
                );

                // Establecer el número de copias disponibles si existe
                if (jo["NumeroCopiasDisponibles"] != null)
                {
                    int copiasDisponibles = jo["NumeroCopiasDisponibles"].ToObject<int>();
                    // Usar reflexión para establecer las copias disponibles
                    var propCopiasDisponibles = typeof(Libro).GetProperty("NumeroCopiasDisponibles");
                    propCopiasDisponibles.SetValue(libro, copiasDisponibles);
                }

                // Establecer el idioma si existe
                if (jo["Idioma"] != null)
                {
                    libro.Idioma = jo["Idioma"].ToString();
                }

                return libro;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al deserializar libro: {ex.Message}");
                return null;
            }
        }
    }
}