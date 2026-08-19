using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DungeonGenerator;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Random = UnityEngine.Random;

public class GraphAlgorithms : MonoBehaviour
{
    /// <summary>
    /// Handles gui controls, runs before node layout
    /// </summary>
    /*
     * Compara cada habitación conectada con todas las no conectadas para hallar la que esté a menor distancia, añade cada habitación
     * a las connectedRooms y mueve la habitación recién conectada de unconnected a connected
    */

    private float distance = 0;
    public bool forceGridAlignment = false;


    public void ConnectRoomsWithPrim(List<DungeonGenerator.Room> rooms)
    {
        if (rooms.Count == 0) return;

        HashSet<DungeonGenerator.Room> connected = new HashSet<DungeonGenerator.Room>();
        List<DungeonGenerator.Room> unconnected = new List<DungeonGenerator.Room>(rooms);

        DungeonGenerator.Room startRoom = unconnected[Random.Range(0, unconnected.Count)];
        connected.Add(startRoom);
        unconnected.Remove(startRoom);

        while (connected.Count < rooms.Count)
        {
            float minDistance = float.MaxValue;
            DungeonGenerator.Room closestConnected = null;
            DungeonGenerator.Room closestUnconnected = null;

            // Buscar la arista más corta entre connected y unconnected
            foreach (var connectedRoom in connected)
            {
                foreach (var unconnectedRoom in unconnected)
                {
                    if (forceGridAlignment) distance = GetConnectionCost(connectedRoom, unconnectedRoom);
                    else distance = Vector2.Distance(connectedRoom.position, unconnectedRoom.position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestConnected = connectedRoom;
                        closestUnconnected = unconnectedRoom;
                    }
                }
            }

            // Conectar las habitaciones más cercanas
            if (closestConnected != null && closestUnconnected != null)
            {
                closestConnected.connectedRooms.Add(closestUnconnected);
                closestUnconnected.connectedRooms.Add(closestConnected);
                connected.Add(closestUnconnected);
                unconnected.Remove(closestUnconnected);
            }
        }


    }

    /***************************************************************************/

    public void ConnectRoomsWithPrimOptimized(List<DungeonGenerator.Room> rooms)
    {
        if (rooms == null || rooms.Count <= 1) return;

        MinHeap<DungeonGenerator.Room> minHeap = new MinHeap<DungeonGenerator.Room>();
        HashSet<DungeonGenerator.Room> inMST = new HashSet<DungeonGenerator.Room>();
        Dictionary<DungeonGenerator.Room, float> minDistance = new Dictionary<DungeonGenerator.Room, float>();
        Dictionary<DungeonGenerator.Room, DungeonGenerator.Room> parent = new Dictionary<DungeonGenerator.Room, DungeonGenerator.Room>();

        DungeonGenerator.Room startRoom = rooms[0];
        minDistance[startRoom] = 0;
        minHeap.Insert(startRoom, 0);

        while (minHeap.Count > 0)
        {
            var currentRoom = minHeap.ExtractMin();

            if (inMST.Contains(currentRoom)) continue;

            inMST.Add(currentRoom);

            // Esto crea la conexión entre la habitación actual y su padre, si es que existe el padre
            if (parent.TryGetValue(currentRoom, out var parentRoom))
            {
                currentRoom.connectedRooms.Add(parentRoom);
                parentRoom.connectedRooms.Add(currentRoom);
            }

            // Se calcula la distancia entre habitaciones que no estén conectadas y se asigna la que menor distancia tenga
            foreach (var neighbor in rooms)
            {
                if (!inMST.Contains(neighbor))
                {
                    if (forceGridAlignment) distance = GetConnectionCost(currentRoom, neighbor);
                    else distance = Vector2.Distance(currentRoom.position, neighbor.position);



                    if (!minDistance.ContainsKey(neighbor) || distance < minDistance[neighbor])
                    {
                        minDistance[neighbor] = distance;

                        parent[neighbor] = currentRoom;

                        minHeap.Insert(neighbor, distance);
                    }
                }
            }
        }
    }

    /***************************************************************************/

    public void ConnectRoomsWithKruskal(List<DungeonGenerator.Room> rooms)
    {
        if (rooms.Count <= 1) return;

        // Lista de todas las posibles conexiones con sus distancias
        List<(DungeonGenerator.Room, DungeonGenerator.Room, float)> edges = new List<(DungeonGenerator.Room, DungeonGenerator.Room, float)>();

        // Generar todas las aristas posibles
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                if(forceGridAlignment) distance = GetConnectionCost(rooms[i], rooms[j]);
                else distance = Vector2.Distance(rooms[i].position, rooms[j].position);

                edges.Add((rooms[i], rooms[j], distance));
            }
        }

        // Ordenar aristas por distancia (de menor a mayor)
        edges.Sort((a, b) => a.Item3.CompareTo(b.Item3));

        Dictionary<DungeonGenerator.Room, int> component = new Dictionary<DungeonGenerator.Room, int>();
        int currentComponent = 0;

        // Inicializar cada habitación como su propio componente
        foreach (var room in rooms)
        {
            component[room] = currentComponent++;
        }

        // Procesar aristas en orden
        foreach (var edge in edges)
        {
            var roomA = edge.Item1;
            var roomB = edge.Item2;

            // Si están en componentes diferentes
            if (component[roomA] != component[roomB])
            {
                // Conectar las habitaciones
                roomA.connectedRooms.Add(roomB);
                roomB.connectedRooms.Add(roomA);

                // Fusionar los componentes
                int componentToMerge = component[roomB];
                int targetComponent = component[roomA];

                foreach (var room in rooms)
                {
                    if (component[room] == componentToMerge)
                    {
                        component[room] = targetComponent;
                    }
                }
            }
        }
    }





    /***************************************************************************/


    public class MinHeap<T>
    {
        private List<(T item, float priority)> heap = new List<(T, float)>();

        public int Count => heap.Count;

        public void Insert(T item, float priority)
        {
            heap.Add((item, priority));
            int i = heap.Count - 1;
            while (i > 0)
            {
                int parentIndex = (i - 1) / 2;
                if (heap[parentIndex].priority <= heap[i].priority) break;
                (heap[i], heap[parentIndex]) = (heap[parentIndex], heap[i]);
                i = parentIndex;
            }
        }

        /***************************************************************************/

        public T ExtractMin()
        {
            if (heap.Count == 0) throw new InvalidOperationException("Heap is empty");

            var min = heap[0];
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            int i = 0;
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < heap.Count && heap[left].priority < heap[smallest].priority)
                    smallest = left;
                if (right < heap.Count && heap[right].priority < heap[smallest].priority)
                    smallest = right;

                if (smallest == i) break;

                (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
                i = smallest;
            }

            return min.item;
        }
    }


    public float GetConnectionCost(DungeonGenerator.Room a, DungeonGenerator.Room b)
    {
        float baseDist = Vector2.Distance(a.position, b.position);

        // Si no está activada la opción, usar distancia normal
        if (forceGridAlignment)
            return baseDist;

        // Premio si están alineados (misma X o misma Y)
        if (Mathf.Approximately(a.position.x, b.position.x) || Mathf.Approximately(a.position.y, b.position.y))
            return baseDist * 0.5f; // reduce el coste para forzar conexiones rectas

        return baseDist;
    }


}