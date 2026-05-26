using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;

[InitializeOnLoad]
public class TilemapAutoSelector
{
    static TilemapAutoSelector()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null) return;

        Tilemap tilemap = selected.GetComponent<Tilemap>();
        if (tilemap == null) return;

        TileBase targetTile = null;
        string tilePath = "";

        if (selected.name == "Tilemap_Colliders")
        {
            tilePath = "Assets/Tiles/Collider_Tile.asset";
        }
        else if (selected.name.StartsWith("Tilemap_Water"))
        {
            tilePath = "Assets/Tiles/Water_Simple_Fixed.asset";
        }

        if (!string.IsNullOrEmpty(tilePath))
        {
            targetTile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath);
            if (targetTile == null) return;

            // 1. Force the Paint Target
            GridPaintingState.scenePaintTarget = selected;
            
            // 2. Force the Palette
            var palette = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Palettes/Waterfall_Level_Palette.prefab");
            if (palette != null)
            {
                GridPaintingState.palette = palette;
            }

            // 3. Set the Brush Tile and ensure we are in Paint mode
            GridBrush brush = GridPaintingState.gridBrush as GridBrush;
            if (brush != null)
            {
                brush.Init(new Vector3Int(1, 1, 1));
                brush.SetTile(Vector3Int.zero, targetTile);
                
                // Repaint to ensure the change is visible
                SceneView.RepaintAll();
                
                Debug.Log("TilemapAutoSelector: Auto-selected " + targetTile.name + " for " + selected.name);
            }
        }
    }
}