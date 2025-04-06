using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightFinderGraph
{
    // Clase que representa un vuelo entre dos aeropuertos
    class Flight
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public double Cost { get; set; }

        public Flight(string origin, string destination, double cost)
        {
            Origin = origin;
            Destination = destination;
            Cost = cost;
        }
    }

    // Clase que representa el grafo de vuelos
    class Graph
    {
        // Lista de adyacencia: cada aeropuerto y la lista de vuelos que parten de él
        private Dictionary<string, List<Flight>> adjacencyList;

        public Graph()
        {
            adjacencyList = new Dictionary<string, List<Flight>>();
        }

        // Agrega un vuelo (arista) al grafo
        public void AddFlight(Flight flight)
        {
            if (!adjacencyList.ContainsKey(flight.Origin))
                adjacencyList[flight.Origin] = new List<Flight>();

            adjacencyList[flight.Origin].Add(flight);

            // Aseguramos que el destino también esté presente en el grafo
            if (!adjacencyList.ContainsKey(flight.Destination))
                adjacencyList[flight.Destination] = new List<Flight>();
        }

        // Implementación del algoritmo de Dijkstra para encontrar la ruta más barata
        public (double cost, List<string> path) FindCheapestPath(string start, string end)
        {
            // Verificar que ambos nodos existan en el grafo
            if (!adjacencyList.ContainsKey(start) || !adjacencyList.ContainsKey(end))
            {
                Console.WriteLine("UNO O AMBOS AEROPUERTOS NO EXISTEN EN LA BASE DE DATOS.");
                return (double.MaxValue, new List<string>());
            }

            // Inicialización de distancias: infinito para todos excepto el nodo de inicio
            Dictionary<string, double> distances = new Dictionary<string, double>();
            Dictionary<string, string?> previous = new Dictionary<string, string?>();

            foreach (var vertex in adjacencyList.Keys)
            {
                distances[vertex] = double.MaxValue;
                previous[vertex] = null;
            }
            distances[start] = 0;

            // Uso de SortedSet como cola de prioridad, usando Item1 para la distancia y Item2 para el aeropuerto
            var queue = new SortedSet<(double, string)>(Comparer<(double, string)>.Create((a, b) =>
            {
                int cmp = a.Item1.CompareTo(b.Item1);
                if (cmp == 0)
                    return a.Item2.CompareTo(b.Item2);
                return cmp;
            }));
            queue.Add((0, start));

            while (queue.Count > 0)
            {
                var current = queue.First();
                queue.Remove(current);
                string currentAirport = current.Item2;
                double currentDistance = current.Item1;

                if (currentAirport == end)
                    break;

                // Procesar cada vuelo saliente del aeropuerto actual
                foreach (var flight in adjacencyList[currentAirport])
                {
                    double newDist = currentDistance + flight.Cost;
                    if (newDist < distances[flight.Destination])
                    {
                        // Se actualiza la distancia: se elimina el valor anterior de la cola de prioridad si existe
                        var oldTuple = (distances[flight.Destination], flight.Destination);
                        if (queue.Contains(oldTuple))
                            queue.Remove(oldTuple);

                        distances[flight.Destination] = newDist;
                        previous[flight.Destination] = currentAirport;
                        queue.Add((newDist, flight.Destination));
                    }
                }
            }

            // Reconstrucción del camino más barato
            List<string> path = new List<string>();
            string? currentPath = end;
            while (currentPath != null)
            {
                path.Insert(0, currentPath);
                currentPath = previous[currentPath];
            }

            if (path.First() != start)
                return (double.MaxValue, new List<string>());

            return (distances[end], path);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Simulación de base de datos de vuelos con datos de ejemplo
            Graph flightGraph = new Graph();
            flightGraph.AddFlight(new Flight("Madrid", "Barcelona", 100));
            flightGraph.AddFlight(new Flight("Madrid", "Paris", 150));
            flightGraph.AddFlight(new Flight("Barcelona", "Paris", 120));
            flightGraph.AddFlight(new Flight("Paris", "Londres", 200));
            flightGraph.AddFlight(new Flight("Madrid", "Londres", 300));
            flightGraph.AddFlight(new Flight("Barcelona", "Londres", 250));
            flightGraph.AddFlight(new Flight("Ecuador", "Argentina", 750));
            flightGraph.AddFlight(new Flight("Argentina", "Ecuador", 850));
            flightGraph.AddFlight(new Flight("Ecuador", "Madrid", 350));

            Console.WriteLine("INGRESE EL AEROPUERTO DE ORIGEN:");
            string origin = Console.ReadLine()!;

            Console.WriteLine("INGRESE EL AEROPUERTO DE DESTINO:");
            string destination = Console.ReadLine()!;

            var result = flightGraph.FindCheapestPath(origin, destination);
            if (result.cost == double.MaxValue)
            {
                Console.WriteLine("NO SE ENCONTRÓ UNA RUTA DISPONIBLE O UNO DE LOS AEROPUERTOS NO EXISTE EN LA BASE DE DATOS.");
            }
            else
            {
                Console.WriteLine($"RUTA MÁS BARATA CON COSTO {result.cost}:");
                Console.WriteLine(string.Join(" -> ", result.path));
            }
        }
    }
}
