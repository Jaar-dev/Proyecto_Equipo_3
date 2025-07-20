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

            writer.WritePropertyName("ISBNLibros");
            serializer.Serialize(writer, value.ISBNLibros);

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

            writer.WritePropertyName("Renovaciones");
            writer.WriteValue(value.Renovaciones);

            writer.WritePropertyName("NotasAdicionales");
            writer.WriteValue(value.NotasAdicionales);

            writer.WritePropertyName("FechaUltimaRenovacion");
            writer.WriteValue(value.FechaUltimaRenovacion);

            writer.WritePropertyName("ISBNLibrosDevueltos");
            serializer.Serialize(writer, value.ISBNLibrosDevueltos);

            if (value.ISBNLibros != null && value.ISBNLibros.Any())
            {
                writer.WritePropertyName("ISBMLibro");
                writer.WriteValue(value.ISBNLibros.First());
            }

            writer.WriteEndObject();
        }

        public override Préstamo ReadJson(JsonReader reader, Type objectType, Préstamo existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jo = JObject.Load(reader);

                var préstamo = new Préstamo();

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

                // Manejar ISBNLibros - puede ser una lista o un string (compatibilidad)
                if (jo["ISBNLibros"] != null)
                {
                    if (jo["ISBNLibros"].Type == JTokenType.Array)
                    {
                        préstamo.ISBNLibros = jo["ISBNLibros"].ToObject<List<string>>();
                    }
                    else if (jo["ISBNLibros"].Type == JTokenType.String)
                    {
                        // Compatibilidad: si es un string, convertirlo a lista
                        préstamo.ISBNLibros = new List<string> { jo["ISBNLibros"].ToString() };
                    }
                }
                else if (jo["ISBMLibro"] != null || jo["ISBNLibro"] != null)
                {
                    string isbn = jo["ISBMLibro"]?.ToString() ?? jo["ISBNLibro"]?.ToString();
                    if (!string.IsNullOrEmpty(isbn))
                    {
                        préstamo.ISBNLibros = new List<string> { isbn };
                    }
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

                if (jo["Renovaciones"] != null)
                {
                    var propRenovaciones = type.GetProperty("Renovaciones");
                    propRenovaciones.SetValue(préstamo, jo["Renovaciones"].ToObject<int>());
                }

                if (jo["NotasAdicionales"] != null)
                {
                    préstamo.NotasAdicionales = jo["NotasAdicionales"].ToString();
                }

                if (jo["FechaUltimaRenovacion"] != null && jo["FechaUltimaRenovacion"].Type != JTokenType.Null)
                {
                    var propFechaRenovacion = type.GetProperty("FechaUltimaRenovacion");
                    propFechaRenovacion.SetValue(préstamo, jo["FechaUltimaRenovacion"].ToObject<DateTime?>());
                }

                if (jo["ISBNLibrosDevueltos"] != null)
                {
                    if (jo["ISBNLibrosDevueltos"].Type == JTokenType.Array)
                    {
                        préstamo.ISBNLibrosDevueltos = jo["ISBNLibrosDevueltos"].ToObject<List<string>>();
                    }
                    else if (jo["ISBNLibrosDevueltos"].Type == JTokenType.String)
                    {
                        préstamo.ISBNLibrosDevueltos = new List<string> { jo["ISBNLibrosDevueltos"].ToString() };
                    }
                }

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