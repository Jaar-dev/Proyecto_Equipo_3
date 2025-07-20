using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sistema_de_Biblioteca.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sistema_de_Biblioteca
{
    public class EmpleadoJsonConverter : JsonConverter<Empleado>
    {
        public override void WriteJson(JsonWriter writer, Empleado value, JsonSerializer serializer)
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

            // Propiedades específicas de Empleado
            writer.WritePropertyName("NumeroEmpleado");
            writer.WriteValue(value.NumeroEmpleado);

            writer.WritePropertyName("Puesto");
            writer.WriteValue(value.Puesto);

            writer.WritePropertyName("Turno");
            writer.WriteValue(value.Turno);

            writer.WritePropertyName("FechaContratación");
            writer.WriteValue(value.FechaContratación);

            writer.WritePropertyName("Salario");
            writer.WriteValue(value.Salario);

            writer.WritePropertyName("Tipo");
            writer.WriteValue(value.Tipo.ToString());

            writer.WriteEndObject();
        }

        public override Empleado ReadJson(JsonReader reader, Type objectType, Empleado existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jo = JObject.Load(reader);

                // Validar que tenemos todos los campos necesarios
                if (jo["Nombre"] == null || jo["Identidad"] == null || jo["CorreoElectrónico"] == null ||
                    jo["Teléfono"] == null || jo["FechaNacimiento"] == null || jo["Dirección"] == null ||
                    jo["Puesto"] == null || jo["Turno"] == null || jo["FechaContratación"] == null ||
                    jo["Salario"] == null)
                {
                    Console.WriteLine("Error: Faltan campos requeridos en el JSON del empleado");
                    return null;
                }

                // Parsear el tipo de empleado
                Empleado.TipoEmpleado tipo = Empleado.TipoEmpleado.Bibliotecario;
                if (jo["Tipo"] != null)
                {
                    if (!Enum.TryParse<Empleado.TipoEmpleado>(jo["Tipo"].ToString(), out tipo))
                    {
                        Console.WriteLine($"Advertencia: Tipo de empleado inválido '{jo["Tipo"]}', usando Bibliotecario por defecto");
                    }
                }

                // Crear el empleado con el constructor
                var empleado = new Empleado(
                    jo["Nombre"].ToString(),
                    jo["Identidad"].ToString(),
                    jo["CorreoElectrónico"].ToString(),
                    jo["Teléfono"].ToString(),
                    jo["FechaNacimiento"].ToObject<DateTime>(),
                    jo["Dirección"].ToString(),
                    jo["Puesto"].ToString(),
                    jo["Turno"].ToString(),
                    jo["FechaContratación"].ToObject<DateTime>(),
                    jo["Salario"].ToObject<decimal>(),
                    tipo
                );

                // Restaurar el número de empleado si existe
                if (jo["NumeroEmpleado"] != null)
                {
                    int numeroEmpleado = jo["NumeroEmpleado"].ToObject<int>();
                    // Usar reflexión para establecer el número de empleado
                    var propNumeroEmpleado = typeof(Empleado).GetProperty("NumeroEmpleado");
                    propNumeroEmpleado.SetValue(empleado, numeroEmpleado);
                }

                return empleado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al deserializar empleado: {ex.Message}");
                return null;
            }
        }
    }
}