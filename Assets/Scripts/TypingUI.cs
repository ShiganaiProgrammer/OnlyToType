using UnityEngine;
using UnityEngine.UI;

public class TypingUI : MonoBehaviour
{
    Text scoreText;
    Text comboText;
    Text hpText;
    Text waveText;
    Text statusText;
    Text inputPreviewText;
    GameObject gameOverPanel;
    Text gameOverScoreText;

    Font uiFont;

    public void BuildUI(Font font)
    {
        uiFont = font;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<GraphicRaycaster>();

        scoreText = CreateLabel("ScoreText", new Vector2(-750f, 480f), new Vector2(400f, 60f), 28, TextAnchor.MiddleLeft);
        comboText = CreateLabel("ComboText", new Vector2(-750f, 420f), new Vector2(400f, 50f), 24, TextAnchor.MiddleLeft);
        hpText = CreateLabel("HpText", new Vector2(-750f, 360f), new Vector2(400f, 50f), 24, TextAnchor.MiddleLeft);
        waveText = CreateLabel("WaveText", new Vector2(550f, 480f), new Vector2(400f, 60f), 28, TextAnchor.MiddleRight);
        statusText = CreateLabel("StatusText", new Vector2(0f, 420f), new Vector2(800f, 60f), 32, TextAnchor.MiddleCenter);
        inputPreviewText = CreateLabel("InputPreview", new Vector2(0f, -420f), new Vector2(900f, 50f), 26, TextAnchor.MiddleCenter);

        CreateTitleLabel();
        CreateGameOverPanel();
        CreateBattleField();
    }

    void CreateTitleLabel()
    {
        var title = CreateLabel("Title", new Vector2(0f, 300f), new Vector2(900f, 80f), 48, TextAnchor.MiddleCenter);
        title.text = "ONLY TO TYPE";
        title.color = new Color(1f, 0.9f, 0.4f);
    }

    void CreateBattleField()
    {
        var field = new GameObject("BattleField");
        field.transform.SetParent(transform, false);

        var rect = field.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, -50f);
        rect.sizeDelta = new Vector2(1600f, 300f);

        var bg = field.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.15f, 0.25f, 0.5f);
        bg.raycastTarget = false;

        var playerLine = CreateLabel("PlayerLine", new Vector2(-700f, -50f), new Vector2(200f, 40f), 20, TextAnchor.MiddleCenter);
        playerLine.transform.SetParent(field.transform, false);
        playerLine.text = "◀ PLAYER";
        playerLine.color = new Color(0.6f, 0.8f, 1f);

        var enemyLine = CreateLabel("EnemyLine", new Vector2(700f, -50f), new Vector2(200f, 40f), 20, TextAnchor.MiddleCenter);
        enemyLine.transform.SetParent(field.transform, false);
        enemyLine.text = "ENEMY ▶";
        enemyLine.color = new Color(1f, 0.6f, 0.6f);
    }

    void CreateGameOverPanel()
    {
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(transform, false);
        gameOverPanel.SetActive(false);

        var rect = gameOverPanel.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(700f, 400f);

        var bg = gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);

        var title = CreateLabel("GameOverTitle", new Vector2(0f, 100f), new Vector2(600f, 80f), 48, TextAnchor.MiddleCenter);
        title.transform.SetParent(gameOverPanel.transform, false);
        title.text = "GAME OVER";
        title.color = new Color(1f, 0.4f, 0.4f);

        gameOverScoreText = CreateLabel("GameOverScore", new Vector2(0f, 20f), new Vector2(600f, 60f), 32, TextAnchor.MiddleCenter);
        gameOverScoreText.transform.SetParent(gameOverPanel.transform, false);

        var retry = CreateLabel("RetryHint", new Vector2(0f, -80f), new Vector2(600f, 50f), 24, TextAnchor.MiddleCenter);
        retry.transform.SetParent(gameOverPanel.transform, false);
        retry.text = "Press ENTER to retry";
        retry.color = new Color(0.8f, 0.8f, 0.8f);
    }

    Text CreateLabel(string name, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(transform, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var text = obj.AddComponent<Text>();
        text.font = uiFont;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return text;
    }

    public void UpdateScore(int score) => scoreText.text = $"Score: {score}";
    public void UpdateCombo(int combo) => comboText.text = combo > 1 ? $"Combo x{combo}" : "";
    public void UpdateHp(int hp, int maxHp) => hpText.text = $"HP: {new string('♥', hp)}{new string('♡', maxHp - hp)}";
    public void UpdateWave(int wave) => waveText.text = $"Wave {wave}";
    public void UpdateStatus(string message) => statusText.text = message;
    public void UpdateInputPreview(string preview) => inputPreviewText.text = preview;

    public void ShowGameOver(int finalScore)
    {
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = $"Final Score: {finalScore}";
    }

    public void HideGameOver() => gameOverPanel.SetActive(false);

    public Transform GetEnemyContainer()
    {
        var field = transform.Find("BattleField");
        return field != null ? field : transform;
    }
}
