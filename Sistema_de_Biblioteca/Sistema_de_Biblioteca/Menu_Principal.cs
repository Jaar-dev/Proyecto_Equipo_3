using Sistema_de_Biblioteca.Objetos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Console;

namespace Sistema_de_Biblioteca
{
    internal class Menu_Principal
    {
        private static List<Estudiante> estudiantes = new List<Estudiante>();
        private static List<Libro> libros = new List<Libro>();
        private static List<Préstamo> prestamos = new List<Préstamo>();
        private static List<Empleado> empleados = new List<Empleado>();
        private static Sala_Lectura salaPrincipal = new Sala_Lectura("Sala Principal", "Primer Piso", 3, 3, new TimeSpan(8, 0, 0), new TimeSpan(20, 0, 0));

        private static readonly string rutaBase = @"C:\Users\jo415\OneDrive\Documentos\2 semestre\Programacion I\Proyecto_Equipo_3\Sistema_de_Biblioteca\Sistema_de_Biblioteca\bin\Debug";

        private static readonly GestorPersistenciaJSON<Estudiante> gestorEstudiantes = new GestorPersistenciaJSON<Estudiante>(Path.Combine(rutaBase, "estudiantes.json"));
        private static readonly GestorPersistenciaJSON<Libro> gestorLibros = new GestorPersistenciaJSON<Libro>(Path.Combine(rutaBase, "libros.json"));
        private static readonly GestorPersistenciaJSON<Préstamo> gestorPrestamos = new GestorPersistenciaJSON<Préstamo>(Path.Combine(rutaBase, "prestamos.json"));
        private static readonly GestorPersistenciaJSON<Empleado> gestorEmpleados = new GestorPersistenciaJSON<Empleado>(Path.Combine(rutaBase, "empleados.json"));

        private static Empleado empleadoActual;

        public delegate void NotificaciónHandler(string mensaje);
        static NotificaciónHandler notificador = mensaje => WriteLine($"\n[NOTIFICACIÓN]: {mensaje}");

        static void Main(string[] args)
        {
            CargarDatos();


            InicializarAdministradorPorDefecto();

            IniciarSesionComoAdmin();

            if (empleadoActual == null)
            {
                WriteLine("No se pudo iniciar sesión. El sistema se cerrará.");
                return;
            }

            string opción;
            do
            {
                Clear();
                MostrarMenuPrincipal();
                opción = ReadLine();

                switch (opción)
                {
                    case "1":
                        MenuEstudiantes();
                        break;
                    case "2":
                        MenuLibros();
                        break;
                    case "3":
                        MenuPrestamos();
                        break;
                    case "4":
                        MenuSalaLectura();
                        break;
                    case "5":
                        MenuEmpleados();
                        break;
                    case "6":
                        GenerarReportes();
                        break;
                    case "7":
                        ActualizarDatosPersonales();
                        break;
                    case "8":
                        CambiarUsuario();
                        break;
                    case "9":
                        GuardarDatos();
                        WriteLine("Datos Guardados. ¡Hasta luego!");
                        break;
                    default:
                        WriteLine("Opción no válida.");
                        break;
                }
                if (opción != "9") PausarPantalla();
            } while (opción != "9");
        }

        private static void CargarDatos()
        {
            WriteLine("Cargando datos del sistema...");
            estudiantes = gestorEstudiantes.Cargar();
            libros = gestorLibros.Cargar();
            prestamos = gestorPrestamos.Cargar();
            empleados = gestorEmpleados.Cargar();

            VincularDatos();
            RestaurarContadoresEstaticos();
            WriteLine("¡Datos cargados exitosamente!");
            PausarPantalla();
        }

        private static void GuardarDatos()
        {
            WriteLine("Guardando datos...");
            gestorEstudiantes.Guardar(estudiantes);
            gestorLibros.Guardar(libros);
            gestorPrestamos.Guardar(prestamos);
            gestorEmpleados.Guardar(empleados);
        }

        private static void GuardarDatosAutomaticamente()
        {
            try
            {
                gestorEstudiantes.Guardar(estudiantes);
                gestorLibros.Guardar(libros);
                gestorPrestamos.Guardar(prestamos);
                gestorEmpleados.Guardar(empleados);
                WriteLine("\n✓ Datos guardados automáticamente.");
            }
            catch (Exception ex)
            {
                WriteLine($"\n Error al guardar automáticamente: {ex.Message}");
            }
        }

        private static void VincularDatos()
        {
            foreach (var prestamo in prestamos)
            {
                prestamo.VincularDatos(estudiantes, libros);
            }

            foreach (var estudiante in estudiantes)
            {
                estudiante.LibrosPrestados.Clear();
                var prestamosDelEstudiante = prestamos.Where(p => p.IdEstudiante == estudiante.Identidad && (p.Estado == "Activo" || p.Estado == "Confirmado"));
                foreach (var prestamo in prestamosDelEstudiante)
                {
                    if (prestamo.LibroPrestado != null)
                    {
                        estudiante.LibrosPrestados.Add(prestamo.LibroPrestado);
                    }
                }
            }
        }

        private static void RestaurarContadoresEstaticos()
        {
            if (prestamos.Any())
            {
                typeof(Préstamo).GetField("nextIdPréstamo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                               .SetValue(null, prestamos.Max(p => p.IdPréstamo) + 1);
            }
            if (empleados.Any())
            {
                typeof(Empleado).GetField("contadorEmpleados", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                                .SetValue(null, empleados.Max(e => e.NumeroEmpleado));
            }
            if (libros.Any())
            {
                typeof(Libro).GetProperty("TotalLibrosCreados", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                             .SetValue(null, libros.Count);
            }
        }

        private static void MostrarMenuPrincipal()
        {
            WriteLine($"==== Sistema de Biblioteca - Sesión: {empleadoActual.Nombre} ({empleadoActual.Tipo}) ====");
            WriteLine("1. Gestionar Estudiantes");
            WriteLine("2. Gestionar Libros");
            WriteLine("3. Gestionar Préstamos");
            WriteLine("4. Gestionar Sala de Lectura");
            WriteLine("5. Gestionar Empleados");
            WriteLine("6. Generar Reportes");
            WriteLine("7. Actualizar Datos Personales");
            WriteLine("8. Cambiar Usuario");
            WriteLine("9. Salir");
            Write("Seleccionar una opción: ");
        }

        private static void InicializarAdministradorPorDefecto()
        {
            try
            {
                if (empleados.Any())
                {
                    WriteLine(" Sistema ya tiene empleados configurados.");
                    return;
                }

                var adminPrincipal = new Empleado(
                    "Administrador Principal",
                    "0000000000000",
                    "admin@biblioteca.com",
                    "99999999",
                    new DateTime(2000, 7, 20),
                    "Oficina Principal",
                    "Administrador del Sistema",
                    "Completo",
                    DateTime.Now.AddYears(-5),
                    50000m,
                    Empleado.TipoEmpleado.Administrador
                );

                empleados.Add(adminPrincipal);
                WriteLine(" Sistema inicializado con Administrador Principal.");
            }
            catch (ArgumentException ex)
            {
                WriteLine($"Error al inicializar administrador: {ex.Message}");
                WriteLine("Se requiere un administrador para iniciar el sistema.");
            }
            catch (Exception ex)
            {
                WriteLine($" Error inesperado: {ex.Message}");
            }
        }

        private static void IniciarSesionComoAdmin()
        {
            empleadoActual = empleados.FirstOrDefault();
            if (empleadoActual == null)
            {
                WriteLine(" ERROR CRÍTICO: No se pudo inicializar el administrador.");
                WriteLine("El sistema no puede continuar sin un usuario administrador.");
                WriteLine("Presione cualquier tecla para salir...");
                ReadKey();
                return;
            }

            WriteLine("======== SISTEMA DE BIBLIOTECA ========");
            WriteLine();
            WriteLine(" INICIO DE SESIÓN AUTOMÁTICO");
            WriteLine($" Bienvenido: {empleadoActual.Nombre}");
            WriteLine($" Rol: {empleadoActual.Tipo}");
            WriteLine($" Estado: Todos los permisos habilitados");
            WriteLine(" Presione cualquier tecla para continuar...");
            ReadKey();
        }

        private static void CambiarUsuario()
        {
            WriteLine("=== CAMBIAR USUARIO ===");

            if (empleados.Count <= 1)
            {
                WriteLine(" Solo existe el administrador principal.");
                WriteLine(" Cree más empleados primero en el menú de empleados.");
                return;
            }

            WriteLine("Empleados disponibles:");
            foreach (var emp in empleados)
            {
                WriteLine($"   • {emp.Identidad} - {emp.Nombre} ({emp.Tipo})");
            }
            WriteLine();

            string identidad = PedirString("Identidad del empleado: ");
            var nuevoEmpleado = empleados.Find(e => e.Identidad.Equals(identidad, StringComparison.OrdinalIgnoreCase));

            if (nuevoEmpleado != null)
            {
                empleadoActual = nuevoEmpleado;
                notificador($"Sesión cambiada a: {empleadoActual.Nombre} ({empleadoActual.Tipo})");
            }
            else
            {
                WriteLine(" Empleado no encontrado.");
            }
        }

        static void MenuEstudiantes()
        {
            string opción;
            do
            {
                Clear();
                WriteLine("=== GESTIÓN DE ESTUDIANTES ===");
                WriteLine("1. Agregar Estudiante");
                WriteLine("2. Listar Estudiantes");
                WriteLine("3. Buscar Estudiante por Identidad");
                WriteLine("4. Volver al Menú Principal");
                Write("Seleccione una opción: ");
                opción = ReadLine();

                switch (opción)
                {
                    case "1":
                        AgregarEstudiante();
                        break;
                    case "2":
                        ListarEstudiantes();
                        break;
                    case "3":
                        BuscarEstudiante();
                        break;
                    case "4":
                        break;
                    default:
                        WriteLine("Opción inválida.");
                        break;
                }
                if (opción != "4") PausarPantalla();
            } while (opción != "4");
        }

        static void AgregarEstudiante()
        {
            WriteLine("--- AGREGAR NUEVO ESTUDIANTE ---");
            try
            {
                string nombre = PedirString("Nombre: ");

                string identidad;
                do
                {
                    identidad = PedirString("Identidad (única): ");
                    if (estudiantes.Any(e => e.Identidad == identidad))
                    {
                        WriteLine("Error: Ya existe un estudiante con esta identidad. Intente de nuevo.");
                    }
                } while (estudiantes.Any(e => e.Identidad == identidad));

                string correo = PedirString("Correo Electrónico: ");
                string teléfono = PedirTeléfono("Teléfono (8-12 dígitos): ");

                DateTime fechaNacimiento = PedirFecha("Fecha de Nacimiento",
                                                     new DateTime(1900, 1, 1),
                                                     DateTime.Now.AddYears(-7));

                string dirección = PedirString("Dirección: ");
                string carrera = PedirString("Carrera: ");

                var nuevoEstudiante = new Estudiante(nombre, identidad, correo, teléfono,
                                                   fechaNacimiento, dirección, carrera);
                estudiantes.Add(nuevoEstudiante);
                GuardarDatosAutomaticamente();

                notificador($"Estudiante '{nombre}' agregado exitosamente. Edad: {nuevoEstudiante.Edad} años");
            }
            catch (ArgumentException ex)
            {
                WriteLine($"Error: {ex.Message}");
            }
        }

        static void ListarEstudiantes()
        {
            WriteLine("--- LISTADO DE ESTUDIANTES ---");
            if (!estudiantes.Any())
            {
                WriteLine(" No hay estudiantes registrados en el sistema.");
                WriteLine(" Use la opción 'Agregar Estudiante' para comenzar a registrar estudiantes.");
                return;
            }

            WriteLine($"Total de estudiantes registrados: {estudiantes.Count}");
            WriteLine();
            estudiantes.ForEach(e => e.MostrarInformación());
        }

        static void BuscarEstudiante()
        {
            WriteLine("\n--- BUSCAR ESTUDIANTE ---");

            if (!estudiantes.Any())
            {
                WriteLine(" No hay estudiantes registrados para buscar.");
                return;
            }

            string idBuscar = PedirString("Ingrese la Identidad del estudiante a buscar: ");
            Estudiante encontrado = estudiantes.Find(e => e.Identidad.Equals(idBuscar, StringComparison.OrdinalIgnoreCase));

            if (encontrado != null)
            {
                notificador("Estudiante encontrado:");
                encontrado.MostrarInformación();
            }
            else
            {
                WriteLine($"Estudiante con Identidad '{idBuscar}' no encontrado.");
            }
        }

        static void MenuLibros()
        {
            string opción;
            do
            {
                Clear();
                WriteLine("=== GESTIÓN DE LIBROS ===");
                WriteLine("1. Agregar Libro");
                WriteLine("2. Listar Libros");
                WriteLine("3. Buscar Libro por ISBN");
                WriteLine("4. Volver al Menú Principal");
                Write("Seleccione una opción: ");
                opción = ReadLine();

                switch (opción)
                {
                    case "1":
                        AgregarLibro();
                        break;
                    case "2":
                        ListarLibros();
                        break;
                    case "3":
                        BuscarLibro();
                        break;
                    case "4":
                        break;
                    default:
                        WriteLine("Opción inválida.");
                        break;
                }
                if (opción != "4") PausarPantalla();
            } while (opción != "4");
        }

        static void AgregarLibro()
        {
            WriteLine("--- AGREGAR NUEVO LIBRO ---");
            try
            {
                if (!empleadoActual.PuedeRealizarOperación("agregar_libro"))
                {
                    WriteLine($" {empleadoActual.Nombre} ({empleadoActual.Tipo}) no tiene permisos para agregar libros.");
                    return;
                }

                string titulo = PedirString("Título: ");
                string autor = PedirString("Autor: ");
                string isbn = PedirISBNUnico();
                string genero = PedirString("Género: ");
                int añoPublicación = PedirInt("Año de Publicación: ", 1000, DateTime.Now.Year);
                string editorial = PedirString("Editorial: ");
                int totalCopias = PedirInt("Número Total de Copias: ", 1);

                var nuevoLibro = new Libro(titulo, autor, isbn, genero, añoPublicación, editorial, totalCopias);

                if (empleadoActual.RegistrarNuevoLibro(libros, nuevoLibro))
                {
                    GuardarDatosAutomaticamente();
                    notificador($"Libro '{titulo}' agregado exitosamente por {empleadoActual.Nombre}.");
                }
            }
            catch (ArgumentException ex)
            {
                WriteLine($"Error: {ex.Message}");
            }
        }

        static void ListarLibros()
        {
            WriteLine("--- LISTADO DE LIBROS ---");
            if (!libros.Any())
            {
                WriteLine(" No hay libros registrados en el sistema.");
                WriteLine("   Use la opción 'Agregar Libro' para comenzar a crear el inventario.");
                return;
            }

            WriteLine($"Total de libros en el sistema: {libros.Count}");
            WriteLine($"Total de ejemplares: {libros.Sum(l => l.NumeroCopiasTotales)}");
            WriteLine();
            libros.ForEach(l => l.MostrarInfo());
            WriteLine($"\nResumen: {Libro.TotalLibrosCreados} títulos registrados en la biblioteca");
        }

        static void BuscarLibro()
        {
            WriteLine("--- BUSCAR LIBRO ---");

            if (!libros.Any())
            {
                WriteLine(" No hay libros registrados para buscar.");
                return;
            }

            string isbnBuscar = PedirString("Ingrese el ISBN del libro a buscar: ");
            Libro encontrado = libros.Find(l => l.ISBN.Equals(isbnBuscar, StringComparison.OrdinalIgnoreCase));

            if (encontrado != null)
            {
                notificador("Libro encontrado:");
                encontrado.MostrarInfo();
            }
            else
            {
                WriteLine($"Libro con ISBN '{isbnBuscar}' no encontrado.");
            }
        }

        static void MenuPrestamos()
        {
            string opción;
            do
            {
                Clear();
                WriteLine("=== GESTIÓN COMPLETA DE PRÉSTAMOS ===");
                WriteLine("1. Realizar Préstamo");
                WriteLine("2. Confirmar Préstamo");
                WriteLine("3. Cancelar Préstamo");
                WriteLine("4. Devolver Libro");
                WriteLine("5. Listar Préstamos");
                WriteLine("6. Préstamos por Vencer");
                WriteLine("7. Volver al Menú Principal");
                Write("Seleccione una opción: ");
                opción = ReadLine();

                switch (opción)
                {
                    case "1":
                        RealizarPréstamo();
                        break;
                    case "2":
                        ConfirmarPréstamo();
                        break;
                    case "3":
                        CancelarPréstamo();
                        break;
                    case "4":
                        DevolverLibro();
                        break;
                    case "5":
                        ListarPrestamos();
                        break;
                    case "6":
                        MostrarPrestamosPorVencer();
                        break;
                    case "7":
                        break;
                    default:
                        WriteLine("Opción inválida.");
                        break;
                }
                if (opción != "7") PausarPantalla();
            } while (opción != "7");
        }

        static void RealizarPréstamo()
        {
            WriteLine("--- REALIZAR PRÉSTAMO ---");

            if (!estudiantes.Any())
            {
                WriteLine(" No hay estudiantes registrados. Agregue estudiantes primero.");
                return;
            }

            if (!libros.Any())
            {
                WriteLine(" No hay libros registrados. Agregue libros primero.");
                return;
            }

            try
            {
                string idEstudiante = PedirString("Identidad del estudiante: ");
                Estudiante est = estudiantes.Find(e => e.Identidad.Equals(idEstudiante, StringComparison.OrdinalIgnoreCase));

                if (est == null)
                    throw new Exception("Estudiante no encontrado.");

                if (!est.PuedePrestar())
                    throw new Exception($"El estudiante '{est.Nombre}' ya tiene el máximo de {est.MaxLibrosPermitidos} libros prestados.");

                string isbnLibro = PedirString("ISBN del libro a prestar: ");
                Libro lib = libros.Find(l => l.ISBN.Equals(isbnLibro, StringComparison.OrdinalIgnoreCase));

                if (lib == null)
                    throw new Exception("Libro no encontrado.");
                if (!lib.Disponible)
                    throw new Exception($"El libro '{lib.Titulo}' no está disponible actualmente.");

                Préstamo nuevoPréstamo = new Préstamo(est, lib);
                prestamos.Add(nuevoPréstamo);

                lib.PrestarCopia();
                est.AgregarLibroPrestado(lib);
                GuardarDatosAutomaticamente();

                notificador($"Préstamo registrado correctamente (ID: {nuevoPréstamo.IdPréstamo}).");
            }
            catch (Exception ex)
            {
                WriteLine($"Error al realizar préstamo: {ex.Message}");
            }
        }

        static void ConfirmarPréstamo()
        {
            WriteLine("--- CONFIRMAR PRÉSTAMO ---");

            var prestamosActivos = prestamos.Where(p => p.Estado == "Activo").ToList();

            if (!prestamosActivos.Any())
            {
                WriteLine(" No hay préstamos pendientes de confirmación.");
                return;
            }

            WriteLine("Préstamos pendientes de confirmación:");
            foreach (var p in prestamosActivos)
            {
                WriteLine($"ID: {p.IdPréstamo} - {p.Alumno.Nombre} - '{p.LibroPrestado.Titulo}'");
            }

            try
            {
                int idPréstamo = PedirInt("\nIngrese ID del préstamo a confirmar: ");
                Préstamo préstamo = prestamos.Find(p => p.IdPréstamo == idPréstamo);

                if (préstamo == null)
                    throw new Exception("Préstamo no encontrado.");

                préstamo.Confirmar();
                GuardarDatosAutomaticamente();
                notificador($"Préstamo {idPréstamo} confirmado exitosamente por {empleadoActual?.Nombre ?? "Sistema"}.");
            }
            catch (Exception ex)
            {
                WriteLine($"Error al confirmar préstamo: {ex.Message}");
            }
        }

        static void CancelarPréstamo()
        {
            WriteLine("--- CANCELAR PRÉSTAMO ---");

            var prestamosActivos = prestamos.Where(p => p.Estado == "Activo").ToList();

            if (!prestamosActivos.Any())
            {
                WriteLine(" No hay préstamos activos para cancelar.");
                return;
            }

            WriteLine("Préstamos activos:");
            foreach (var p in prestamosActivos)
            {
                WriteLine($"ID: {p.IdPréstamo} - {p.Alumno.Nombre} - '{p.LibroPrestado.Titulo}' - Fecha: {p.FechaPréstamo:dd/MM/yyyy}");
            }

            try
            {
                int idPréstamo = PedirInt("\nIngrese ID del préstamo a cancelar: ");
                Préstamo préstamo = prestamos.Find(p => p.IdPréstamo == idPréstamo);

                if (préstamo == null)
                    throw new Exception("Préstamo no encontrado.");

                WriteLine($"¿Está seguro de cancelar el préstamo de '{préstamo.LibroPrestado.Titulo}' para {préstamo.Alumno.Nombre}? (s/n): ");

                if (ReadLine()?.ToLower() == "s")
                {
                    préstamo.Cancelar();

                    if (préstamo.Alumno.LibrosPrestados.Contains(préstamo.LibroPrestado))
                    {
                        préstamo.Alumno.RemoverLibroPrestado(préstamo.LibroPrestado);
                        préstamo.LibroPrestado.RecibirCopia();
                    }
                    GuardarDatosAutomaticamente();

                    notificador($"Préstamo {idPréstamo} cancelado exitosamente.");
                }
                else
                {
                    WriteLine("Cancelación abortada.");
                }
            }
            catch (Exception ex)
            {
                WriteLine($"Error al cancelar préstamo: {ex.Message}");
            }
        }

        static void DevolverLibro()
        {
            WriteLine("\n--- DEVOLVER LIBRO ---");

            if (!prestamos.Any())
            {
                WriteLine(" No hay préstamos registrados.");
                return;
            }

            try
            {
                int idPréstamo = PedirInt("Ingrese el ID del préstamo a devolver: ");
                Préstamo préstamoADevolver = prestamos.Find(p => p.IdPréstamo == idPréstamo);

                if (préstamoADevolver == null)
                    throw new Exception("Préstamo no encontrado.");

                if (préstamoADevolver.Estado == "Devuelto")
                    throw new Exception("Este préstamo ya ha sido devuelto anteriormente.");

                if (préstamoADevolver.Estado == "Cancelado")
                    throw new Exception("Este préstamo fue cancelado y no puede ser devuelto.");

                if (empleadoActual != null && empleadoActual.ProcesarDevolución(préstamoADevolver))
                {
                    préstamoADevolver.LibroPrestado.RecibirCopia();
                    préstamoADevolver.Alumno.RemoverLibroPrestado(préstamoADevolver.LibroPrestado);

                    if (préstamoADevolver.Multa > 0)
                    {
                        WriteLine($"\nMULTA GENERADA: {préstamoADevolver.Multa:C}");
                        WriteLine("¿Multa pagada? (s/n): ");
                        if (ReadLine()?.ToLower() == "s")
                        {
                            empleadoActual.GestionarMulta(préstamoADevolver, true);
                        }
                    }
                    GuardarDatosAutomaticamente();

                    notificador($"Devolución del préstamo {idPréstamo} procesada exitosamente por {empleadoActual.Nombre}.");
                }
            }
            catch (Exception ex)
            {
                WriteLine($"Error al procesar devolución: {ex.Message}");
            }
        }

        static void ListarPrestamos()
        {
            WriteLine("--- LISTADO DE PRÉSTAMOS ---");
            if (!prestamos.Any())
            {
                WriteLine(" No hay préstamos registrados en el sistema.");
                WriteLine(" Los préstamos aparecerán aquí cuando se realicen operaciones de préstamo.");
                return;
            }

            WriteLine($"Total de préstamos en el sistema: {prestamos.Count}");
            WriteLine();
            foreach (var p in prestamos)
            {
                p.MostrarDetalle();
            }
            WriteLine($"\nResumen: {Préstamo.TotalPrestamos} préstamos realizados en total");
        }

        static void MostrarPrestamosPorVencer()
        {
            WriteLine("--- PRÉSTAMOS POR VENCER ---");

            var prestamosActivos = prestamos.Where(p => p.Estado == "Activo" || p.Estado == "Confirmado").ToList();

            if (!prestamosActivos.Any())
            {
                WriteLine(" No hay préstamos activos en el sistema.");
                return;
            }

            var prestamosPorVencer = prestamosActivos.Where(p =>
            {
                var diasRestantes = (p.FechaLimite - DateTime.Now).Days;
                return diasRestantes <= 3 && diasRestantes >= 0;
            }).ToList();

            if (!prestamosPorVencer.Any())
            {
                WriteLine(" No hay préstamos por vencer en los próximos 3 días.");
                return;
            }

            WriteLine($"Préstamos que vencen en los próximos 3 días ({prestamosPorVencer.Count}):");

            foreach (var prestamo in prestamosPorVencer.OrderBy(p => p.FechaLimite))
            {
                var diasRestantes = (prestamo.FechaLimite - DateTime.Now).Days;
                string urgencia;

                if (diasRestantes == 0)
                    urgencia = "HOY";
                else if (diasRestantes == 1)
                    urgencia = "MAÑANA";
                else
                    urgencia = $"EN {diasRestantes} DÍAS";

                WriteLine($"- {urgencia}: {prestamo.Alumno.Nombre} debe devolver '{prestamo.LibroPrestado.Titulo}' (ID: {prestamo.IdPréstamo})");
            }
        }

        static void MenuSalaLectura()
        {
            string opción;
            do
            {
                Clear();
                salaPrincipal.ActualizarEstadoSala();

                WriteLine("=== GESTIÓN DE SALA DE LECTURA ===");
                WriteLine("1. Ver Estado de la Sala");
                WriteLine("2. Ocupar Asiento");
                WriteLine("3. Liberar Asiento");
                WriteLine("4. Volver al Menú Principal");
                Write("Seleccione una opción: ");
                opción = ReadLine();

                switch (opción)
                {
                    case "1":
                        salaPrincipal.MostrarSala();
                        break;
                    case "2":
                        OcuparAsientoSala();
                        break;
                    case "3":
                        LiberarAsientoSala();
                        break;
                    case "4":
                        break;
                    default:
                        WriteLine("Opción inválida.");
                        break;
                }
                if (opción != "4") PausarPantalla();
            } while (opción != "4");
        }

        static void OcuparAsientoSala()
        {
            WriteLine("--- OCUPAR ASIENTO EN SALA ---");

            if (!estudiantes.Any())
            {
                WriteLine(" No hay estudiantes registrados. Solo estudiantes registrados pueden usar la sala.");
                return;
            }

            salaPrincipal.ActualizarEstadoSala();

            if (!salaPrincipal.EstaAbierta)
            {
                WriteLine(" La sala está cerrada. No se pueden ocupar asientos.");
                WriteLine($"Horario de atención: {salaPrincipal.HorarioApertura:hh\\:mm} - {salaPrincipal.HorarioCierre:hh\\:mm}");
                return;
            }

            salaPrincipal.MostrarSala();

            string identidadEstudiante = PedirString("Identidad del estudiante: ");
            Estudiante estudiante = estudiantes.Find(e => e.Identidad.Equals(identidadEstudiante, StringComparison.OrdinalIgnoreCase));

            if (estudiante == null)
            {
                WriteLine(" Estudiante no encontrado. Solo estudiantes registrados pueden usar la sala.");
                return;
            }

            int fila = PedirInt("Ingrese el número de fila (1-3): ", 1, 3) - 1;
            int columna = PedirInt("Ingrese el número de columna (1-3): ", 1, 3) - 1;

            if (salaPrincipal.OcuparAsiento(fila, columna))
            {
                notificador($"Asiento ({fila + 1},{columna + 1}) asignado a {estudiante.Nombre}.");
            }
        }

        static void LiberarAsientoSala()
        {
            WriteLine("--- LIBERAR ASIENTO EN SALA ---");

            salaPrincipal.MostrarSala();

            int fila = PedirInt("Ingrese el número de fila (1-3): ", 1, 3) - 1;
            int columna = PedirInt("Ingrese el número de columna (1-3): ", 1, 3) - 1;

            salaPrincipal.LiberarAsiento(fila, columna);
        }

        private static void MenuEmpleados()
        {
            string opción;
            do
            {
                Clear();
                WriteLine("=== GESTIÓN DE EMPLEADOS ===");
                WriteLine("1. Agregar Empleado");
                WriteLine("2. Listar Empleados");
                WriteLine("3. Buscar Empleado");
                WriteLine("4. Volver al Menú Principal");
                Write("Seleccione una opción: ");
                opción = ReadLine();

                switch (opción)
                {
                    case "1":
                        AgregarEmpleado();
                        break;
                    case "2":
                        ListarEmpleados();
                        break;
                    case "3":
                        BuscarEmpleado();
                        break;
                    case "4":
                        break;
                    default:
                        WriteLine("Opción inválida.");
                        break;
                }
                if (opción != "4") PausarPantalla();
            } while (opción != "4");
        }

        static void AgregarEmpleado()
        {
            WriteLine("--- AGREGAR NUEVO EMPLEADO ---");
            try
            {
                if (!empleadoActual.PuedeRealizarOperación("agregar_empleado"))
                {
                    WriteLine($" {empleadoActual.Nombre} ({empleadoActual.Tipo}) no tiene permisos para agregar empleados.");
                    return;
                }

                string nombre = PedirString("Nombre: ");
                string identidad = PedirIdentidadUnica(empleados.Cast<Persona>().ToList());
                string correo = PedirString("Correo Electrónico: ");
                string teléfono = PedirTeléfono("Teléfono: ");

                DateTime fechaNacimiento = PedirFecha("Fecha de Nacimiento",
                                                     new DateTime(1940, 1, 1),
                                                     DateTime.Now.AddYears(-18));

                string dirección = PedirString("Dirección: ");
                string puesto = PedirString("Puesto: ");
                string turno = PedirOpción("Turno", new[] { "Matutino", "Vespertino", "Nocturno", "Completo" });

                DateTime fechaContratación = PedirFecha("Fecha de Contratación",
                                                       fechaNacimiento.AddYears(14),
                                                       DateTime.Now);

                decimal salario = PedirDecimal("Salario: ", 13000m);

                WriteLine("Tipo de empleado:");
                WriteLine("1. Bibliotecario");
                WriteLine("2. Supervisor");
                WriteLine("3. Administrador");
                int tipoOpcion = PedirInt("Seleccione tipo (1-3): ", 1, 3);

                var tipo = (Empleado.TipoEmpleado)(tipoOpcion - 1);

                var nuevoEmpleado = new Empleado(nombre, identidad, correo, teléfono,
                                               fechaNacimiento, dirección, puesto, turno,
                                               fechaContratación, salario, tipo);

                empleados.Add(nuevoEmpleado);
                GuardarDatosAutomaticamente();
                notificador($"Empleado '{nombre}' agregado exitosamente. Edad: {nuevoEmpleado.Edad} años, Tipo: {tipo}");
            }
            catch (ArgumentException ex)
            {
                WriteLine($"Error: {ex.Message}");
            }
        }

        private static void ListarEmpleados()
        {
            WriteLine("--- LISTADO DE EMPLEADOS ---");
            if (!empleados.Any())
            {
                WriteLine(" No hay empleados registrados en el sistema.");
                return;
            }

            WriteLine($"Total de empleados en el sistema: {empleados.Count}");
            WriteLine();
            empleados.ForEach(e => e.MostrarInformación());
        }

        private static void BuscarEmpleado()
        {
            WriteLine("--- BUSCAR EMPLEADO ---");

            if (empleados.Count <= 1)
            {
                WriteLine(" Solo existe el administrador principal.");
                return;
            }

            string identidad = PedirString("Identidad del empleado: ");
            var encontrado = empleados.Find(e => e.Identidad.Equals(identidad, StringComparison.OrdinalIgnoreCase));

            if (encontrado != null)
            {
                notificador("Empleado encontrado:");
                encontrado.MostrarInformación();
            }
            else
            {
                WriteLine("Empleado no encontrado.");
            }
        }

        private static void GenerarReportes()
        {
            WriteLine("=== GENERAR REPORTES ===");

            if (!empleadoActual.PuedeRealizarOperación("generar_reportes"))
            {
                WriteLine($" {empleadoActual.Nombre} no tiene permisos para generar reportes.");
                return;
            }

            WriteLine("Seleccione tipo de reporte:");
            WriteLine("1. Reporte General");
            WriteLine("2. Reporte Básico");
            WriteLine("3. Reporte de Estudiantes por Edad");
            WriteLine("4. Reporte de Empleados");

            int opción = PedirInt("Opción (1-4): ", 1, 4);

            switch (opción)
            {
                case 1:
                    empleadoActual.GenerarReporteCompleto(libros, prestamos, estudiantes, "General");
                    break;
                case 2:
                    empleadoActual.GenerarReporteBásico("Básico");
                    break;
                case 3:
                    GenerarReporteEstudiantesPorEdad();
                    break;
                case 4:
                    GenerarReporteEmpleados();
                    break;
            }
        }

        static void GenerarReporteEstudiantesPorEdad()
        {
            string separador = new string('=', 50);

            WriteLine($"\n{separador}");
            WriteLine($" REPORTE DE ESTUDIANTES POR EDAD");
            WriteLine($"{separador}");

            if (!estudiantes.Any())
            {
                WriteLine(" No hay estudiantes registrados para generar reporte.");
                return;
            }

            var gruposEdad = estudiantes.GroupBy(e =>
            {
                int edad = e.Edad;

                if (edad < 12)
                    return "Niños (7-11)";
                else if (edad < 15)
                    return "Adolescentes Jóvenes (12-14)";
                else if (edad < 18)
                    return "Adolescentes (15-17)";
                else if (edad < 25)
                    return "Jóvenes Adultos (18-24)";
                else if (edad < 65)
                    return "Adultos (25-64)";
                else
                    return "Adultos Mayores (65+)";
            }).ToList();

            foreach (var grupo in gruposEdad.OrderBy(g => g.Key))
            {
                WriteLine($"\n{grupo.Key}: {grupo.Count()} estudiantes");
                foreach (var estudiante in grupo.OrderBy(e => e.Edad))
                {
                    WriteLine($"  • {estudiante.Nombre} - {estudiante.Edad} años ({estudiante.Carrera})");
                }
            }
        }

        static void GenerarReporteEmpleados()
        {
            string separador = new string('=', 50);
            WriteLine($"\n{separador}");
            WriteLine($" REPORTE DE EMPLEADOS");
            WriteLine($"{separador}");

            var empleadosPorTipo = empleados.GroupBy(e => e.Tipo);

            foreach (var grupo in empleadosPorTipo)
            {
                WriteLine($"\n{grupo.Key}: {grupo.Count()} empleados");
                foreach (var emp in grupo.OrderBy(e => e.Nombre))
                {
                    WriteLine($"  • {emp.Nombre} - {emp.Puesto} ({emp.Turno}) - {emp.Edad} años");
                }
            }
        }

        private static void ActualizarDatosPersonales()
        {
            WriteLine("=== ACTUALIZAR DATOS PERSONALES ===");
            WriteLine("Datos actuales:");
            empleadoActual.MostrarInformación();

            WriteLine("\n¿Qué dato desea actualizar?");
            WriteLine("1. Correo electrónico");
            WriteLine("2. Teléfono");
            WriteLine("3. Dirección");
            WriteLine("4. Todos los anteriores");

            int opción = PedirInt("Seleccione opción (1-4): ", 1, 4);

            try
            {
                switch (opción)
                {
                    case 1:
                        string nuevoCorreo = PedirString("Nuevo correo electrónico: ");
                        empleadoActual.ActualizarCorreo(nuevoCorreo);
                        break;
                    case 2:
                        string nuevoTelefono = PedirTeléfono("Nuevo teléfono: ");
                        empleadoActual.ActualizarTeléfono(nuevoTelefono);
                        break;
                    case 3:
                        string nuevaDireccion = PedirString("Nueva dirección: ");
                        empleadoActual.ActualizarDirección(nuevaDireccion);
                        break;
                    case 4:
                        string correo = PedirString("Nuevo correo electrónico: ");
                        string telefono = PedirTeléfono("Nuevo teléfono: ");
                        string direccion = PedirString("Nueva dirección: ");

                        empleadoActual.ActualizarCorreo(correo);
                        empleadoActual.ActualizarTeléfono(telefono);
                        empleadoActual.ActualizarDirección(direccion);
                    break;
                }

                GuardarDatosAutomaticamente();
                notificador("Datos actualizados exitosamente.");
            }
            catch (Exception ex)
            {
                WriteLine($"Error al actualizar datos: {ex.Message}");
            }
        }

        static string PedirString(string mensaje, bool puedeSerVacío = false)
        {
            string input;
            do
            {
                Write(mensaje);
                input = ReadLine();
                if (!puedeSerVacío && string.IsNullOrWhiteSpace(input))
                {
                    WriteLine("Error: Este campo no puede estar vacío. Intente de nuevo.");
                }
            } while (!puedeSerVacío && string.IsNullOrWhiteSpace(input));
            return input;
        }

        static int PedirInt(string mensaje, int min = 1, int max = int.MaxValue)
        {
            int valor;
            bool valido;
            do
            {
                Write(mensaje);
                valido = int.TryParse(ReadLine(), out valor) && valor >= min && valor <= max;
                if (!valido)
                {
                    WriteLine($"Error: Ingrese un número entero válido entre {min} y {max}. Intente de nuevo.");
                }
            } while (!valido);
            return valor;
        }

        static string PedirTeléfono(string mensaje)
        {
            string teléfono;
            bool valido;
            do
            {
                Write(mensaje);
                teléfono = ReadLine();
                valido = !string.IsNullOrWhiteSpace(teléfono) && Regex.IsMatch(teléfono, @"^\d{8,12}$");
                if (!valido)
                {
                    WriteLine("Error: El teléfono debe contener solo dígitos y tener una longitud de 8 a 12. Intente de nuevo.");
                }
            } while (!valido);
            return teléfono;
        }

        static DateTime PedirFecha(string mensaje, DateTime? minDate = null, DateTime? maxDate = null)
        {
            DateTime fecha;
            bool valido;
            minDate = minDate ?? new DateTime(1945, 1, 1);
            maxDate = maxDate ?? DateTime.Now;

            do
            {
                Write($"{mensaje} (YYYY-MM-DD): ");
                valido = DateTime.TryParse(ReadLine(), out fecha) && fecha >= minDate && fecha <= maxDate;
                if (!valido)
                {
                    WriteLine($"Error: Ingrese una fecha válida en formato YYYY-MM-DD, entre {minDate.Value.ToShortDateString()} y {maxDate.Value.ToShortDateString()}. Intente de nuevo.");
                }
            } while (!valido);
            return fecha;
        }

        static decimal PedirDecimal(string mensaje, decimal min = 0m)
        {
            decimal valor;
            bool valido;
            do
            {
                Write(mensaje);
                valido = decimal.TryParse(ReadLine(), out valor) && valor >= min;
                if (!valido)
                {
                    WriteLine($"Error: Ingrese un valor numérico válido mayor o igual a {min}. Intente de nuevo.");
                }
            } while (!valido);
            return valor;
        }

        private static string PedirIdentidadUnica(List<Persona> lista)
        {
            string identidad;
            do
            {
                identidad = PedirString("Identidad (única): ");
                if (lista.Any(e => e.Identidad == identidad))
                {
                    WriteLine("Error: Ya existe una persona con esta identidad.");
                }
            } while (lista.Any(e => e.Identidad == identidad));
            return identidad;
        }

        private static string PedirISBNUnico()
        {
            string isbn;
            do
            {
                isbn = PedirString("ISBN (único): ");
                if (libros.Any(l => l.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase)))
                {
                    WriteLine("Error: Ya existe un libro con este ISBN.");
                }
            } while (libros.Any(l => l.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase)));
            return isbn;
        }

        static string PedirOpción(string mensaje, string[] opciones)
        {
            WriteLine($"{mensaje}:");
            for (int i = 0; i < opciones.Length; i++)
            {
                WriteLine($"{i + 1}. {opciones[i]}");
            }

            int selección = PedirInt($"Seleccione opción (1-{opciones.Length}): ", 1, opciones.Length);
            return opciones[selección - 1];
        }

        private static void PausarPantalla()
        {
            WriteLine("\nPresione cualquier tecla para continuar...");
            ReadKey();
        }
    }
}