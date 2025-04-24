using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileGenerator))]
public class TileGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Board"))
        {
            GenerateBoard();
        }
        if (GUILayout.Button("Clear Board"))
        {
            ClearBoard((TileGenerator)target);
        }
    }
void GenerateBoard()
{
    TileGenerator tg = (TileGenerator)target;

    ClearBoard(tg);

    if (tg.tiles == null || tg.tiles.Length == 0 || tg.groundCoverWeights == null || tg.groundCoverWeights.Length != tg.tiles.Length)
    {
        Debug.LogError("Ensure that tiles and ground cover weights are assigned, and their lengths match.");
        return;
    }

    if (tg.generationMode == TileGenerator.Mode.Basemap)
    {
        GenerateBasemap(tg);
    }
    else if (tg.generationMode == TileGenerator.Mode.Decoration)
    {
        GenerateDecorations(tg);
    }
}

void GenerateBasemap(TileGenerator tg)
{
    float totalWeight = 0f;
    foreach (float weight in tg.groundCoverWeights)
    {
        totalWeight += weight;
    }

    for (int i = 0; i < tg.size.x; i++)
    {
        for (int j = 0; j < tg.size.y; j++)
        {
            GameObject selectedTile = GetRandomTileByWeight(tg.tiles, tg.groundCoverWeights, totalWeight);
            var newTile = PrefabUtility.InstantiatePrefab(selectedTile) as GameObject;
            newTile.transform.SetParent(tg.transform);
            newTile.transform.localPosition = new Vector3(
                (i - j) * tg.offset.x,
                -(i + j) * tg.offset.y,
                0
            );
            newTile.name += " " + i + "-" + j;
        }
    }
}

void GenerateDecorations(TileGenerator tg)
{
    if (tg.tiles == null || tg.decorations == null || tg.decorations.Length == 0)
    {
        Debug.LogError("Ensure decorations and tiles are assigned.");
        return;
    }

    // Generate ground tiles first
    GenerateBasemap(tg);

    // Randomly place decorations based on density
    int totalTiles = Mathf.FloorToInt(tg.size.x * tg.size.y * tg.decorationDensity);

    for (int k = 0; k < totalTiles; k++)
    {
        int i = Random.Range(0, (int)tg.size.x);
        int j = Random.Range(0, (int)tg.size.y);

        // Select a random decoration prefab
        GameObject selectedDecoration = tg.decorations[Random.Range(0, tg.decorations.Length)];
        var newDecoration = PrefabUtility.InstantiatePrefab(selectedDecoration) as GameObject;

        // Set decoration's position based on the same isometric grid logic
        newDecoration.transform.SetParent(tg.transform);
        newDecoration.transform.localPosition = new Vector3(
            (i - j) * tg.offset.x,
            -(i + j) * tg.offset.y,
            -0.1f // Slightly offset decorations so they appear above the ground
        );
        newDecoration.name += " Decoration " + i + "-" + j;
    }
}



GameObject GetRandomTileByWeight(GameObject[] tiles, float[] weights, float totalWeight)
{
    float randomValue = Random.value * totalWeight;
    float cumulativeWeight = 0f;

    for (int i = 0; i < tiles.Length; i++)
    {
        cumulativeWeight += weights[i];
        if (randomValue <= cumulativeWeight)
        {
            return tiles[i];
        }
    }

    // Fallback to the last tile in case of rounding issues
    return tiles[tiles.Length - 1];
}

    void ClearBoard(TileGenerator tg)
    {
        // Remove all children of the TileGenerator object
        for (int i = tg.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(tg.transform.GetChild(i).gameObject);
        }
    }
}
