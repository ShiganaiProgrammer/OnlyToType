using UnityEngine;

[DefaultExecutionOrder(-50)]
public class TypingGameBootstrap : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;

        var menuObj = new GameObject("GameMenu");
        var menu = menuObj.AddComponent<GameMenu>();
        menu.Show(StartStage);
    }

    void StartStage(GameMenu.Stage stage)
    {
        SetupCamera();

        if (stage == GameMenu.Stage.Random)
        {
            var existingGrid = GameObject.Find("Grid");
            if (existingGrid != null)
                Destroy(existingGrid);
        }

        SetupBackground();

        var player = SetupPlayer();
        var ui = SetupUI();

        LevelGenerator levelGen = null;
        if (stage == GameMenu.Stage.Random)
            levelGen = SetupLevelGenerator(player.transform);

        var camFollow = Camera.main.gameObject.AddComponent<AutoScrollCamera>();
        camFollow.SetTarget(player.transform);

        SetupManager(player, ui, levelGen, camFollow);
        player.EnableInput();
    }

    AutoScrollPlayer SetupPlayer()
    {
        var existing = GameObject.Find("Complete");
        GameObject playerObj;

        if (existing != null)
        {
            playerObj = existing;
            playerObj.name = "Player";
            playerObj.transform.position = new Vector3(0f, -0.9f, 0f);

            foreach (var col in playerObj.GetComponents<Collider2D>())
                Destroy(col);
        }
        else
        {
            playerObj = new GameObject("Player");
            playerObj.transform.position = new Vector3(0f, -0.9f, 0f);

            var sr = playerObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlayerSprite();
            sr.sortingOrder = 10;
        }

        var rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = playerObj.AddComponent<Rigidbody2D>();

        var circle = playerObj.GetComponent<CircleCollider2D>();
        if (circle == null) circle = playerObj.AddComponent<CircleCollider2D>();
        circle.radius = 0.45f;
        circle.offset = new Vector2(0f, -0.15f);

        var autoPlayer = playerObj.GetComponent<AutoScrollPlayer>() ?? playerObj.AddComponent<AutoScrollPlayer>();
        autoPlayer.DisableInput();
        return autoPlayer;
    }

    PlatformerUI SetupUI()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var uiObj = new GameObject("PlatformerUI");
        var ui = uiObj.AddComponent<PlatformerUI>();
        ui.Build(font);
        return ui;
    }

    LevelGenerator SetupLevelGenerator(Transform player)
    {
        var obj = new GameObject("LevelGenerator");
        var gen = obj.AddComponent<LevelGenerator>();
        gen.Initialize(player);
        return gen;
    }

    void SetupManager(AutoScrollPlayer player, PlatformerUI ui, LevelGenerator levelGen, AutoScrollCamera camera)
    {
        var obj = new GameObject("PlatformerGameManager");
        var manager = obj.AddComponent<PlatformerGameManager>();
        manager.Initialize(player, ui, levelGen, camera);
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    void SetupBackground()
    {
        if (GameObject.Find("ParallaxBG") != null) return;

        var bg = new GameObject("ParallaxBG");
        bg.AddComponent<BackgroundFollow>();
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = CreateBgSprite();
        sr.color = new Color(0.55f, 0.82f, 0.98f);
        sr.sortingOrder = -20;
        bg.transform.localScale = new Vector3(30f, 15f, 1f);
        bg.transform.position = new Vector3(0f, 0f, 1f);
    }

    static Sprite CreateBgSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    static Sprite CreatePlayerSprite()
    {
        var tex = new Texture2D(8, 12);
        for (int y = 0; y < 12; y++)
        for (int x = 0; x < 8; x++)
            tex.SetPixel(x, y, new Color(0.9f, 0.3f, 0.25f));

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 8, 12), new Vector2(0.5f, 0.5f), 8f);
    }
}
