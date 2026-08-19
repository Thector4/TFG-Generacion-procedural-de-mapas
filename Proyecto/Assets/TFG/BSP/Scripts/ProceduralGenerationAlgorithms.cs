using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ProceduralGenerationAlgorithms
{
    public static List<BoundsInt> BinarySpacePartitioning(
    BoundsInt spaceToSplit,
    int minWidth, int minHeight,
    int maxWidth, int maxHeight,
    int minRooms, int maxRooms,
    int spacing) 
    {
        Queue<BoundsInt> spacesQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        spacesQueue.Enqueue(spaceToSplit);

        while (spacesQueue.Count > 0 && roomsList.Count < maxRooms)
        {
            var space = spacesQueue.Dequeue();

            int availableWidth = space.size.x - (2 * spacing);
            int availableHeight = space.size.y - (2 * spacing);

            if (availableWidth < minWidth || availableHeight < minHeight) continue;

            bool canSplitVertically = availableWidth >= (minWidth * 2) + (3 * spacing);
            bool canSplitHorizontally = availableHeight >= (minHeight * 2) + (3 * spacing);

            if ((canSplitVertically || canSplitHorizontally) && (roomsList.Count < minRooms || Random.value < 0.7f))
            {
                if (canSplitVertically && (!canSplitHorizontally || availableWidth >= availableHeight))
                {
                    SplitSpaceWithSpacing(space, minWidth, spacing, true, spacesQueue);
                }
                else if (canSplitHorizontally)
                {
                    SplitSpaceWithSpacing(space, minHeight, spacing, false, spacesQueue);
                }
                else
                {
                    roomsList.Add(CreateRoomWithSpacing(space, minWidth, minHeight, maxWidth, maxHeight, spacing));
                }
            }
            else
            {
                roomsList.Add(CreateRoomWithSpacing(space, minWidth, minHeight, maxWidth, maxHeight, spacing));
            }
        }

        return roomsList;
    }

    private static void SplitSpaceWithSpacing(BoundsInt space, int minSize, int spacing, bool isVertical, Queue<BoundsInt> spacesQueue)
    {
        if (isVertical)
        {
            int minSplit = minSize + (2 * spacing);
            int maxSplit = space.size.x - minSize - (2 * spacing);

            if (maxSplit <= minSplit) return;

            int splitX = Random.Range(minSplit, maxSplit);

            BoundsInt space1 = new BoundsInt(space.min, new Vector3Int(splitX, space.size.y, space.size.z));
            BoundsInt space2 = new BoundsInt(
                new Vector3Int(space.min.x + splitX, space.min.y, space.min.z),
                new Vector3Int(space.size.x - splitX, space.size.y, space.size.z));

            spacesQueue.Enqueue(space1);
            spacesQueue.Enqueue(space2);
        }
        else
        {
            int minSplit = minSize + (2 * spacing);
            int maxSplit = space.size.y - minSize - (2 * spacing);

            if (maxSplit <= minSplit) return;

            int splitY = Random.Range(minSplit, maxSplit);

            BoundsInt space1 = new BoundsInt(space.min, new Vector3Int(space.size.x, splitY, space.size.z));
            BoundsInt space2 = new BoundsInt(
                new Vector3Int(space.min.x, space.min.y + splitY, space.min.z),
                new Vector3Int(space.size.x, space.size.y - splitY, space.size.z));

            spacesQueue.Enqueue(space1);
            spacesQueue.Enqueue(space2);
        }
    }

    private static BoundsInt CreateRoomWithSpacing(BoundsInt space, int minW, int minH, int maxW, int maxH, int spacing)
    {
        int maxRoomWidth = Mathf.Min(maxW, space.size.x - (2 * spacing));
        int maxRoomHeight = Mathf.Min(maxH, space.size.y - (2 * spacing));

        int roomWidth = Random.Range(minW, maxRoomWidth + 1);
        int roomHeight = Random.Range(minH, maxRoomHeight + 1);

        int centerX = space.min.x + spacing + Random.Range(0, Mathf.Max(0, space.size.x - roomWidth - (2 * spacing)));
        int centerY = space.min.y + spacing + Random.Range(0, Mathf.Max(0, space.size.y - roomHeight - (2 * spacing)));

        centerX = Mathf.Clamp(centerX, space.min.x + spacing, space.min.x + space.size.x - roomWidth - spacing);
        centerY = Mathf.Clamp(centerY, space.min.y + spacing, space.min.y + space.size.y - roomHeight - spacing);

        return new BoundsInt(
            new Vector3Int(centerX, centerY, space.min.z),
            new Vector3Int(roomWidth, roomHeight, space.size.z)
        );
    }
}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1),        //UP
        new Vector2Int(1,0),        //RIGHT
        new Vector2Int(0, -1),      // DOWN
        new Vector2Int(-1, 0)       //LEFT
    };

    public static List<Vector2Int> diagonalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(1,1),        //UP-RIGHT
        new Vector2Int(1,-1),       //RIGHT-DOWN
        new Vector2Int(-1, -1),     // DOWN-LEFT
        new Vector2Int(-1, 1)       //LEFT-UP
    };

    public static List<Vector2Int> eightDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1),        //UP
        new Vector2Int(1,1),        //UP-RIGHT
        new Vector2Int(1,0),        //RIGHT
        new Vector2Int(1,-1),       //RIGHT-DOWN
        new Vector2Int(0, -1),      //DOWN
        new Vector2Int(-1, -1),     //DOWN-LEFT
        new Vector2Int(-1, 0),      //LEFT
        new Vector2Int(-1, 1)       //LEFT-UP
    };
}