using System.Collections.Generic;
using UnityEngine;

public class TypingGameManager : MonoBehaviour
{
    public static TypingGameManager Instance { get; private set; }

    const int MaxHp = 3;
    const float BaseSpawnInterval = 3.5f;
    const float MinSpawnInterval = 1.2f;
    const float BaseEnemySpeed = 1.2f;
    const float MaxEnemySpeed = 3.5f;

    enum GameState
    {
        Ready,
        Playing,
        GameOver
    }

    TypingUI ui;
    Transform enemyContainer;
    Font uiFont;
    GameObject playerVisual;

    readonly List<TypingEnemy> activeEnemies = new();

    GameState state = GameState.Ready;
    int score;
    int combo;
    int hp = MaxHp;
    int wave = 1;
    int defeatedCount;
    float spawnTimer;
    float spawnInterval = BaseSpawnInterval;
    float enemySpeed = BaseEnemySpeed;
    string lastInputFeedback = "";

    public void Initialize(TypingUI typingUI, Font font, GameObject existingPlayer = null)
    {
        ui = typingUI;
        uiFont = font;
        enemyContainer = ui.GetEnemyContainer();
        playerVisual = existingPlayer;

        if (playerVisual != null)
        {
            playerVisual.transform.position = new Vector3(-6f, -1.5f, 0f);
        }

        ShowReadyState();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        HandleInput();

        if (state != GameState.Playing) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (state == GameState.Ready || state == GameState.GameOver)
            {
                StartGame();
            }

            return;
        }

        if (state != GameState.Playing) return;

        if (Input.inputString.Length > 0)
        {
            foreach (char c in Input.inputString)
            {
                if (char.IsControl(c)) continue;
                ProcessChar(char.ToLower(c));
            }
        }
    }

    void ProcessChar(char c)
    {
        TypingEnemy target = GetFrontmostEnemy();
        if (target == null)
        {
            ShowFeedback("No target!");
            ResetCombo();
            return;
        }

        if (target.TryTypeChar(c))
        {
            lastInputFeedback = $"Typed: {c}";
            ShowFeedback(lastInputFeedback);
            TriggerPlayerAttack();
        }
        else
        {
            ShowFeedback($"Miss: {c}");
            ResetCombo();
        }
    }

    TypingEnemy GetFrontmostEnemy()
    {
        TypingEnemy front = null;
        float minX = float.MaxValue;

        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || enemy.IsDefeated) continue;

            var rect = enemy.GetComponent<RectTransform>();
            float x = rect != null ? rect.anchoredPosition.x : enemy.transform.position.x;
            if (x < minX)
            {
                minX = x;
                front = enemy;
            }
        }

        return front;
    }

    void SpawnEnemy()
    {
        var difficulty = WordDatabase.GetDifficultyForWave(wave);
        string word = WordDatabase.GetRandomWord(difficulty);

        var enemyObj = new GameObject($"Enemy_{word}");
        enemyObj.transform.SetParent(enemyContainer, false);
        enemyObj.AddComponent<RectTransform>();

        var enemy = enemyObj.AddComponent<TypingEnemy>();
        enemy.Initialize(word, 750f, Random.Range(-80f, 80f), enemySpeed, uiFont);

        activeEnemies.Add(enemy);
        UpdateInputPreview();
    }

    public void OnEnemyDefeated(TypingEnemy enemy)
    {
        activeEnemies.Remove(enemy);
        combo++;
        defeatedCount++;

        int points = 100 + combo * 10;
        score += points;

        ui.UpdateScore(score);
        ui.UpdateCombo(combo);
        ShowFeedback($"Defeated! +{points}");

        if (defeatedCount > 0 && defeatedCount % 5 == 0)
        {
            wave++;
            spawnInterval = Mathf.Max(MinSpawnInterval, BaseSpawnInterval - wave * 0.25f);
            enemySpeed = Mathf.Min(MaxEnemySpeed, BaseEnemySpeed + wave * 0.15f);
            ui.UpdateWave(wave);
            ShowFeedback($"Wave {wave}!");
        }

        UpdateInputPreview();
    }

    public void OnEnemyReachedPlayer(TypingEnemy enemy)
    {
        if (enemy.IsDefeated) return;

        activeEnemies.Remove(enemy);
        enemy.ForceDestroy();

        hp--;
        ResetCombo();
        ui.UpdateHp(hp, MaxHp);
        ShowFeedback("Enemy reached you! -1 HP");

        if (hp <= 0)
        {
            EndGame();
        }

        UpdateInputPreview();
    }

    void TriggerPlayerAttack()
    {
        if (playerVisual == null) return;

        StopAllCoroutines();
        StartCoroutine(AttackFlash());
    }

    System.Collections.IEnumerator AttackFlash()
    {
        var original = playerVisual.transform.localScale;
        playerVisual.transform.localScale = original * 1.2f;
        yield return new WaitForSeconds(0.08f);
        playerVisual.transform.localScale = original;
    }

    void ResetCombo()
    {
        combo = 0;
        ui.UpdateCombo(combo);
    }

    void ShowReadyState()
    {
        state = GameState.Ready;
        ui.HideGameOver();
        ui.UpdateScore(0);
        ui.UpdateCombo(0);
        ui.UpdateHp(MaxHp, MaxHp);
        ui.UpdateWave(1);
        ui.UpdateStatus("Press ENTER to start");
        ui.UpdateInputPreview("Type the word on the nearest enemy");
    }

    void StartGame()
    {
        ClearEnemies();

        score = 0;
        combo = 0;
        hp = MaxHp;
        wave = 1;
        defeatedCount = 0;
        spawnInterval = BaseSpawnInterval;
        enemySpeed = BaseEnemySpeed;
        spawnTimer = 1.5f;
        state = GameState.Playing;

        ui.HideGameOver();
        ui.UpdateScore(score);
        ui.UpdateCombo(combo);
        ui.UpdateHp(hp, MaxHp);
        ui.UpdateWave(wave);
        ui.UpdateStatus("Defeat enemies by typing their words!");
        ui.UpdateInputPreview("Focus on the leftmost enemy");
    }

    void EndGame()
    {
        state = GameState.GameOver;
        ui.ShowGameOver(score);
        ui.UpdateStatus("GAME OVER - Press ENTER to retry");
        ClearEnemies();
    }

    void ClearEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.ForceDestroy();
        }

        activeEnemies.Clear();
    }

    void ShowFeedback(string message)
    {
        ui.UpdateStatus(message);
    }

    void UpdateInputPreview()
    {
        var target = GetFrontmostEnemy();
        if (target == null)
        {
            ui.UpdateInputPreview("Waiting for next enemy...");
            return;
        }

        string remaining = target.Word.Substring(target.TypedIndex);
        ui.UpdateInputPreview($"Target: {remaining}");
    }
}
