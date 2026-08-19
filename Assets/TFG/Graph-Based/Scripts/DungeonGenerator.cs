using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using static DungeonParameters;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class DungeonGenerator : DungeonParameters
{
    private void Awake()
    {
        GenerateDungeon();
    }
    
    public void GenerateDungeon()
    {
        graphAlgorithm = new GraphAlgorithms();

        ClearAllChildren();

        int randomCountRooms = (int)MathF.Abs(Random.Range(roomsMinMax.x, roomsMinMax.y));

        for (int i = 0; i < randomCountRooms; i++)
        {
            Vector2 randomPos = Vector2.zero;

            switch (spatiatingUsed)
            {
                case SpatiatingType.Square:
                    randomPos = SpaciatingType(SpatiatingType.Square);
                    break;
                case SpatiatingType.SquareRandom:
                    randomPos = SpaciatingType(SpatiatingType.SquareRandom);
                    break;
                case SpatiatingType.Pentagon:
                    randomPos = SpaciatingType(SpatiatingType.Pentagon);
                    break;
                case SpatiatingType.Circle:
                    randomPos = SpaciatingType(SpatiatingType.Circle);
                    break;
                case SpatiatingType.Final:
                    randomPos = SpaciatingType(SpatiatingType.Final);
                    break;
                default:
                    break;
            }

            Room newRoom = new Room(randomPos);
            rooms.Add(newRoom);
        }

        switch (algorithmUsed)
        {
            case AlgorithmType.Prim:
                MeasureAndExecuteAlgorithm(graphAlgorithm.ConnectRoomsWithPrim, "Prim");
                break;
            case AlgorithmType.PrimOptimized:
                MeasureAndExecuteAlgorithm(graphAlgorithm.ConnectRoomsWithPrimOptimized, "Prim Optimizado");
                break;
            case AlgorithmType.Kruskal:
                MeasureAndExecuteAlgorithm(graphAlgorithm.ConnectRoomsWithKruskal, "Kruskal");
                break;
            default:
                break;
        }

        if(options.HasFlag(ActivationOptions.SeparatedRooms)) TypeOfRoomsSeparated();
        else if(options.HasFlag(ActivationOptions.ColorChances)) TypeOfRoomsRandom();

        if (ShowColorRooms)
        {
            foreach (Room room in rooms)
            {
                InstantiateRoom(room);
            }
            if(ShowColorCorridors) GenerateCorridors();
        }
        if(ShowSpriteMap)
        {
            foreach (Room room in rooms)
            {
                InstantiateRoom(room);
            }
            GenerateCorridors();
        }
    }

    private void ClearAllChildren()
    {
        int childCount = transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        rooms.Clear();
        corridors.Clear();
        occupiedPositions.Clear();
    }

    private void GenerateCorridors()
    {
        foreach (var room in rooms)
        {
            foreach (var connectedRoom in room.connectedRooms)
            {
                if (!corridors.Any(c =>
                    (c.roomA == room && c.roomB == connectedRoom) ||
                    (c.roomA == connectedRoom && c.roomB == room)))
                {
                    Corridor newCorridor = new Corridor(room, connectedRoom);
                    corridors.Add(newCorridor);

                    if (ShowColorRooms && CorridorColorPrefab != null || ShowSpriteMap == true)
                    {
                        InstantiateCorridor(newCorridor);
                    }
                }
            }
        }
    }

    private void InstantiateCorridor(Corridor corridor)
    {
        if (CorridorColorPrefab == null) return;
        if (CorridorSpritePrefab == null) return;

        Vector2 direction = (corridor.roomB.position - corridor.roomA.position).normalized;

        float corridorSize = GetCorridorSize();

        float totalDistance = Vector2.Distance(corridor.roomA.position, corridor.roomB.position);

        int corridorCount = Mathf.CeilToInt(totalDistance / corridorSize);

        GameObject corridorContainer = new GameObject("Corridor_" + corridor.roomA.position + "_to_" + corridor.roomB.position);
        corridorContainer.transform.SetParent(transform);

        float spacing = totalDistance / corridorCount;

        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
        float lateralOffset = corridorSize * 0.5f; 

        Vector2 currentPosition = corridor.roomA.position;

        if(ShowColorCorridors)
        {
            for (int i = 0; i < corridorCount; i++)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Vector2 leftPosition = currentPosition + perpendicular * lateralOffset;
                GameObject leftTile = Instantiate(
                    CorridorColorPrefab,
                    leftPosition,
                    Quaternion.identity,
                    corridorContainer.transform
                );
                leftTile.transform.rotation = Quaternion.Euler(0, 0, angle);

                Vector2 rightPosition = currentPosition - perpendicular * lateralOffset;
                GameObject rightTile = Instantiate(
                    CorridorColorPrefab,
                    rightPosition,
                    Quaternion.identity,
                    corridorContainer.transform
                );
                rightTile.transform.rotation = Quaternion.Euler(0, 0, angle);

                currentPosition += direction * spacing;

            }
        }
        else if(ShowSpriteMap)
        {
            for (int i = 0; i < corridorCount; i++)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Vector2 leftPosition = currentPosition + perpendicular * lateralOffset;
                GameObject leftTile = Instantiate(
                    CorridorSpritePrefab,
                    leftPosition,
                    Quaternion.identity,
                    corridorContainer.transform
                );
                leftTile.transform.rotation = Quaternion.Euler(0, 0, angle);

                Vector2 rightPosition = currentPosition - perpendicular * lateralOffset;
                GameObject rightTile = Instantiate(
                    CorridorSpritePrefab,
                    rightPosition,
                    Quaternion.identity,
                    corridorContainer.transform
                );
                rightTile.transform.rotation = Quaternion.Euler(0, 0, angle);

                currentPosition += direction * spacing;

            }
        }
    }

    private float GetCorridorSize()
    {
        SpriteRenderer corridorSprite = CorridorColorPrefab.GetComponent<SpriteRenderer>();
        if (corridorSprite != null && corridorSprite.sprite != null)
        {
            return corridorSprite.sprite.bounds.size.x * CorridorColorPrefab.transform.localScale.x;
        }

        return 1f;
    }

    public void TypeOfRoomsRandom()
    {
        if (rooms == null || rooms.Count == 0) return;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == 0) rooms[i].Type = RoomType.Start;
            else if (i == rooms.Count - 1) rooms[i].Type = RoomType.Boss;
            else rooms[i].Type = GetRandomRoomType();
        }
    }

    private RoomType GetRandomRoomType()
    {
        int total = (int)(roomChances.x + roomChances.y + roomChances.z);
        int roll = Random.Range(0, total);

        if (roll < roomChances.y) return RoomType.Normal;
        else if (roll < roomChances.y + roomChances.x) return RoomType.Easy;
        else return RoomType.Hard;
    }

    private void FixBossRoomConnections(Room bossRoom)
    {
        if (bossRoom.connectedRooms.Count > 1)
        {
            var toKeep = bossRoom.connectedRooms.First();
            bossRoom.connectedRooms.Clear();
            bossRoom.connectedRooms.Add(toKeep);

            foreach (var other in rooms)
            {
                if (other != bossRoom && other.connectedRooms.Contains(bossRoom))
                {
                    if (other != toKeep)
                        other.connectedRooms.Remove(bossRoom);
                }
            }
        }
    }

    public void TypeOfRoomsSeparated()
    {
        if (rooms == null || rooms.Count == 0) return;

        Room start = FindFurthestRoom(rooms[0], out _);
        Room boss = FindFurthestRoom(start, out _);

        start.Type = RoomType.Start;
        boss.Type = RoomType.Boss;

        FixBossRoomConnections(boss);

        foreach (Room room in rooms)
        {
            if (room == start || room == boss)
                continue;

            room.Type = GetRandomRoomType();
        }
    }

    private Room FindFurthestRoom(Room start, out float maxDist)
    {
        Queue<Room> queue = new();
        Dictionary<Room, float> distance = new();
        queue.Enqueue(start);
        distance[start] = 0;

        Room furthest = start;
        maxDist = 0;

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();
            foreach (var neighbor in current.connectedRooms)
            {
                if (!distance.ContainsKey(neighbor))
                {
                    distance[neighbor] = distance[current] + 1;
                    queue.Enqueue(neighbor);

                    if (distance[neighbor] > maxDist)
                    {
                        maxDist = distance[neighbor];
                        furthest = neighbor;
                    }
                }
            }
        }

        return furthest;
    }

    public Vector2 SpaciatingType(SpatiatingType type)
    {
        Vector2 randomPos = Vector2.zero;

        switch (spatiatingUsed)
        {
            case SpatiatingType.Square:
                randomPos = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)) * roomSpacing;
                break;
            case SpatiatingType.SquareRandom:
                randomPos = new Vector2(Random.Range(-10, 10) * Random.Range(-10, 10),
                                        Random.Range(-10, 10) * Random.Range(-10, 10)) * roomSpacing;
                break;
            case SpatiatingType.Pentagon:
                randomPos = Quaternion.Euler(0, 0, 72 * Mathf.Floor(Random.Range(0, 5))) *
                            (Random.Range(0, 10) * roomSpacing * Random.insideUnitCircle.normalized);
                break;
            case SpatiatingType.Circle:
                randomPos = Quaternion.Euler(0, 0, 72 * Mathf.Floor(Random.Range(0, 5))) *
                            (Random.Range(0, 10) * roomSpacing * Random.insideUnitCircle.normalized);
                break;
            case SpatiatingType.Final:
                randomPos = GenerateFinalRoomPosition();
                break;
            default:
                break;
        }

        return randomPos;
    }

    private Vector2 GenerateFinalRoomPosition()
    {
        Vector2 newPos = Vector2.zero;
        bool valid = false;
        int attempts = 0;
        int maxAttempts = 50;

        Room baseRoom = rooms.Count > 0 ? rooms[Random.Range(0, rooms.Count)] : null;

        while (!valid && attempts < maxAttempts)
        {
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            Vector2 dir = directions[Random.Range(0, directions.Length)];

            newPos = (baseRoom != null ? baseRoom.position : Vector2.zero) + dir * roomSpacing * 2;

            if (!occupiedPositions.Contains(newPos))
            {
                valid = true;
                occupiedPositions.Add(newPos);
            }

            attempts++;
        }

        if (!valid)
        {
            baseRoom = rooms.Count > 0 ? rooms[rooms.Count - 1] : null;
            newPos = (baseRoom != null ? baseRoom.position : Vector2.zero) + Vector2.right * roomSpacing * 2;
            occupiedPositions.Add(newPos);
        }

        return newPos;
    }

    private void InstantiateRoom(Room room)
    {
        GameObject prefabToUse = null;

        if(ShowColorRooms)
        {
            switch (room.Type)
            {
                case RoomType.Start:
                    prefabToUse = startRoomColorPrefab;
                    break;
                case RoomType.Boss:
                    prefabToUse = bossRoomColorPrefab;
                    break;
                case RoomType.Easy:
                    prefabToUse = easyRoomColorPrefab;
                    break;
                case RoomType.Normal:
                    prefabToUse = normalRoomColorPrefab;
                    break;
                case RoomType.Hard:
                    prefabToUse = hardRoomColorPrefab;
                    break;
            }
        }
        else if(ShowSpriteMap)
        {
            switch (room.Type)
            {
                case RoomType.Start:
                    prefabToUse = startRoomSpritePrefab;
                    break;
                case RoomType.Boss:
                    prefabToUse = bossRoomSpritePrefab;
                    break;
                case RoomType.Easy:
                    prefabToUse = easyRoomSpritePrefab;
                    break;
                case RoomType.Normal:
                    prefabToUse = normalRoomSpritePrefab;
                    break;
                case RoomType.Hard:
                    prefabToUse = hardRoomSpritePrefab;
                    break;
            }
        }

        if (prefabToUse != null)
        {
            GameObject roomsContainer = GameObject.Find("RoomsContainer");
            if (roomsContainer == null)
            {
                roomsContainer = new GameObject("RoomsContainer");
                roomsContainer.transform.SetParent(transform);
            }

            room.roomGameObject = Instantiate(prefabToUse, room.position, Quaternion.identity, roomsContainer.transform);
        }

    }

    public void OnDrawGizmosSelected()
    {
        if (rooms == null || rooms.Count == 0) return;

        if(!ShowSpriteMap)
        {
            foreach (Room room in rooms)
            {
                if(options == ActivationOptions.None)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(room.position, roomSpacing / 5.0f);
                }
                else if (!ShowColorRooms)
                {
                    switch (room.Type)
                    {
                        case RoomType.Start:
                            Gizmos.color = Color.yellow;
                            break;
                        case RoomType.Boss:
                            Gizmos.color = Color.red;
                            break;
                        case RoomType.Easy:
                            Gizmos.color = Color.green;
                            break;
                        case RoomType.Normal:
                            Gizmos.color = Color.blue;
                            break;
                        case RoomType.Hard:
                            Gizmos.color = new Color(0.5f, 0f, 0.5f); 
                            break;
                        default:
                            Gizmos.color = Color.white;
                            break;
                    }

                    Gizmos.DrawSphere(room.position, roomSpacing/ 5.0f);
                }

                if (room.connectedRooms != null)
                {
                    foreach (Room connectedRoom in room.connectedRooms)
                    {
                        if(!ShowColorCorridors)
                        {
                            Gizmos.color = new Color(0.9f, 0.9f, 0.9f);
                            Gizmos.DrawLine(room.position, connectedRoom.position);
                        }
                    }
                }
            }

        }
    }

    private void MeasureAndExecuteAlgorithm(Action<List<Room>> algorithm, string algorithmName)
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        long memoryBefore = GC.GetTotalMemory(true);
        algorithm(rooms);
        long memoryAfter = GC.GetTotalMemory(true);
        long memoryUsed = memoryAfter - memoryBefore;

        algorithm(rooms);

        long startTicks = Stopwatch.GetTimestamp();
        long startCpuTicks = Process.GetCurrentProcess().TotalProcessorTime.Ticks;

        algorithm(rooms); 

        long endTicks = Stopwatch.GetTimestamp();
        long endCpuTicks = Process.GetCurrentProcess().TotalProcessorTime.Ticks;

        double realTimeNs = (endTicks - startTicks) * (1_000_000_000.0 / Stopwatch.Frequency);
        double cpuTimeMs = (endCpuTicks - startCpuTicks) / (double)TimeSpan.TicksPerMillisecond;

        int cores = System.Environment.ProcessorCount;
        double estimatedCpuNs = realTimeNs * 1.2; 
        UnityEngine.Debug.Log($"{algorithmName}: " +
                                $"Tiempo REAL: {realTimeNs / 1_000_000:F3} ms " +
                                $"CPU time (estimado): {estimatedCpuNs / 1_000_000:F3} ms " +
                                $"Memoria usada: {memoryUsed / 1024} KB");
    }

}
