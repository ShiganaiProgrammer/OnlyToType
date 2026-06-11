using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] float groundY = -4f;
    [SerializeField] float lookahead = 40f;
    [SerializeField] float cleanupBehind = 20f;

    [Header("Tile References")]
    public TileBase groundTile;
    public TileBase undergroundTile;
    public TileBase obstacleTile;
    public TileBase[] backgroundTiles;

    Transform player;
    Tilemap groundTilemap;
    Tilemap backgroundTilemap;

    readonly List<PlacedRegion> placedGround = new();
    readonly List<PlacedRegion> placedUnderground = new();
    readonly List<PlacedRegion> placedBackground = new();

    float nextSpawnX;
    float lastPlayerX;

    struct PlacedRegion
    {
        public int startX;
        public int endX;
        public int y;
    }

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        CreateTilemapHierarchy();
        LoadTiles();
        PlaceGroundTiles(-1f, 14f);
        nextSpawnX = 13f;
        for (int i = 0; i < 5; i++)
            SpawnNextSegment();
    }

    void CreateTilemapHierarchy()
    {
        var existing = GameObject.Find("Grid");
        if (existing != null)
        {
            AssignTilemaps(existing.transform);
            return;
        }

        var gridObj = new GameObject("Grid");
        var grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        var ground = new GameObject("GroundTilemap");
        ground.transform.SetParent(gridObj.transform);
        groundTilemap = ground.AddComponent<Tilemap>();
        var groundRenderer = ground.AddComponent<TilemapRenderer>();
        groundRenderer.sortingOrder = 0;
        var groundCollider = ground.AddComponent<TilemapCollider2D>();
        ground.layer = LayerMask.NameToLayer("Ground");

        var bg = new GameObject("BackgroundTilemap");
        bg.transform.SetParent(gridObj.transform);
        backgroundTilemap = bg.AddComponent<Tilemap>();
        var bgRenderer = bg.AddComponent<TilemapRenderer>();
        bgRenderer.sortingOrder = -2;
    }

    void AssignTilemaps(Transform gridTransform)
    {
        foreach (Transform child in gridTransform)
        {
            var tm = child.GetComponent<Tilemap>();
            if (tm == null) continue;
            if (child.name == "GroundTilemap")
                groundTilemap = tm;
            else if (child.name == "BackgroundTilemap")
                backgroundTilemap = tm;
        }
    }

    void LoadTiles()
    {
#if UNITY_EDITOR
        var basePath = "Assets/2D Pixel Art Platformer Biome - American Forest/Tilemap";

        if (groundTile == null)
            groundTile = UnityEditor.AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileGround2.asset");
        if (undergroundTile == null)
            undergroundTile = UnityEditor.AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileGround5.asset");
        if (obstacleTile == null)
            obstacleTile = UnityEditor.AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileSpikes.asset");

        if (backgroundTiles == null || backgroundTiles.Length == 0)
        {
            var loaded = new List<TileBase>();
            for (int i = 1; i <= 13; i++)
            {
                var tile = UnityEditor.AssetDatabase.LoadAssetAtPath<TileBase>($"{basePath}/TileBackGround{i}.asset");
                if (tile != null) loaded.Add(tile);
            }
            backgroundTiles = loaded.ToArray();
        }
#endif
    }

    void Update()
    {
        if (player == null) return;
        lastPlayerX = player.position.x;
        while (nextSpawnX < lastPlayerX + lookahead)
            SpawnNextSegment();
        CleanupOldTiles();
    }

    void SpawnNextSegment()
    {
        int pattern = Random.Range(0, 100);

        if (pattern < 45)
            SpawnFlatGround(Random.Range(4f, 8f));
        else if (pattern < 65)
            SpawnGap(Random.Range(2.5f, 4.5f));
        else if (pattern < 80)
            SpawnFloatingPlatform();
        else if (pattern < 92)
            SpawnStairPlatforms();
        else
            SpawnFlatGround(Random.Range(6f, 10f));
    }

    void SpawnFlatGround(float width)
    {
        float startX = nextSpawnX;
        PlaceGroundTiles(nextSpawnX, width);
        nextSpawnX += width;
        TrySpawnDecorations(width);
        PlaceBackgroundAbove(startX, nextSpawnX);
    }

    void SpawnGap(float width)
    {
        nextSpawnX += width;
        float landingWidth = Random.Range(3f, 6f);
        PlaceGroundTiles(nextSpawnX, landingWidth);
        nextSpawnX += landingWidth;
    }

    void SpawnFloatingPlatform()
    {
        SpawnFlatGround(Random.Range(3f, 5f));

        float width = Random.Range(2f, 4f);
        float height = groundY + Random.Range(1.5f, 3f);
        PlaceFloatingTiles(nextSpawnX, width, height);
        nextSpawnX += width + Random.Range(1f, 2f);

        SpawnFlatGround(Random.Range(3f, 5f));
    }

    void SpawnStairPlatforms()
    {
        SpawnFlatGround(3f);

        for (int i = 0; i < 3; i++)
        {
            float width = 2.5f;
            float height = groundY + 1f + i * 1.2f;
            PlaceFloatingTiles(nextSpawnX, width, height);
            nextSpawnX += width;
        }

        SpawnFlatGround(Random.Range(4f, 6f));
    }

    void PlaceGroundTiles(float startX, float width)
    {
        if (groundTilemap == null || groundTile == null) return;

        int start = Mathf.RoundToInt(startX);
        int end = Mathf.RoundToInt(startX + width);
        int y = Mathf.RoundToInt(groundY);

        for (int x = start; x < end; x++)
            groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);

        placedGround.Add(new PlacedRegion { startX = start, endX = end, y = y });
        PlaceUndergroundTiles(start, end);
    }

    void PlaceUndergroundTiles(int start, int end)
    {
        if (groundTilemap == null || undergroundTile == null) return;
        int groundYInt = Mathf.RoundToInt(groundY);
        for (int y = groundYInt - 1; y >= groundYInt - 5; y--)
        {
            for (int x = start; x < end; x++)
                groundTilemap.SetTile(new Vector3Int(x, y, 0), undergroundTile);
            placedUnderground.Add(new PlacedRegion { startX = start, endX = end, y = y });
        }
    }

    void PlaceFloatingTiles(float startX, float width, float height)
    {
        if (groundTilemap == null || groundTile == null) return;

        int start = Mathf.RoundToInt(startX);
        int end = Mathf.RoundToInt(startX + width);
        int y = Mathf.RoundToInt(height);

        for (int x = start; x < end; x++)
            groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);

        placedGround.Add(new PlacedRegion { startX = start, endX = end, y = y });
    }

    void TrySpawnDecorations(float platformWidth)
    {
        if (obstacleTile == null) return;
        if (platformWidth < 4f || Random.value > 0.55f) return;

        int count = platformWidth > 6f && Random.value < 0.35f ? 2 : 1;
        float segmentStart = nextSpawnX - platformWidth;

        for (int i = 0; i < count; i++)
        {
            float x = segmentStart + Random.Range(1.5f, platformWidth - 1.5f);
            int tileX = Mathf.RoundToInt(x);
            int tileY = Mathf.RoundToInt(groundY + 1f);
            backgroundTilemap?.SetTile(new Vector3Int(tileX, tileY, 0), obstacleTile);
        }
    }

    void PlaceBackgroundAbove(float startX, float endX)
    {
        if (backgroundTilemap == null || backgroundTiles == null || backgroundTiles.Length == 0) return;

        int bgY = Mathf.RoundToInt(groundY - 1f);
        int s = Mathf.RoundToInt(startX);
        int e = Mathf.RoundToInt(endX);

        for (int x = s; x < e; x++)
        {
            if (Random.value < 0.3f)
                backgroundTilemap.SetTile(new Vector3Int(x, bgY, 0),
                    backgroundTiles[Random.Range(0, backgroundTiles.Length)]);
        }

        placedBackground.Add(new PlacedRegion { startX = s, endX = e, y = bgY });
    }

    void CleanupOldTiles()
    {
        float cleanupX = lastPlayerX - cleanupBehind;
        int limit = Mathf.FloorToInt(cleanupX);

        CleanupList(placedGround, groundTilemap, limit);
        CleanupList(placedUnderground, groundTilemap, limit);
        CleanupList(placedBackground, backgroundTilemap, limit);
    }

    void CleanupList(List<PlacedRegion> list, Tilemap tilemap, int limit)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var r = list[i];
            if (r.endX <= limit)
            {
                for (int x = r.startX; x < r.endX; x++)
                    tilemap?.SetTile(new Vector3Int(x, r.y, 0), null);
                list.RemoveAt(i);
            }
        }
    }
}
