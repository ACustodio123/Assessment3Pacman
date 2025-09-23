using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject[] tilePrefabs = new GameObject[9];
    public float tileSize = 1.28f;
    public Transform manualLevelRoot;

    int[,] levelMap = {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    void Start()
    {
        if (manualLevelRoot != null)
        {
            Destroy(manualLevelRoot.gameObject);
        }

        GenerateQuadrant(Vector3.zero, false, false);
        int width = levelMap.GetLength(1);
        int height = levelMap.GetLength(0);

        GenerateQuadrant(new Vector3(width * tileSize, 0, 0), true, false);
        GenerateQuadrant(new Vector3(0, -height * tileSize, 0), false, true);
        GenerateQuadrant(new Vector3(width * tileSize, -height * tileSize, 0), true, true);
    }

    void GenerateQuadrant(Vector3 offset, bool mirrorX, bool mirrorY)
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            if (mirrorY && y == rows - 1) continue;

            for (int x = 0; x < cols; x++)
            {
                int tileIndex = levelMap[y, x];
                if (tileIndex < 0 || tileIndex >= tilePrefabs.Length || tilePrefabs[tileIndex] == null)
                    continue;

                int mappedX = mirrorX ? (cols - 1 - x) : x;
                int mappedY = mirrorY ? (rows - 1 - y) : y;
                Vector3 position = new Vector3(mappedX * tileSize, -mappedY * tileSize, 0) + offset;

                GameObject original = Instantiate(tilePrefabs[tileIndex], position, Quaternion.identity, this.transform);

                // Always determine original rotation
                Quaternion rotation = DetermineRotation(tileIndex, x, y);

                // Adjust rotation for mirrored quadrants
                float zRot = rotation.eulerAngles.z;
                if (mirrorX && mirrorY)
                    zRot = 180 - zRot;
                else if (mirrorX)
                    zRot = 180 - zRot;
                else if (mirrorY)
                    zRot = -zRot;

                original.transform.rotation = Quaternion.Euler(0, 0, zRot);


                // Apply mirroring
                Vector3 scale = Vector3.one;
                if (mirrorX) scale.x = -1;
                if (mirrorY) scale.y = -1;
                original.transform.localScale = scale;
            }
        }
    }

    Quaternion DetermineRotation(int tileIndex, int x, int y)
    {
        bool IsWall(int tile) => tile == 1 || tile == 2 || tile == 3 || tile == 4 || tile == 6 || tile == 7 || tile == 8;

        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        bool hasUp    = y > 0             && IsWall(levelMap[y - 1, x]);
        bool hasDown  = y < rows - 1      && IsWall(levelMap[y + 1, x]);
        bool hasLeft  = x > 0             && IsWall(levelMap[y, x - 1]);
        bool hasRight = x < cols - 1      && IsWall(levelMap[y, x + 1]);

        switch (tileIndex)
        {
            case 1: // OuterCorner
                if (hasDown && hasRight) return Quaternion.Euler(0, 0, 90);
                if (hasUp && hasRight) return Quaternion.Euler(0, 0, 180);
                if (hasUp && hasLeft) return Quaternion.Euler(0, 0, 270);
                return Quaternion.identity; // default: down + left

            case 2: // OuterWall
                if (hasLeft && hasRight) return Quaternion.Euler(0, 0, 90); // Horizontal
                return Quaternion.identity; // Vertical

            case 3: // InnerCorner
                if (hasLeft && hasDown) return Quaternion.Euler(0, 0, 0);   // Bottom-left
                if (hasDown && hasRight) return Quaternion.Euler(0, 0, 90); // Bottom-right
                if (hasRight && hasUp) return Quaternion.Euler(0, 0, 180);  // Top-right
                if (hasUp && hasLeft) return Quaternion.Euler(0, 0, 270);   // Top-left
                return Quaternion.identity;

            case 4: // InnerWall
                if (hasLeft && hasRight) return Quaternion.Euler(0, 0, 90); // Horizontal
                return Quaternion.identity; // Vertical

            case 6: // TSection
                if (!hasUp && hasLeft && hasRight && hasDown) return Quaternion.identity;
                if (!hasRight && hasLeft && hasUp && hasDown) return Quaternion.Euler(0, 0, 90);
                if (!hasDown && hasLeft && hasRight && hasUp) return Quaternion.Euler(0, 0, 180);
                if (!hasLeft && hasRight && hasUp && hasDown) return Quaternion.Euler(0, 0, 270);
                return Quaternion.identity;

            default:
                return Quaternion.identity;
        }
    }
}
