using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public enum Mode { Basemap, Decoration } // Dropdown options
    public Mode generationMode;             // Selected mode
    public GameObject[] tiles;              // Ground cover tiles
    public float[] groundCoverWeights;      // Corresponding weights for ground tiles
    public GameObject[] decorations;        // Decoration prefabs
    public Vector2 size;                    // Board size
    public Vector2 offset;                  // Offset between tiles
    [Range(0, 1)] public float decorationDensity = 0.2f; // % of decorations per ground tile
}
