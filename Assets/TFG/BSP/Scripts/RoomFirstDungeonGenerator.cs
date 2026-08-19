using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int maxRoomWidth = 8, maxRoomHeight = 8;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    private int minRooms = 5, maxRooms = 10;
    [SerializeField]
    [Range(0,4)]
    private int offset = 0;
    
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private HashSet<Vector2Int> corridorPositions;

    [Header("Modes")]
    public bool showWalls                   = true;
    public bool showCorridors               = true;
    public bool autoCalculateDungeonSize    = true;

    [Header("Decorations")]
    [SerializeField]
    private bool generateDecorations = true;
    [SerializeField]
    [Range(0f, 1f)]
    private float decorationDensity = 0.05f;
    [SerializeField]
    private bool useAdvancedDecorationSystem = false;
    [SerializeField]
    

    public enum RoomType
    {
        None,
        Easy,
        Normal,
        Hard,
        Boss,
        Start,
        Treasure
    }

    [Header("Room Configuration")]
    [SerializeField]
    private RoomType startRoomType = RoomType.Start;
    [SerializeField]
    private RoomType bossRoomType = RoomType.Boss;
    [SerializeField]
    private int minEasyRooms = 1;
    [SerializeField]
    private int maxEasyRooms = 3;
    [SerializeField]
    private int minNormalRooms = 1;
    [SerializeField]
    private int maxNormalRooms = 2;
    [SerializeField]
    private int minHardRooms = 1;
    [SerializeField]
    private int maxHardRooms = 3;
    [SerializeField]
    private int minTreasureRooms = 1;
    [SerializeField]
    private int maxTreasureRooms = 2;

    [Header("Decoration Limits")]
    [SerializeField]
    private int minChestsPerRoom = 0;
    [SerializeField]
    private int maxChestsPerRoom = 2;
    [SerializeField]
    private int minTorchesPerRoom = 1;
    [SerializeField]
    private int maxTorchesPerRoom = 4;
    [SerializeField]
    private int minMoneyPerRoom = 0;
    [SerializeField]
    private int maxMoneyPerRoom = 3;
    [SerializeField]
    private int minKeysPerRoom = 0;
    [SerializeField]
    private int maxKeysPerRoom = 1;

    private Dictionary<Vector2Int, RoomType> roomTypes = new Dictionary<Vector2Int, RoomType>();

    protected override void RunProceduralGeneration()
    {
        if(autoCalculateDungeonSize)
        {
            MeasureAndExecuteAlgorithm("Generación de mapa automátizada", AutoCreateRooms);
        }
        else
        {
            MeasureAndExecuteAlgorithm("Generación de mapa manual", CreateRooms);
        }
    }


    private void CreateRooms()
    {
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth, minRoomHeight, maxRoomWidth, maxRoomHeight, minRooms, maxRooms, offset);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        floor = CreateSimpleRooms(roomsList);

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        if(showCorridors)
        {
            HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
            floor.UnionWith(corridors);
            corridorPositions = corridors;
        }

        tilemapVisualizer.PaintFloorTiles(floor);
        if(showWalls) WallGenerator.CreateWalls(floor, tilemapVisualizer);

        GenerateRoomDecorations();
    }

    private void AutoCreateRooms()
    {
        int spacing = offset + 1;

        int targetMaxRooms = useAdvancedDecorationSystem ?
            (maxEasyRooms + maxNormalRooms + maxHardRooms + maxTreasureRooms + 2) : maxRooms;
        int targetMinRooms = useAdvancedDecorationSystem ?
            (minEasyRooms + minNormalRooms + minHardRooms + minTreasureRooms + 2) : minRooms;

        int estimatedArea = targetMaxRooms * (maxRoomWidth + spacing * 2) * (maxRoomHeight + spacing * 2);
        int side = Mathf.CeilToInt(Mathf.Sqrt(estimatedArea)) + (spacing * 4);

        dungeonWidth = Mathf.Max(side, (minRoomWidth + spacing * 2) * 3);
        dungeonHeight = Mathf.Max(side, (minRoomHeight + spacing * 2) * 3);

        int spacingBetweenRooms = offset + 1;
        List<BoundsInt> roomsList = null;
        int attempts = 0;
        int maxAttempts = 10;

        while (attempts < maxAttempts)
        {
            attempts++;

            roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
                new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
                minRoomWidth, minRoomHeight,
                maxRoomWidth, maxRoomHeight,
                targetMinRooms,
                targetMaxRooms,
                spacingBetweenRooms
            );

            if (roomsList.Count >= targetMinRooms)
            {
                break;
            }

            dungeonWidth += spacingBetweenRooms * 2;
            dungeonHeight += spacingBetweenRooms * 2;
        
        }

        HashSet<Vector2Int> floor = CreateSimpleRooms(roomsList);

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        if (showCorridors)
        {
            HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
            floor.UnionWith(corridors);
            corridorPositions = corridors;
        }

        tilemapVisualizer.PaintFloorTiles(floor);
        if (showWalls) WallGenerator.CreateWalls(floor, tilemapVisualizer);

        GenerateRoomDecorations();
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if(destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if(destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }else if(destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if(currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        roomsDictionary.Clear(); 

        foreach (var room in roomsList)
        {
            HashSet<Vector2Int> roomFloor = new HashSet<Vector2Int>();

            for (int col = 1; col < room.size.x - 1; col++)
            {
                for (int row = 1; row < room.size.y - 1; row++) 
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                    roomFloor.Add(position);
                }
            }

            Vector2Int roomCenter = (Vector2Int)Vector3Int.RoundToInt(room.center);
            roomsDictionary[roomCenter] = roomFloor;
        }

        return floor;
    }

    private void GenerateRoomDecorations()
    {
        if (!generateDecorations) return;

        if (useAdvancedDecorationSystem)
        {
            List<Vector2Int> roomCentersList = new List<Vector2Int>(roomsDictionary.Keys);
            AssignRoomTypes(roomCentersList);
        }

        foreach (var roomEntry in roomsDictionary)
        {
            Vector2Int roomCenter = roomEntry.Key;
            HashSet<Vector2Int> roomPositions = roomEntry.Value;

            HashSet<Vector2Int> validPositions = new HashSet<Vector2Int>(roomPositions);
            if (corridorPositions != null)
            {
                validPositions.ExceptWith(corridorPositions);

                HashSet<Vector2Int> positionsNearCorridors = new HashSet<Vector2Int>();
                foreach (var corridorPos in corridorPositions)
                {
                    foreach (var direction in Direction2D.cardinalDirectionsList)
                    {
                        positionsNearCorridors.Add(corridorPos + direction);
                    }
                }
                validPositions.ExceptWith(positionsNearCorridors);
            }

            if (validPositions.Count == 0) continue;

            if (useAdvancedDecorationSystem)
            {
                RoomType roomType = roomTypes.ContainsKey(roomCenter) ? roomTypes[roomCenter] : RoomType.Normal;
                GenerateAdvancedDecorations(validPositions, roomType);
            }
            else
            {
                GenerateSimpleDecorations(validPositions);
            }
        }
    }

    private void GenerateSimpleDecorations(HashSet<Vector2Int> positions)
    {
        TilemapVisualizer visualizer = tilemapVisualizer;
        List<TileBase> availableDecorations = new List<TileBase>();

        if (visualizer.chest != null) availableDecorations.Add(visualizer.chest);
        if (visualizer.key != null) availableDecorations.Add(visualizer.key);
        if (visualizer.money != null) availableDecorations.Add(visualizer.money);
        if (visualizer.torch != null) availableDecorations.Add(visualizer.torch);

        if (availableDecorations.Count == 0) return;

        foreach (var position in positions)
        {
            if (Random.value < decorationDensity && availableDecorations.Count > 0)
            {
                TileBase randomTile = availableDecorations[Random.Range(0, availableDecorations.Count)];
                tilemapVisualizer.PaintSingleTile(tilemapVisualizer.decorationTilemap, randomTile, position);
            }
        }
    }

    private void GenerateAdvancedDecorations(HashSet<Vector2Int> positions, RoomType roomType)
    {
        TilemapVisualizer visualizer = tilemapVisualizer;
        List<TileBase> availableDecorations = new List<TileBase>();

        if (visualizer.chest != null) availableDecorations.Add(visualizer.chest);
        if (visualizer.key != null) availableDecorations.Add(visualizer.key);
        if (visualizer.money != null) availableDecorations.Add(visualizer.money);
        if (visualizer.torch != null) availableDecorations.Add(visualizer.torch);

        switch (roomType)
        {
            case RoomType.Start:
                PlaceDecorations(positions, visualizer.key, 1);
                break;
            case RoomType.Boss:
                PlaceDecorations(positions, visualizer.torch, 10);
                break;
            case RoomType.Easy:
                PlaceDecorations(positions, visualizer.torch, Random.Range(minTorchesPerRoom, maxTorchesPerRoom));
                PlaceDecorations(positions, visualizer.money, Random.Range(1, maxMoneyPerRoom + 1));
                break;
            case RoomType.Normal:
                PlaceDecorations(positions, visualizer.torch, Random.Range(minTorchesPerRoom, maxTorchesPerRoom));
                PlaceDecorations(positions, visualizer.money, Random.Range(1, maxMoneyPerRoom + 1));
                if (Random.value > 0.7f) PlaceDecorations(positions, visualizer.chest, Random.Range(1, maxChestsPerRoom + 1));
                break;
            case RoomType.Hard:
                PlaceDecorations(positions, visualizer.torch, Random.Range(4, 7));
                PlaceDecorations(positions, visualizer.money, Random.Range(1, maxMoneyPerRoom + 1));
                PlaceDecorations(positions, visualizer.chest, Random.Range(0, 3));
                break;
            case RoomType.Treasure:
                PlaceDecorations(positions, visualizer.chest, Random.Range(5, 11));
                break;
            default:
                break;
        }
    }

    private void AssignRoomTypes(List<Vector2Int> roomCenters)
    {
        roomTypes.Clear();
        if (roomCenters.Count == 0) return;

        List<Vector2Int> availableRooms = new List<Vector2Int>(roomCenters);

        Vector2Int startRoom = FindMostCentralRoom(availableRooms);
        roomTypes[startRoom] = startRoomType;
        availableRooms.Remove(startRoom);

        Vector2Int bossRoom = FindFurthestRoomFrom(startRoom, availableRooms);
        roomTypes[bossRoom] = bossRoomType;
        availableRooms.Remove(bossRoom);

        int easyRoomsCount = Random.Range(minEasyRooms, maxEasyRooms + 1);
        easyRoomsCount = Mathf.Min(easyRoomsCount, availableRooms.Count);
        for (int i = 0; i < easyRoomsCount; i++)
        {
            int randomIndex = Random.Range(0, availableRooms.Count);
            roomTypes[availableRooms[randomIndex]] = RoomType.Easy;
            availableRooms.RemoveAt(randomIndex);
        }

        int hardRoomsCount = Random.Range(minHardRooms, maxHardRooms + 1);
        hardRoomsCount = Mathf.Min(hardRoomsCount, availableRooms.Count);
        List<Vector2Int> roomsNearBoss = FindRoomsNearTarget(bossRoom, availableRooms, hardRoomsCount);
        foreach (var room in roomsNearBoss)
        {
            roomTypes[room] = RoomType.Hard;
            availableRooms.Remove(room);
        }

        int treasureRoomsCount = Random.Range(minTreasureRooms, maxTreasureRooms + 1);
        treasureRoomsCount = Mathf.Min(treasureRoomsCount, availableRooms.Count);
        for (int i = 0; i < treasureRoomsCount; i++)
        {
            int randomIndex = Random.Range(0, availableRooms.Count);
            roomTypes[availableRooms[randomIndex]] = RoomType.Treasure;
            availableRooms.RemoveAt(randomIndex);
        }

        foreach (var room in availableRooms)
        {
            roomTypes[room] = RoomType.Normal;
        }

        UnityEngine.Debug.Log($"Rooms assigned: Start=1, Boss=1, Easy={easyRoomsCount}, Hard={hardRoomsCount}, Treasure={treasureRoomsCount}, Normal={availableRooms.Count}");
    }

    private Vector2Int FindMostCentralRoom(List<Vector2Int> rooms)
    {
        Vector2Int mapCenter = startPosition + new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
        return rooms.OrderBy(room => Vector2.Distance(room, mapCenter)).First();
    }

    private Vector2Int FindFurthestRoomFrom(Vector2Int targetRoom, List<Vector2Int> rooms)
    {
        return rooms.OrderByDescending(room => Vector2.Distance(room, targetRoom)).First();
    }

    private List<Vector2Int> FindRoomsNearTarget(Vector2Int targetRoom, List<Vector2Int> rooms, int count)
    {
        if (rooms.Count <= count) return new List<Vector2Int>(rooms);

        var sortedByDistance = rooms.OrderBy(room => Vector2.Distance(room, targetRoom)).ToList();

        List<Vector2Int> result = new List<Vector2Int>();
        for (int i = 0; i < count; i++)
        {
            int index = (i * sortedByDistance.Count) / count;
            result.Add(sortedByDistance[index]);
        }

        return result;
    }

    private void PlaceDecorations(HashSet<Vector2Int> positions, TileBase tile, int count)
    {
        if (tile == null || count <= 0) return;

        List<Vector2Int> availablePositions = new List<Vector2Int>(positions);
        count = Mathf.Min(count, availablePositions.Count);

        for (int i = 0; i < count; i++)
        {
            if (availablePositions.Count == 0) break;

            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int position = availablePositions[randomIndex];
            availablePositions.RemoveAt(randomIndex);

            tilemapVisualizer.PaintSingleTile(tilemapVisualizer.decorationTilemap, tile, position);
        }
    }

    private void MeasureAndExecuteAlgorithm(string name, Action algorithm)
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        long memoryBefore = GC.GetTotalMemory(true); 
        algorithm();
        long memoryAfter = GC.GetTotalMemory(true); 
        long memoryUsed = memoryAfter - memoryBefore;

        long startTicks = Stopwatch.GetTimestamp();
        long startCpuTicks = Process.GetCurrentProcess().TotalProcessorTime.Ticks;

        long endTicks = Stopwatch.GetTimestamp();
        long endCpuTicks = Process.GetCurrentProcess().TotalProcessorTime.Ticks;

        double realTimeNs = (endTicks - startTicks) * (1_000_000_000.0 / Stopwatch.Frequency);
        double cpuTimeMs = (endCpuTicks - startCpuTicks) / (double)TimeSpan.TicksPerMillisecond;

        int cores = Environment.ProcessorCount;
        double estimatedCpuNs = realTimeNs * 1.2; 
        UnityEngine.Debug.Log($"{name}: " +
                  $"Tiempo REAL: {realTimeNs / 1_000_000:F3} ms " +
                  $"CPU time (estimado): {estimatedCpuNs / 1_000_000:F3} ms " +
                  $"Memoria usada: {memoryUsed / 1024} KB");
    }

}
