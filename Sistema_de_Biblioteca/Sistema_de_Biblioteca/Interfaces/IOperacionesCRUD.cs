using System;
using System.Collections.Generic;

namespace Sistema_de_Biblioteca.Interfaces
{
    public interface IOperacionesCRUD<T>
    {
        bool Agregar(T item);
        T Buscar(string criterio);
        bool Actualizar(string identificador, T itemActualizado);
        bool Eliminar(string identificador);
        List<T> Listar();
    }

    // Interface para objetos que pueden ser prestados
    public interface IPrestable
    {
        bool EstaDisponible();
        bool RealizarPrestamo();
        bool RealizarDevolucion();
        string ObtenerEstadoPrestamo();
    }

    // Interface para entidades que pueden generar reportes
    public interface IReporteable
    {
        void GenerarReporte();
        string ObtenerResumenActividad();
        Dictionary<string, object> ObtenerEstadisticas();
    }

    // Interface para validación de datos
    public interface IValidable
    {
        bool ValidarDatos();
        List<string> ObtenerErroresValidacion();
    }

    // Interface para persistencia en JSON
    public interface IPersistenciaJSON
    {
        bool GuardarEnJSON(string rutaArchivo);
        bool CargarDesdeJSON(string rutaArchivo);
        string ConvertirAJSON();
    }

    // Interface para notificaciones
    public interface INotificable
    {
        void EnviarNotificacion(string mensaje, string tipo);
        List<string> ObtenerNotificacionesPendientes();
        void MarcarNotificacionComoLeida(int idNotificacion);
    }

    // Interface para búsqueda avanzada
    public interface IBuscable
    {
        List<T> BuscarPorCriterio<T>(string campo, string valor);
        List<T> BuscarPorFecha<T>(DateTime fechaInicio, DateTime fechaFin);
        List<T> BuscarPorRango<T>(string campo, decimal valorMin, decimal valorMax);
    }
}