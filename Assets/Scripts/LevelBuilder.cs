using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public GameObject[] tilePrefabs = new GameObject[9];
    public float tileSize = 1.0f;

    int[,] levelMap =
    {
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
        BuildLevel();
    }

    void BuildLevel()
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tileIndex = levelMap[y, x];

                if (tileIndex < 0 || tileIndex >= tilePrefabs.Length || tilePrefabs[tileIndex] == null)
                    continue;

                Vector3 position = new Vector3(x * tileSize, -y * tileSize, 0);
                Quaternion rotation = Quaternion.identity;

                // --- 🔄 Rotation logic for corner tiles ---
                if (tileIndex == 1) // Outer Corner
                {
                    bool up = y > 0 && IsWall(levelMap[y - 1, x]);
                    bool down = y < rows - 1 && IsWall(levelMap[y + 1, x]);
                    bool left = x > 0 && IsWall(levelMap[y, x - 1]);
                    bool right = x < cols - 1 && IsWall(levelMap[y, x + 1]);

                    if (down && right)
                        rotation = Quaternion.Euler(0, 0, 0);     // Top-left corner
                    else if (down && left)
                        rotation = Quaternion.Euler(0, 0, 90);    // Top-right
                    else if (up && left)
                        rotation = Quaternion.Euler(0, 0, 180);   // Bottom-right
                    else if (up && right)
                        rotation = Quaternion.Euler(0, 0, 270);   // Bottom-left
                }

                else if (tileIndex == 2)
                {
                    bool up = y > 0 && IsWall(levelMap[y - 1, x]);
                    bool down = y < rows - 1 && IsWall(levelMap[y + 1, x]);
                    bool left = x > 0 && IsWall(levelMap[y, x - 1]);
                    bool right = x < cols - 1 && IsWall(levelMap[y, x + 1]);

                    if ((left && right) && !(up || down))
                        rotation = Quaternion.Euler(0, 0, 0); // horizontal wall
                    else if ((up && down) && !(left || right))
                        rotation = Quaternion.Euler(0, 0, 90); // vertical wall
                }                

                Instantiate(tilePrefabs[tileIndex], position, rotation, this.transform);
            }
        }

        Debug.Log("Level built with corner rotation.");
    }

    bool IsWall(int tileIndex)
    {
        // Define which tiles count as walls
        return tileIndex == 1 || tileIndex == 2 || tileIndex == 3 || tileIndex == 4 || tileIndex == 7 || tileIndex == 8;
    }
}
