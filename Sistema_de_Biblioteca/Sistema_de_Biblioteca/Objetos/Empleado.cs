using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public class Empleado : Gestor_Biblioteca
    {
        private string puesto;
        public string Puesto
        {
            get => puesto;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El puesto del empleado no puede estar vacío.");
                puesto = value;
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
                turno = value;
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
                if (value < FechaNacimiento.AddYears(14))
                    throw new ArgumentException("La fecha de contratación no puede ser antes de los 14 años de edad.");
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
                salario = value;
            }
        }

        public enum TipoEmpleado
        {
            Bibliotecario,    // Operaciones básicas
            Supervisor,       // Operaciones avanzadas
            Administrador     // Todas las operaciones
        }

        public TipoEmpleado Tipo { get; set; }

        public Empleado(string nombre, string identidad, string correoElectrónico,
                       string teléfono, DateTime fechaNacimiento, string dirección,
                       string puesto, string turno, DateTime fechaContratación,
                       decimal salario, TipoEmpleado tipo = TipoEmpleado.Bibliotecario)
            : base(nombre, identidad, correoElectrónico, teléfono, fechaNacimiento, dirección)
        {
            Puesto = puesto;
            Turno = turno;
            FechaContratación = fechaContratación;
            Salario = salario;
            Tipo = tipo;
        }

        public override void MostrarInformación()
        {
            WriteLine($"\n{new string('=', 40)}");
            WriteLine($"    INFORMACIÓN DEL EMPLEADO");
            WriteLine($"{new string('=', 40)}");
            WriteLine($"Nombre: {Nombre}");
            WriteLine($"Identidad: {Identidad}");
            WriteLine($"Edad: {Edad} años (Nacimiento: {FechaNacimiento:dd/MM/yyyy})");
            WriteLine($"Puesto: {Puesto} ({Tipo})");
            WriteLine($"Turno: {Turno}");
            WriteLine($"Contacto: {CorreoElectrónico} | {Teléfono}");
            WriteLine($"Dirección: {Dirección}");
            WriteLine($"Fecha Contratación: {FechaContratación:dd/MM/yyyy}");
            WriteLine($"Años de servicio: {DateTime.Now.Year - FechaContratación.Year}");
            WriteLine($"Salario: {Salario:C}");
            WriteLine($"{new string('=', 40)}");
        }

        public override bool PuedeRealizarOperación(string tipoOperación)
        {
            string operacion = tipoOperación.ToLower();

            // Operaciones básicas - todos los empleados
            if (operacion == "prestamo" || operacion == "devolucion" ||
                operacion == "buscar" || operacion == "listar")
            {
                return true;
            }

            // Operaciones de bibliotecario
            if (operacion == "agregar_libro" || operacion == "modificar_libro" ||
                operacion == "agregar_estudiante")
            {
                return Tipo >= TipoEmpleado.Bibliotecario;
            }

            // Operaciones de supervisor
            if (operacion == "generar_reportes" || operacion == "gestionar_multas" ||
                operacion == "configurar_sala")
            {
                return Tipo >= TipoEmpleado.Supervisor;
            }

            // Operaciones de administrador
            if (operacion == "agregar_empleado" || operacion == "modificar_empleado" ||
                operacion == "eliminar_datos" || operacion == "configurar_sistema")
            {
                return Tipo == TipoEmpleado.Administrador;
            }

            return false;
        }

        public bool CambiarTurno(string nuevoTurno)
        {
            if (string.IsNullOrWhiteSpace(nuevoTurno))
            {
                WriteLine("Error: El turno no puede estar vacío.");
                return false;
            }

            string turnoAnterior = Turno;
            Turno = nuevoTurno;
            RegistrarActividad($"Turno cambiado de '{turnoAnterior}' a '{nuevoTurno}'");
            return true;
        }

        public bool ActualizarSalario(decimal nuevoSalario, Empleado empleadoAutorizador)
        {
            if (empleadoAutorizador?.Tipo != TipoEmpleado.Administrador)
            {
                WriteLine("Error: Solo administradores pueden actualizar salarios.");
                return false;
            }

            if (nuevoSalario <= 0)
            {
                WriteLine("Error: El salario debe ser positivo.");
                return false;
            }

            decimal salarioAnterior = Salario;
            Salario = nuevoSalario;
            RegistrarActividad($"Salario actualizado de {salarioAnterior:C} a {nuevoSalario:C} por {empleadoAutorizador.Nombre}");
            return true;
        }

        public TimeSpan CalcularAntiguedad()
        {
            return DateTime.Now - FechaContratación;
        }

        public bool EstaEnHorarioLaboral()
        {
            var horaActual = DateTime.Now.TimeOfDay;
            string turnoLower = Turno.ToLower();

            if (turnoLower == "matutino")
            {
                return horaActual >= new TimeSpan(6, 0, 0) && horaActual <= new TimeSpan(14, 0, 0);
            }
            else if (turnoLower == "vespertino")
            {
                return horaActual >= new TimeSpan(14, 0, 0) && horaActual <= new TimeSpan(22, 0, 0);
            }
            else if (turnoLower == "nocturno")
            {
                return horaActual >= new TimeSpan(22, 0, 0) || horaActual <= new TimeSpan(6, 0, 0);
            }
            else if (turnoLower == "completo")
            {
                return horaActual >= new TimeSpan(8, 0, 0) && horaActual <= new TimeSpan(17, 0, 0);
            }
            else
            {
                return true;
            }
        }

        public override bool GestionarMulta(Préstamo préstamo, bool pagada = false)
        {
            if (!PuedeRealizarOperación("gestionar_multas"))
            {
                WriteLine($"Error: {Nombre} ({Tipo}) no tiene permisos para gestionar multas.");
                return false;
            }

            if (!EstaEnHorarioLaboral())
            {
                WriteLine($"Advertencia: {Nombre} está gestionando multas fuera de su horario laboral ({Turno}).");
            }

            return base.GestionarMulta(préstamo, pagada);
        }

        public void MostrarResumenDesempeño(List<Préstamo> prestamos, List<Libro> libros)
        {
            WriteLine($"\n  RESUMEN DE DESEMPEÑO - {Nombre}");
            WriteLine($"Período: {FechaContratación:dd/MM/yyyy} - {DateTime.Now:dd/MM/yyyy}");
            WriteLine($"Antigüedad: {CalcularAntiguedad().Days} días");

            WriteLine($"Estado: {(EstaEnHorarioLaboral() ? "En horario" : "Fuera de horario")}");
            WriteLine($"Permisos: {Tipo}");
        }
    }
}