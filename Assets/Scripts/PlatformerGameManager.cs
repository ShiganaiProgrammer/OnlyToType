using UnityEngine;

public class PlatformerGameManager : MonoBehaviour
{
    const float DeathY = -8f;

    AutoScrollPlayer player;
    PlatformerUI ui;
    LevelGenerator levelGenerator;
    AutoScrollCamera scrollCamera;
    TypingInputReader inputReader;
    bool isGameOver;

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

        inputReader = new TypingInputReader();
        inputReader.OnEnterPressed += HandleEnterPressed;
        inputReader.Enable();
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
        GameOver("障害物にぶつかった！");
    }

    void HandleEnterPressed()
    {
        if (isGameOver)
            Restart();
    }

    void Update()
    {
        inputReader?.Poll();

        if (isGameOver || player == null) return;

        if (player.IsFallen(DeathY))
            GameOver("落ちてしまった！");
    }

    void GameOver(string reason)
    {
        isGameOver = true;
        player.enabled = false;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        ui.ShowGameOver(player.Distance, reason);
    }

    void Restart()
    {
        isGameOver = false;
        ui.HideGameOver();

        player.enabled = true;
        player.transform.position = new Vector3(0f, -0.9f, 0f);
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.ResetRun();
        scrollCamera?.ResetPosition();

        if (levelGenerator != null)
        {
            if (levelGenerator.gameObject != null)
                DestroyImmediate(levelGenerator.gameObject);
            var oldGrid = GameObject.Find("Grid");
            if (oldGrid != null)
                DestroyImmediate(oldGrid);

            var obj = new GameObject("LevelGenerator");
            levelGenerator = obj.AddComponent<LevelGenerator>();
            levelGenerator.Initialize(player.transform);
        }
    }
}
