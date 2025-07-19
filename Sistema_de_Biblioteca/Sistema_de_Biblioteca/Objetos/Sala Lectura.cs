using System;
using static System.Console;

namespace Sistema_de_Biblioteca.Objetos
{
    public class Sala_Lectura
    {
        private char[,] asientos;
        public string NombreSala { get; set; }
        public int CapacidadMaxima { get; private set; }
        public string Ubicación { get; set; }
        public TimeSpan HorarioApertura { get; set; }
        public TimeSpan HorarioCierre { get; set; }
        public bool EstaAbierta { get; private set; }
        public int AsientosOcupados { get; private set; }

        public Sala_Lectura(string nombreSala, string ubicación, int filas, int columnas, TimeSpan apertura, TimeSpan cierre)
        {
            if (filas <= 0 || columnas <= 0) throw new ArgumentException("Filas y columnas deben ser positivas.");
            if (string.IsNullOrWhiteSpace(nombreSala)) throw new ArgumentException("El nombre de la sala no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(ubicación)) throw new ArgumentException("La ubicación no puede estar vacía.");

            NombreSala = nombreSala;
            Ubicación = ubicación;
            HorarioApertura = apertura;
            HorarioCierre = cierre;
            asientos = new char[filas, columnas];
            CapacidadMaxima = filas * columnas;
            AsientosOcupados = 0;
            InicializarAsientos();
            ActualizarEstadoSala();
        }

        private void InicializarAsientos()
        {
            for (int i = 0; i < asientos.GetLength(0); i++)
                for (int j = 0; j < asientos.GetLength(1); j++)
                    asientos[i, j] = 'D';
        }

        public void MostrarSala()
        {
            WriteLine($"--- Sala de Lectura: {NombreSala} ({Ubicación}) ---");
            WriteLine($"Horario: {HorarioApertura:hh\\:mm} - {HorarioCierre:hh\\:mm} | Estado: {(EstaAbierta ? "Abierta" : "Cerrada")}");
            WriteLine($"Asientos (D=Disponible, O=Ocupado, F=Fuera de Servicio):");
            for (int i = 0; i < asientos.GetLength(0); i++)
            {
                Write($"Fila {i + 1}: ");
                for (int j = 0; j < asientos.GetLength(1); j++)
                {
                    Write(asientos[i, j] + " ");
                }
                WriteLine();
            }
            WriteLine($"Asientos Ocupados: {AsientosOcupados}/{CapacidadMaxima}");
        }

        public bool OcuparAsiento(int fila, int columna)
        {
            if (!EstaAbierta)
            {
                WriteLine("La sala está cerrada. No se pueden ocupar asientos.");
                return false;
            }
            if (fila < 0 || fila >= asientos.GetLength(0) || columna < 0 || columna >= asientos.GetLength(1))
            {
                WriteLine("Asiento inválido. Fila o columna fuera de rango.");
                return false;
            }
            if (asientos[fila, columna] == 'D')
            {
                asientos[fila, columna] = 'O';
                AsientosOcupados++;
                WriteLine($"Asiento ({fila + 1},{columna + 1}) ocupado exitosamente.");
                return true;
            }
            else
            {
                WriteLine($"Asiento ({fila + 1},{columna + 1}) ya está ocupado o fuera de servicio.");
                return false;
            }
        }

        public bool LiberarAsiento(int fila, int columna)
        {
            if (fila < 0 || fila >= asientos.GetLength(0) || columna < 0 || columna >= asientos.GetLength(1))
            {
                WriteLine("Asiento inválido. Fila o columna fuera de rango.");
                return false;
            }
            if (asientos[fila, columna] == 'O')
            {
                asientos[fila, columna] = 'D';
                AsientosOcupados--;
                WriteLine($"Asiento ({fila + 1},{columna + 1}) liberado exitosamente.");
                return true;
            }
            else
            {
                WriteLine($"Asiento ({fila + 1},{columna + 1}) no está ocupado.");
                return false;
            }
        }

        public void ActualizarEstadoSala()
        {
            TimeSpan ahora = DateTime.Now.TimeOfDay;
            EstaAbierta = (ahora >= HorarioApertura && ahora <= HorarioCierre);
        }
    }
}