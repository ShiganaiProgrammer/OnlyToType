using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformerGameManager : MonoBehaviour
{
    const float DeathY = -8f;
    const int MaxLives = 3;
    const float DemoGoalDistance = 50f;

    AutoScrollPlayer player;
    PlatformerUI ui;
    LevelGenerator levelGenerator;
    AutoScrollCamera scrollCamera;
    TypingInputReader inputReader;
    AudioSource audioSource;
    bool isGameOver;
    bool isRespawning;
    bool isCleared;
    int lives;

    public void Initialize(
        AutoScrollPlayer scrollPlayer,
        PlatformerUI platformerUI,
        LevelGenerator generator,
        AutoScrollCamera camera)
    {
        player = scrollPlayer;
        ui = platformerUI;
        levelGenerator = generator;
        scrollCamera = camera;
        ui.Bind(player);
        player.OnHitObstacle += HandleHitObstacle;
        isGameOver = false;
        isRespawning = false;
        isCleared = false;
        lives = MaxLives;
        ui.UpdateLives(lives, MaxLives);

        audioSource = gameObject.AddComponent<AudioSource>();

        inputReader = new TypingInputReader();
        inputReader.OnEnterPressed += HandleEnterPressed;
        inputReader.Enable();
    }

    void Start()
    {
        if (GameObject.Find("Goal") == null)
            CreateGoal();
    }

    void OnDestroy()
    {
        if (player != null)
            player.OnHitObstacle -= HandleHitObstacle;

        if (inputReader == null) return;
        inputReader.OnEnterPressed -= HandleEnterPressed;
        inputReader.Dispose();
    }

    void HandleHitObstacle()
    {
        if (isGameOver) return;
        HandleDamage();
    }

    void HandleEnterPressed()
    {
        if (isGameOver || isCleared)
            Restart();
    }

    void Update()
    {
        inputReader?.Poll();

        if (isGameOver || isRespawning || player == null) return;

        if (player.IsFallen(DeathY))
            HandleDamage();
    }

    void HandleDamage()
    {
        isRespawning = true;
        player.enabled = false;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        lives--;
        ui.UpdateLives(lives, MaxLives);
        PlayHitSound();
        scrollCamera.Shake(0.2f, 0.15f);

        if (lives <= 0)
        {
            GameOver("ライフがなくなった！");
            return;
        }

        StartCoroutine(DelayedRespawn(1f));
    }

    IEnumerator DelayedRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetWorld();
        player.enabled = true;
        isRespawning = false;
    }

    void ResetWorld()
    {
        player.transform.position = new Vector3(0f, -0.9f, 0f);
        player.ResetRun();
        scrollCamera?.ResetPosition();

        var oldGoal = GameObject.Find("Goal");
        if (oldGoal != null)
            DestroyImmediate(oldGoal);

        var oldGrid = GameObject.Find("Grid");
        if (oldGrid != null)
            DestroyImmediate(oldGrid);

        if (levelGenerator != null)
            DestroyImmediate(levelGenerator.gameObject);

        var obj = new GameObject("LevelGenerator");
        levelGenerator = obj.AddComponent<LevelGenerator>();
        levelGenerator.Initialize(player.transform);
        CreateGoal();
    }

    void PlayHitSound()
    {
        int sampleRate = 44100;
        int samples = sampleRate / 10;
        var clip = AudioClip.Create("HitSound", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            data[i] = (Mathf.Sin(2 * Mathf.PI * 220 * t) + Mathf.Sin(2 * Mathf.PI * 180 * t)) * 0.3f * Mathf.Exp(-t * 25f);
        }
        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip);
    }

    float CalcGoalX()
    {
        var grid = GameObject.Find("Grid");
        if (grid != null)
        {
            var groundTm = grid.transform.Find("GroundTilemap")?.GetComponent<Tilemap>();
            if (groundTm != null)
            {
                var bounds = groundTm.cellBounds;
                if (bounds.xMax > 0)
                    return bounds.xMax - 1f;
            }
        }
        return DemoGoalDistance;
    }

    void CreateGoal()
    {
        float poleH = 3f;
        float flagW = 2f;
        float flagH = 1.25f;

        var goalX = CalcGoalX();
        var root = new GameObject("Goal");
        root.transform.position = new Vector3(goalX, -2.5f, 0f);

        var flag = new GameObject("Flag");
        flag.transform.SetParent(root.transform);
        var flagSr = flag.AddComponent<SpriteRenderer>();

        var flagTex = LoadFlagTexture();
        float renderW, renderH;
        if (flagTex != null)
        {
            float pixelsToUnits = flagTex.width / flagW;
            renderW = flagW;
            renderH = flagTex.height / pixelsToUnits;
            flagSr.sprite = Sprite.Create(flagTex, new Rect(0, 0, flagTex.width, flagTex.height), new Vector2(0.5f, 0.5f), pixelsToUnits);
            flagSr.color = Color.white;
        }
        else
        {
            renderW = flagW;
            renderH = flagH;
            flagSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            flagSr.color = new Color(1f, 0.84f, 0f);
        }
        flagSr.sortingOrder = 11;
        flag.transform.localPosition = new Vector3(renderW * 0.5f, poleH + renderH * 0.5f - 4f, 0f);

        var colliderH = poleH + renderH;
        var collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(renderW + 0.5f, colliderH);
        collider.offset = new Vector2(1f, colliderH * 0.5f - 4f);

        var goal = root.AddComponent<Goal>();
        goal.OnPlayerReached += HandleClear;
    }

    Texture2D LoadFlagTexture()
    {
        var path = System.IO.Path.Combine(Application.dataPath, "EifcM5ONk7BFsyO1761288807_1761288851.png");
        if (System.IO.File.Exists(path))
        {
            var bytes = System.IO.File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
            {
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            }
        }

        var fallback = new Texture2D(16, 16);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
                fallback.SetPixel(x, y, (x + y) % 2 == 0 ? Color.red : Color.white);
        fallback.Apply();
        return fallback;
    }

    void HandleClear()
    {
        isCleared = true;
        player.enabled = false;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.DisableInput();
        inputReader.Disable();
        ui.ShowClear(player.Distance);
    }

    void GameOver(string reason)
    {
        isGameOver = true;
        isRespawning = false;
        player.enabled = false;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.DisableInput();
        ui.ShowGameOver(player.Distance, reason);
    }

    void Restart()
    {
        isGameOver = false;
        isRespawning = false;
        isCleared = false;
        lives = MaxLives;
        ui.UpdateLives(lives, MaxLives);
        ui.HideGameOver();

        player.enabled = true;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.EnableInput();
        inputReader.Enable();
        ResetWorld();
    }
}
