using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class DemoLevelBuilder
{
    [MenuItem("OnlyToType/Build Demo Level")]
    static void BuildDemoLevel()
    {
        if (GameObject.Find("Grid") != null)
        {
            if (!EditorUtility.DisplayDialog("Confirm",
                "Grid already exists. Replace it?", "Yes", "Cancel"))
                return;

            UnityEngine.Object.DestroyImmediate(GameObject.Find("Grid"));
        }

        var basePath = "Assets/2D Pixel Art Platformer Biome - American Forest/Tilemap";

        var tileSurface = AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileGround2.asset");
        var tileUnderground = AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileGround5.asset");
        var spikeTile = AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileSpikes.asset");

        var bgTiles = new List<TileBase>();
        for (int i = 1; i <= 13; i++)
        {
            var t = AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileBackGround{i}.asset");
            if (t != null) bgTiles.Add(t);
        }

        var plantTiles = new List<TileBase>();
        for (int i = 1; i <= 12; i++)
        {
            var t = AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TilePlant{i}.asset");
            if (t != null) plantTiles.Add(t);
        }

        var fenceTiles = new List<TileBase>();
        for (int i = 1; i <= 6; i++)
        {
            var t = AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileFence{i}.asset");
            if (t != null) fenceTiles.Add(t);
        }

        if (tileSurface == null)
        {
            EditorUtility.DisplayDialog("Error", "TileGround2 not found at " + basePath, "OK");
            return;
        }

        var gridObj = new GameObject("Grid");
        var grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        var ground = CreateTilemapLayer(gridObj, "GroundTilemap", 0, true, "Ground");
        var decor = CreateTilemapLayer(gridObj, "DecorationTilemap", 2, false, "Default");
        var bg = CreateTilemapLayer(gridObj, "BackgroundTilemap", -2, false, "Default");

        var groundTm = ground.GetComponent<Tilemap>();
        var decorTm = decor.GetComponent<Tilemap>();
        var bgTm = bg.GetComponent<Tilemap>();

        void SetTile(Tilemap tm, int x, int y, TileBase t)
        {
            if (t != null) tm.SetTile(new Vector3Int(x, y, 0), t);
        }

        TileBase RandomTile(List<TileBase> tiles) => tiles[Random.Range(0, tiles.Count)];

        void FillRow(int startX, int endX, int y, TileBase tile)
        {
            for (int x = startX; x < endX; x++)
                SetTile(groundTm, x, y, tile);
        }

        int groundY = -4;

        // ──────────────────────────────────────────────
        // 1段目 (y=-4): 表面地面
        // ──────────────────────────────────────────────
        FillRow(-9, 12, groundY, tileSurface);
        // Gap 1
        FillRow(12, 15, groundY, null);
        FillRow(15, 33, groundY, tileSurface);
        // Gap 2
        FillRow(33, 36, groundY, null);
        FillRow(36, 51, groundY, tileSurface);

        // ──────────────────────────────────────────────
        // 2段目〜6段目 (y=-5 ～ -9): 地下
        // ──────────────────────────────────────────────
        for (int y = groundY - 1; y >= groundY - 5; y--)
        {
            FillRow(-9, 12, y, tileUnderground);
            FillRow(15, 33, y, tileUnderground);
            FillRow(36, 51, y, tileUnderground);
        }

        // ──────────────────────────────────────────────
        // 浮遊台 (y=-1)
        // ──────────────────────────────────────────────
        FillRow(16, 19, -1, tileSurface);

        // ──────────────────────────────────────────────
        // スパイク (装飾, y=-3)
        // ──────────────────────────────────────────────
        SetTile(decorTm, 21, groundY + 1, spikeTile);
        SetTile(decorTm, 39, groundY + 1, spikeTile);
        SetTile(decorTm, 40, groundY + 1, spikeTile);
        SetTile(decorTm, 41, groundY + 1, spikeTile);

        // ──────────────────────────────────────────────
        // 階段状足場 (y=-3, -2, -1)
        // ──────────────────────────────────────────────
        int[][] steps = { new[]{25, 26}, new[]{27, 28}, new[]{29, 30} };
        for (int i = 0; i < steps.Length; i++)
        {
            int stepY = groundY + 1 + i;
            foreach (int sx in steps[i])
                SetTile(groundTm, sx, stepY, tileSurface);
        }

        // ──────────────────────────────────────────────
        // 浮遊台 2 (y=-1)
        // ──────────────────────────────────────────────
        FillRow(44, 47, -1, tileSurface);

        // ──────────────────────────────────────────────
        // 背景タイル (y=-5)
        // ──────────────────────────────────────────────
        for (int x = -9; x < 52; x++)
        {
            if (Random.value < 0.25f)
                SetTile(bgTm, x, groundY - 1, RandomTile(bgTiles));
        }

        // ──────────────────────────────────────────────
        // 装飾 (植物/フェンス)
        // ──────────────────────────────────────────────
        for (int x = -9; x < 52; x++)
        {
            if ((x >= 12 && x < 15) || (x >= 33 && x < 36)) continue;

            if (decorTm.GetTile(new Vector3Int(x, groundY + 1, 0)) != null)
                continue;

            if (Random.value < 0.12f && plantTiles.Count > 0)
                SetTile(decorTm, x, groundY + 1, RandomTile(plantTiles));
            else if (Random.value < 0.08f && fenceTiles.Count > 0)
                SetTile(decorTm, x, groundY + 1, RandomTile(fenceTiles));
        }

        for (int x = 1; x < 5; x++)
            SetTile(decorTm, x, groundY + 1, plantTiles.Count > 0 ? plantTiles[Random.Range(0, plantTiles.Count)] : null);
        for (int x = 5; x < 8; x++)
            SetTile(decorTm, x, groundY + 1, fenceTiles.Count > 0 ? fenceTiles[Random.Range(0, fenceTiles.Count)] : null);

        // ──────────────────────────────────────────────
        // Camera & Bootstrap
        // ──────────────────────────────────────────────
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.45f, 0.75f, 0.95f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        var oldBootstrap = GameObject.Find("TypingGameBootstrap");
        if (oldBootstrap != null)
            UnityEngine.Object.DestroyImmediate(oldBootstrap);

        var bootstrapObj = new GameObject("TypingGameBootstrap");
        bootstrapObj.AddComponent<TypingGameBootstrap>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = gridObj;
    }

    [MenuItem("OnlyToType/Clear Grid & Play Random %#p")]
    static void ClearGridAndPlay()
    {
        var grid = GameObject.Find("Grid");
        if (grid != null)
            UnityEngine.Object.DestroyImmediate(grid);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorApplication.EnterPlaymode();
    }

    static GameObject CreateTilemapLayer(GameObject parent, string name, int sortingOrder, bool withCollider, string layerName)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);

        var tm = obj.AddComponent<Tilemap>();
        var renderer = obj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;

        if (withCollider)
            obj.AddComponent<TilemapCollider2D>();

        obj.layer = LayerMask.NameToLayer(layerName);
        return obj;
    }
}
