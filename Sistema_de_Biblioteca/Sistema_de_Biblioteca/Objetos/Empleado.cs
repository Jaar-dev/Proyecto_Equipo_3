using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public class Empleado : Persona
    {
        private static int contadorEmpleados = 0;
        private static readonly Dictionary<string, decimal> salariosMinimosPorPuesto = new Dictionary<string, decimal>
        {
            { "bibliotecario", 15000m },
            { "supervisor", 20000m },
            { "administrador", 25000m },
            { "auxiliar", 13000m },
            { "jefe", 30000m }
        };

        public int NumeroEmpleado { get; private set; }

        private string puesto;
        public string Puesto
        {
            get => puesto;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El puesto del empleado no puede estar vacío.");
                if (value.Length < 3)
                    throw new ArgumentException("El puesto debe tener al menos 3 caracteres.");
                if (value.Length > 50)
                    throw new ArgumentException("El puesto no puede exceder 50 caracteres.");
                if (!Regex.IsMatch(value, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                    throw new ArgumentException("El puesto solo puede contener letras y espacios.");
                puesto = value.Trim();
            }
        }

        private string turno;
        public string Turno
        {
            get => turno;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El turno del empleado no puede estar vacío.");

                string[] turnosValidos = { "Matutino", "Vespertino", "Nocturno", "Completo", "Mixto" };
                string turnoCapitalizado = char.ToUpper(value[0]) + value.Substring(1).ToLower();

                if (!turnosValidos.Contains(turnoCapitalizado))
                    throw new ArgumentException($"El turno debe ser uno de los siguientes: {string.Join(", ", turnosValidos)}");

                turno = turnoCapitalizado;
            }
        }

        private DateTime fechaContratación;
        public DateTime FechaContratación
        {
            get => fechaContratación;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("La fecha de contratación no puede ser en el futuro.");
                if (value < new DateTime(1980, 1, 1))
                    throw new ArgumentException("La fecha de contratación no puede ser anterior a 1980.");
                if (value < FechaNacimiento.AddYears(18))
                    throw new ArgumentException("La fecha de contratación no puede ser antes de los 18 años de edad.");

                if (value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday)
                    WriteLine(" Advertencia: La fecha de contratación cae en fin de semana.");

                fechaContratación = value;
            }
        }

        private decimal salario;
        public decimal Salario
        {
            get => salario;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("El salario debe ser un valor positivo.");
                if (value < 13000m)
                    throw new ArgumentException("El salario no puede ser menor al salario mínimo (L. 13,000).");
                if (value > 200000m)
                    throw new ArgumentException("El salario no puede exceder L. 200,000.");

                ValidarSalarioPorPuesto(value);
                salario = value;
            }
        }

        public enum TipoEmpleado
        {
            Bibliotecario,
            Supervisor,
            Administrador
        }

        private TipoEmpleado tipo;
        public TipoEmpleado Tipo
        {
            get => tipo;
            set
            {
                if (!string.IsNullOrEmpty(Puesto))
                    ValidarTipoPuesto(value, Puesto);
                tipo = value;
            }
        }

        public bool EstaActivo { get; private set; } = true;
        public DateTime? FechaBaja { get; private set; }
        public string MotivoBaja { get; private set; }
        public List<string> HistorialActividades { get; private set; } = new List<string>();

        public static int TotalEmpleadosContratados => contadorEmpleados;

        public Empleado() : base() { }

        public Empleado(string nombre, string identidad, string correoElectrónico,
                       string teléfono, DateTime fechaNacimiento, string dirección,
                       string puesto, string turno, DateTime fechaContratación,
                       decimal salario, TipoEmpleado tipo = TipoEmpleado.Bibliotecario)
            : base(nombre, identidad, correoElectrónico, teléfono, fechaNacimiento, dirección)
        {
            if (fechaContratación < fechaNacimiento.AddYears(18))
                throw new ArgumentException("El empleado debe tener al menos 18 años al momento de la contratación.");

            contadorEmpleados++;
            NumeroEmpleado = contadorEmpleados;

            Puesto = puesto;
            Turno = turno;
            FechaContratación = fechaContratación;
            Tipo = tipo;
            Salario = salario;

            RegistrarActividad($"Empleado contratado con número: {NumeroEmpleado}");
            WriteLine($"\n✓ Empleado contratado exitosamente con número: {NumeroEmpleado}");
        }

        private void ValidarSalarioPorPuesto(decimal salarioAValidar)
        {
            string puestoLower = Puesto?.ToLower() ?? "";
            foreach (var kvp in salariosMinimosPorPuesto)
            {
                if (puestoLower.Contains(kvp.Key) && salarioAValidar < kvp.Value)
                {
                    throw new ArgumentException($"El salario mínimo para un {kvp.Key} es L. {kvp.Value:N2}");
                }
            }
        }

        private void ValidarTipoPuesto(TipoEmpleado tipoEmpleado, string puestoEmpleado)
        {
            if (string.IsNullOrEmpty(puestoEmpleado))
                return;

            string puestoLower = puestoEmpleado.ToLower();

            if (tipoEmpleado == TipoEmpleado.Administrador &&
                !puestoLower.Contains("admin") &&
                !puestoLower.Contains("jefe") &&
                !puestoLower.Contains("director") &&
                !puestoLower.Contains("gerente"))
            {
                WriteLine(" Advertencia: El tipo 'Administrador' generalmente corresponde a puestos de dirección.");
            }

            if (tipoEmpleado == TipoEmpleado.Bibliotecario &&
                (puestoLower.Contains("jefe") ||
                 puestoLower.Contains("director") ||
                 puestoLower.Contains("gerente")))
            {
                WriteLine(" Advertencia: Un puesto directivo generalmente no es tipo 'Bibliotecario'.");
            }
        }

        public override void MostrarInformación()
        {
            WriteLine($"\n{new string('=', 50)}");
            WriteLine($"    INFORMACIÓN DEL EMPLEADO");
            WriteLine($"{new string('=', 50)}");
            WriteLine($"Número de Empleado: {NumeroEmpleado}");
            WriteLine($"Nombre: {Nombre}");
            WriteLine($"Identidad: {Identidad}");
            WriteLine($"Edad: {Edad} años (Nacimiento: {FechaNacimiento:dd/MM/yyyy})");
            WriteLine($"Puesto: {Puesto} ({Tipo})");
            WriteLine($"Turno: {Turno}");
            WriteLine($"Estado: {(EstaActivo ? "Activo" : $"Inactivo desde {FechaBaja:dd/MM/yyyy}")}");
            WriteLine($"Contacto: {CorreoElectrónico} | {Teléfono}");
            WriteLine($"Dirección: {Dirección}");
            WriteLine($"Fecha Contratación: {FechaContratación:dd/MM/yyyy}");
            WriteLine($"Antigüedad: {CalcularAntiguedad()}");
            WriteLine($"Salario: {Salario:C}");
            WriteLine($"En horario laboral: {(EstaEnHorarioLaboral() ? "Sí" : "No")}");
            WriteLine($"{new string('=', 50)}");
        }

        public bool RegistrarNuevoLibro(List<Libro> libros, Libro nuevoLibro)
        {
            if (!PuedeRealizarOperación("agregar_libro"))
            {
                WriteLine($"Error: {Nombre} ({Tipo}) no tiene permisos para registrar libros.");
                return false;
            }

            if (nuevoLibro == null)
            {
                WriteLine("Error: No se puede registrar un libro nulo.");
                return false;
            }

            if (libros.Any(l => l.ISBN.Equals(nuevoLibro.ISBN, StringComparison.OrdinalIgnoreCase)))
            {
                WriteLine($"Error: Ya existe un libro con ISBN '{nuevoLibro.ISBN}'.");
                return false;
            }

            libros.Add(nuevoLibro);
            RegistrarActividad($"Registró el libro: '{nuevoLibro.Titulo}' (ISBN: {nuevoLibro.ISBN})");
            WriteLine($" {Nombre} ha registrado el nuevo libro: '{nuevoLibro.Titulo}'");
            return true;
        }

        public bool ProcesarDevolución(Préstamo préstamo)
        {
            if (!PuedeRealizarOperación("devolucion"))
            {
                WriteLine($"Error: {Nombre} ({Tipo}) no tiene permisos para procesar devoluciones.");
                return false;
            }

            if (préstamo == null)
            {
                WriteLine("Error: No se puede procesar una devolución de préstamo nula.");
                return false;
            }

            if (préstamo.Estado == "Devuelto")
            {
                WriteLine($"El préstamo {préstamo.IdPréstamo} ya ha sido devuelto.");
                return false;
            }

            if (préstamo.Estado == "Cancelado")
            {
                WriteLine($"Error: El préstamo {préstamo.IdPréstamo} fue cancelado y no puede ser devuelto.");
                return false;
            }

            préstamo.RegistrarDevolución();
            RegistrarActividad($"Procesó devolución del préstamo {préstamo.IdPréstamo}");
            WriteLine($" {Nombre} ha procesado la devolución del préstamo {préstamo.IdPréstamo}.");

            if (préstamo.Multa > 0)
            {
                WriteLine($" Multa generada: {préstamo.Multa:C} por {préstamo.DiasVencidos()} días de retraso.");
            }

            return true;
        }

        public bool GestionarMulta(Préstamo préstamo, bool pagada = false)
        {
            if (!PuedeRealizarOperación("gestionar_multas"))
            {
                WriteLine($"Error: {Nombre} ({Tipo}) no tiene permisos para gestionar multas.");
                return false;
            }

            if (préstamo?.Multa <= 0)
            {
                WriteLine("No hay multa que gestionar para este préstamo.");
                return false;
            }

            if (pagada)
            {
                RegistrarActividad($"Marcó como pagada la multa de {préstamo.Multa:C} del préstamo {préstamo.IdPréstamo}");
                WriteLine($" Multa de {préstamo.Multa:C} marcada como pagada por {Nombre}");
                return true;
            }
            else
            {
                WriteLine($" Multa pendiente: {préstamo.Multa:C} - Préstamo {préstamo.IdPréstamo}");
                return false;
            }
        }

        public void GenerarReporteBásico(string tipoReporte)
        {
            if (!PuedeRealizarOperación("generar_reportes"))
            {
                WriteLine($"Error: {Nombre} ({Tipo}) no tiene permisos para generar reportes.");
                return;
            }

            RegistrarActividad($"Generó reporte básico: {tipoReporte}");
            WriteLine($"\n {Nombre} está generando un reporte de tipo: {tipoReporte}");
            WriteLine($"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        }

        public void GenerarReporteCompleto(List<Libro> libros, List<Préstamo> prestamos,
                                         List<Estudiante> estudiantes, string tipoReporte = "General")
        {
            if (!PuedeRealizarOperación("generar_reportes"))
            {
                WriteLine($"Error: {Nombre} ({Tipo}) no tiene permisos para generar reportes completos.");
                return;
            }

            string separador = new string('=', 60);

            WriteLine($"\n{separador}");
            WriteLine($" REPORTE {tipoReporte.ToUpper()} - SISTEMA DE BIBLIOTECA");
            WriteLine($"Generado por: {Nombre} ({Tipo}) - Empleado #{NumeroEmpleado}");
            WriteLine($"Fecha: {DateTime.Now:dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm:ss}");
            WriteLine($"{separador}");

            WriteLine($"\n  INVENTARIO DE LIBROS:");
            WriteLine($"   • Total de títulos registrados: {libros.Count}");
            WriteLine($"   • Total de copias en biblioteca: {libros.Sum(l => l.NumeroCopiasTotales)}");
            WriteLine($"   • Copias disponibles: {libros.Sum(l => l.NumeroCopiasDisponibles)}");
            WriteLine($"   • Copias prestadas: {libros.Sum(l => l.NumeroCopiasTotales - l.NumeroCopiasDisponibles)}");

            WriteLine($"\n   ESTUDIANTES:");
            WriteLine($"   • Total de estudiantes registrados: {estudiantes.Count}");
            WriteLine($"   • Estudiantes con préstamos activos: {estudiantes.Count(e => e.LibrosPrestados.Any())}");
            WriteLine($"   • Promedio de edad: {(estudiantes.Any() ? estudiantes.Average(e => e.Edad).ToString("F1") : "N/A")} años");

            WriteLine($"\n   PRÉSTAMOS:");
            WriteLine($"   • Total de préstamos realizados: {prestamos.Count}");
            WriteLine($"   • Préstamos activos: {prestamos.Count(p => p.Estado == "Activo" || p.Estado == "Confirmado")}");
            WriteLine($"   • Préstamos devueltos: {prestamos.Count(p => p.Estado == "Devuelto")}");
            WriteLine($"   • Préstamos cancelados: {prestamos.Count(p => p.Estado == "Cancelado")}");
            WriteLine($"   • Préstamos vencidos: {prestamos.Count(p => (p.Estado == "Activo" || p.Estado == "Confirmado") && p.DiasVencidos() > 0)}");

            var multaTotal = prestamos.Where(p => p.Multa > 0).Sum(p => p.Multa);
            if (multaTotal > 0)
            {
                WriteLine($"\n   MULTAS:");
                WriteLine($"   • Total de multas generadas: {multaTotal:C}");
                WriteLine($"   • Préstamos con multa: {prestamos.Count(p => p.Multa > 0)}");
                WriteLine($"   • Multa promedio: {prestamos.Where(p => p.Multa > 0).Average(p => p.Multa):C}");
            }

            if (prestamos.Any())
            {
                var topLibros = prestamos.Where(p => p.LibrosPrestados != null && p.LibrosPrestados.Any())
                                        .SelectMany(p => p.LibrosPrestados.Select(libro => new { Prestamo = p, Libro = libro }))
                                        .Where(x => x.Libro != null)
                                        .GroupBy(x => x.Libro.ISBN)
                                        .Select(g => new
                                        {
                                            ISBN = g.Key,
                                            Titulo = g.First().Libro.Titulo,
                                            Cantidad = g.Count()
                                        })
                                        .OrderByDescending(x => x.Cantidad)
                                        .Take(5);

                if (topLibros.Any())
                {
                    WriteLine($"\n   TOP 5 LIBROS MÁS PRESTADOS:");
                    int posicion = 1;
                    foreach (var item in topLibros)
                    {
                        WriteLine($"   {posicion}. '{item.Titulo}' - {item.Cantidad} préstamos");
                        posicion++;
                    }
                }
            }

            WriteLine($"\n   EMPLEADOS:");
            WriteLine($"   • Total de empleados: {TotalEmpleadosContratados}");

            WriteLine($"\n{separador}");
            WriteLine($"Fin del reporte - {DateTime.Now:HH:mm:ss}");

            RegistrarActividad($"Generó reporte completo: {tipoReporte}");
        }

        public bool PuedeRealizarOperación(string tipoOperación)
        {
            if (!EstaActivo)
            {
                WriteLine($"Error: El empleado {Nombre} no está activo.");
                return false;
            }

            string operacion = tipoOperación.ToLower();

            if (operacion == "prestamo" || operacion == "devolucion" ||
                operacion == "buscar" || operacion == "listar")
            {
                return true;
            }

            if (operacion == "agregar_libro" || operacion == "modificar_libro" ||
                operacion == "agregar_estudiante" || operacion == "confirmar_prestamo")
            {
                return Tipo >= TipoEmpleado.Bibliotecario;
            }

            if (operacion == "generar_reportes" || operacion == "gestionar_multas" ||
                operacion == "configurar_sala" || operacion == "cancelar_prestamo")
            {
                return Tipo >= TipoEmpleado.Supervisor;
            }

            if (operacion == "agregar_empleado" || operacion == "modificar_empleado" ||
                operacion == "eliminar_datos" || operacion == "configurar_sistema" ||
                operacion == "dar_baja_empleado" || operacion == "modificar_salarios")
            {
                return Tipo == TipoEmpleado.Administrador;
            }

            return false;
        }

        public bool CambiarTurno(string nuevoTurno)
        {
            if (!EstaActivo)
            {
                WriteLine("Error: No se puede cambiar el turno de un empleado inactivo.");
                return false;
            }

            try
            {
                string turnoAnterior = Turno;
                Turno = nuevoTurno;
                RegistrarActividad($"Turno cambiado de '{turnoAnterior}' a '{nuevoTurno}'");
                WriteLine($"  Turno actualizado exitosamente.");
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteLine($"Error al cambiar turno: {ex.Message}");
                return false;
            }
        }

        public bool ActualizarSalario(decimal nuevoSalario, Empleado empleadoAutorizador)
        {
            if (empleadoAutorizador?.Tipo != TipoEmpleado.Administrador)
            {
                WriteLine("Error: Solo administradores pueden actualizar salarios.");
                return false;
            }

            if (!empleadoAutorizador.EstaActivo)
            {
                WriteLine("Error: El empleado autorizador no está activo.");
                return false;
            }

            try
            {
                decimal salarioAnterior = Salario;
                decimal porcentajeCambio = ((nuevoSalario - salarioAnterior) / salarioAnterior) * 100;

                if (Math.Abs(porcentajeCambio) > 50)
                {
                    WriteLine($"   Advertencia: Cambio de salario del {porcentajeCambio:F1}%");
                    Write("¿Confirmar cambio? (s/n): ");
                    if (ReadLine()?.ToLower() != "s")
                    {
                        WriteLine("Cambio de salario cancelado.");
                        return false;
                    }
                }

                Salario = nuevoSalario;
                RegistrarActividad($"Salario actualizado de {salarioAnterior:C} a {nuevoSalario:C} por {empleadoAutorizador.Nombre} (#{empleadoAutorizador.NumeroEmpleado})");
                WriteLine($"  Salario actualizado exitosamente.");
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteLine($"Error al actualizar salario: {ex.Message}");
                return false;
            }
        }

        public string CalcularAntiguedad()
        {
            TimeSpan antiguedad = DateTime.Now - FechaContratación;
            int años = (int)(antiguedad.Days / 365.25);
            int meses = (int)((antiguedad.Days % 365.25) / 30);

            if (años == 0)
                return $"{meses} meses";
            else if (meses == 0)
                return $"{años} años";
            else
                return $"{años} años y {meses} meses";
        }

        public bool EstaEnHorarioLaboral()
        {
            if (!EstaActivo) return false;

            var horaActual = DateTime.Now.TimeOfDay;
            string turnoLower = Turno.ToLower();

            switch (turnoLower)
            {
                case "matutino":
                    return horaActual >= new TimeSpan(6, 0, 0) && horaActual <= new TimeSpan(14, 0, 0);
                case "vespertino":
                    return horaActual >= new TimeSpan(14, 0, 0) && horaActual <= new TimeSpan(22, 0, 0);
                case "nocturno":
                    return horaActual >= new TimeSpan(22, 0, 0) || horaActual <= new TimeSpan(6, 0, 0);
                case "completo":
                    return horaActual >= new TimeSpan(8, 0, 0) && horaActual <= new TimeSpan(17, 0, 0);
                case "mixto":
                    return horaActual >= new TimeSpan(10, 0, 0) && horaActual <= new TimeSpan(19, 0, 0);
                default:
                    return true;
            }
        }

        public bool DarDeBaja(string motivo, Empleado empleadoAutorizador)
        {
            if (empleadoAutorizador?.Tipo != TipoEmpleado.Administrador)
            {
                WriteLine("Error: Solo administradores pueden dar de baja empleados.");
                return false;
            }

            if (!EstaActivo)
            {
                WriteLine("Error: El empleado ya está dado de baja.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                WriteLine("Error: Debe proporcionar un motivo para la baja.");
                return false;
            }

            EstaActivo = false;
            FechaBaja = DateTime.Now;
            MotivoBaja = motivo;

            RegistrarActividad($"Dado de baja por {empleadoAutorizador.Nombre}. Motivo: {motivo}");
            WriteLine($"  Empleado {Nombre} dado de baja exitosamente.");
            return true;
        }

        public bool Reactivar(Empleado empleadoAutorizador)
        {
            if (empleadoAutorizador?.Tipo != TipoEmpleado.Administrador)
            {
                WriteLine("Error: Solo administradores pueden reactivar empleados.");
                return false;
            }

            if (EstaActivo)
            {
                WriteLine("Error: El empleado ya está activo.");
                return false;
            }

            EstaActivo = true;
            FechaBaja = null;
            MotivoBaja = null;

            RegistrarActividad($"Reactivado por {empleadoAutorizador.Nombre}");
            WriteLine($"  Empleado {Nombre} reactivado exitosamente.");
            return true;
        }

        protected void RegistrarActividad(string actividad)
        {
            string registro = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {actividad}";
            HistorialActividades.Add(registro);

            if (HistorialActividades.Count > 100)
            {
                HistorialActividades.RemoveAt(0);
            }
        }

        public void MostrarHistorialActividades(int cantidad = 10)
        {
            WriteLine($"\n   HISTORIAL DE ACTIVIDADES - {Nombre}");
            WriteLine(new string('-', 50));

            int totalActividades = HistorialActividades.Count;
            int skip = Math.Max(0, totalActividades - cantidad);
            var actividades = HistorialActividades.Skip(skip).Take(cantidad).Reverse();

            if (!actividades.Any())
            {
                WriteLine("No hay actividades registradas.");
            }
            else
            {
                foreach (var actividad in actividades)
                {
                    WriteLine(actividad);
                }
            }

            WriteLine(new string('-', 50));
        }

        public decimal CalcularBonoPorAntiguedad()
        {
            int añosServicio = (DateTime.Now - FechaContratación).Days / 365;
            decimal porcentajeBono = 0;

            if (añosServicio >= 20) porcentajeBono = 0.20m;
            else if (añosServicio >= 15) porcentajeBono = 0.15m;
            else if (añosServicio >= 10) porcentajeBono = 0.10m;
            else if (añosServicio >= 5) porcentajeBono = 0.05m;
            else if (añosServicio >= 3) porcentajeBono = 0.03m;

            return Salario * porcentajeBono;
        }

        public int DiasVacacionesCorrespondientes()
        {
            int añosServicio = (DateTime.Now - FechaContratación).Days / 365;

            if (añosServicio < 1) return 0;
            else if (añosServicio < 2) return 10;
            else if (añosServicio < 3) return 12;
            else if (añosServicio < 5) return 14;
            else return 20;
        }

        public override string ObtenerResumen()
        {
            return $"Empleado #{NumeroEmpleado} - {Nombre} - {Puesto} ({Tipo}) - {(EstaActivo ? "Activo" : "Inactivo")}";
        }

        public bool ValidarDatosCompletos()
        {
            try
            {
                var tempNombre = Nombre;
                var tempIdentidad = Identidad;
                var tempCorreo = CorreoElectrónico;
                var tempTelefono = Teléfono;
                var tempFecha = FechaNacimiento;
                var tempDireccion = Dirección;

                var tempPuesto = Puesto;
                var tempTurno = Turno;
                var tempFechaContratacion = FechaContratación;
                var tempSalario = Salario;
                var tempTipo = Tipo;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void MostrarListaEmpleados(List<Empleado> empleados)
        {
            WriteLine($"\n{new string('=', 60)}");
            WriteLine($"    LISTA DE EMPLEADOS - Total: {empleados.Count}");
            WriteLine($"{new string('=', 60)}");

            if (empleados.Any())
            {
                var empleadosActivos = empleados.Where(e => e.EstaActivo).OrderBy(e => e.NumeroEmpleado);
                var empleadosInactivos = empleados.Where(e => !e.EstaActivo).OrderBy(e => e.NumeroEmpleado);

                if (empleadosActivos.Any())
                {
                    WriteLine("\n   EMPLEADOS ACTIVOS:");
                    foreach (var emp in empleadosActivos)
                    {
                        WriteLine($"  #{emp.NumeroEmpleado} - {emp.Nombre} - {emp.Puesto} - {emp.Tipo}");
                    }
                }

                if (empleadosInactivos.Any())
                {
                    WriteLine("\n   EMPLEADOS INACTIVOS:");
                    foreach (var emp in empleadosInactivos)
                    {
                        WriteLine($"  #{emp.NumeroEmpleado} - {emp.Nombre} - Baja: {emp.FechaBaja:dd/MM/yyyy} - Motivo: {emp.MotivoBaja}");
                    }
                }
            }
            else
            {
                WriteLine("No hay empleados registrados.");
            }
            WriteLine($"{new string('=', 60)}");
        }

        public static Empleado BuscarPorNumero(List<Empleado> empleados, int numeroEmpleado)
        {
            return empleados.FirstOrDefault(e => e.NumeroEmpleado == numeroEmpleado);
        }
    }
}