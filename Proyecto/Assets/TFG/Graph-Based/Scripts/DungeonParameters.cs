using System.Collections.Generic;
using UnityEngine;

public class DungeonParameters : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public Vector2                  position            = Vector2.zero;
        public HashSet<Room>            connectedRooms      = new HashSet<Room>();
        public GameObject               roomGameObject; 
        public Room(Vector2 pos) { position = pos; }
        public RoomType Type { get; set; }
    }

    [System.Serializable]
    public class Corridor
    {
        public Room                     roomA;
        public Room                     roomB;
        public GameObject               corridorGameObject;

        public Corridor(Room a, Room b)
        {
            roomA = a;
            roomB = b;
        }
    }

    public enum ActivationOptions
    {
        None                = 0,
        ColorChances        = 1,
        SeparatedRooms      = 2
    }

    public enum AlgorithmType
    {
        Kruskal             = 0,
        Prim                = 1,
        PrimOptimized       = 2
    }

    public enum SpatiatingType
    {
        Square              = 0,
        SquareRandom        = 1,
        Pentagon            = 2,
        Circle              = 3,
        Final               = 4
    }

    public enum RoomType
    {
        Start               = 0,
        Normal              = 1,
        Easy                = 2,
        Hard                = 3,
        Boss                = 4
    }

    protected GraphAlgorithms           graphAlgorithm;
    protected List<Room>                rooms                   = new List<Room>();
    protected List<Corridor>            corridors               = new List<Corridor>();
    protected HashSet<Vector2>          occupiedPositions       = new HashSet<Vector2>();

    [Header("Dungeon Settings")]
    [Tooltip("Dungeon % Chances: X = Easy, Y = Normal, Z = Hard")]
    public Vector3                      roomChances             = Vector3.zero;
    [Tooltip("Min and Max rooms of the dungeon : X = Min, Y = Max")]
    public Vector2                      roomsMinMax             = Vector2.zero;
    public float                        roomSpacing             = 10f;

    [Header("Room Color Prefabs")]
    public GameObject                   startRoomColorPrefab;
    public GameObject                   bossRoomColorPrefab;
    public GameObject                   easyRoomColorPrefab;
    public GameObject                   normalRoomColorPrefab;
    public GameObject                   hardRoomColorPrefab;
    public GameObject                   CorridorColorPrefab;

    [Header("Room Color Prefabs")]
    public GameObject startRoomSpritePrefab;
    public GameObject bossRoomSpritePrefab;
    public GameObject easyRoomSpritePrefab;
    public GameObject normalRoomSpritePrefab;
    public GameObject hardRoomSpritePrefab;
    public GameObject CorridorSpritePrefab;


    public bool                         ShowColorRooms        = false;
    public bool                         ShowColorCorridors    = false;
    public bool                         ShowSpriteMap         = false;

    [Header("Algorithm Settings")]
    public AlgorithmType                algorithmUsed         = AlgorithmType.Prim;
    public SpatiatingType               spatiatingUsed        = SpatiatingType.Square;
    public ActivationOptions            options               = ActivationOptions.None;
}
