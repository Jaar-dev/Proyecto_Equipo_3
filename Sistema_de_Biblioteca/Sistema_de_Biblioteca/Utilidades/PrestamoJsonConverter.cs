using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sistema_de_Biblioteca.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sistema_de_Biblioteca
{
    public class PrestamoJsonConverter : JsonConverter<Préstamo>
    {
        public override void WriteJson(JsonWriter writer, Préstamo value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("IdPréstamo");
            writer.WriteValue(value.IdPréstamo);

            writer.WritePropertyName("IdEstudiante");
            writer.WriteValue(value.IdEstudiante);

            writer.WritePropertyName("ISBNLibro");
            writer.WriteValue(value.ISBMLibro);

            writer.WritePropertyName("FechaPréstamo");
            writer.WriteValue(value.FechaPréstamo);

            writer.WritePropertyName("FechaLimite");
            writer.WriteValue(value.FechaLimite);

            writer.WritePropertyName("FechaDevoluciónReal");
            writer.WriteValue(value.FechaDevoluciónReal);

            writer.WritePropertyName("Estado");
            writer.WriteValue(value.Estado);

            writer.WritePropertyName("Multa");
            writer.WriteValue(value.Multa);

            writer.WriteEndObject();
        }

        public override Préstamo ReadJson(JsonReader reader, Type objectType, Préstamo existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jo = JObject.Load(reader);

                // Crear un préstamo vacío usando el constructor sin parámetros
                var préstamo = new Préstamo();

                // Usar reflexión para establecer las propiedades
                var type = typeof(Préstamo);

                if (jo["IdPréstamo"] != null)
                {
                    var propIdPrestamo = type.GetProperty("IdPréstamo");
                    propIdPrestamo.SetValue(préstamo, jo["IdPréstamo"].ToObject<int>());
                }

                if (jo["IdEstudiante"] != null)
                {
                    préstamo.IdEstudiante = jo["IdEstudiante"].ToString();
                }

                if (jo["ISBNLibro"] != null)
                {
                    préstamo.ISBMLibro = jo["ISBNLibro"].ToString();
                }
                else if (jo["ISBMLibro"] != null) // Por si acaso hay un typo en el JSON
                {
                    préstamo.ISBMLibro = jo["ISBMLibro"].ToString();
                }

                if (jo["FechaPréstamo"] != null)
                {
                    var propFechaPrestamo = type.GetProperty("FechaPréstamo");
                    propFechaPrestamo.SetValue(préstamo, jo["FechaPréstamo"].ToObject<DateTime>());
                }

                if (jo["FechaLimite"] != null)
                {
                    var propFechaLimite = type.GetProperty("FechaLimite");
                    propFechaLimite.SetValue(préstamo, jo["FechaLimite"].ToObject<DateTime>());
                }

                if (jo["FechaDevoluciónReal"] != null && jo["FechaDevoluciónReal"].Type != JTokenType.Null)
                {
                    préstamo.FechaDevoluciónReal = jo["FechaDevoluciónReal"].ToObject<DateTime?>();
                }

                if (jo["Estado"] != null)
                {
                    var propEstado = type.GetProperty("Estado");
                    propEstado.SetValue(préstamo, jo["Estado"].ToString());
                }

                if (jo["Multa"] != null)
                {
                    var propMulta = type.GetProperty("Multa");
                    propMulta.SetValue(préstamo, jo["Multa"].ToObject<decimal>());
                }

                // Los objetos Alumno y LibroPrestado se vincularán después mediante VincularDatos()

                return préstamo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al deserializar préstamo: {ex.Message}");
                return null;
            }
        }
    }
}