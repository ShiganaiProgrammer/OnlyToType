using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlatformerUI : MonoBehaviour
{
    Text inputText;
    Text hintText;
    Text distanceText;
    Text statusText;
    GameObject gameOverPanel;
    GameObject clearPanel;

    readonly List<Image> heartImages = new();
    Coroutine blinkCoroutine;
    Sprite heartFillSprite;
    Sprite heartOutlineSprite;

    Font uiFont;
    AutoScrollPlayer player;

    public void Build(Font font)
    {
        uiFont = font;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<GraphicRaycaster>();

        inputText = CreateLabel("Input", new Vector2(0f, 350f), new Vector2(900f, 100f), 80, TextAnchor.MiddleCenter);
        inputText.color = new Color(1f, 0.75f, 0.4f);
        inputText.fontStyle = FontStyle.Bold;
        AddOutline(inputText);

        hintText = CreateLabel("Hint", new Vector2(0f, 270f), new Vector2(600f, 50f), 32, TextAnchor.MiddleCenter);
        hintText.color = new Color(1f, 0.92f, 0.2f);
        hintText.fontStyle = FontStyle.Bold;
        AddOutline(hintText);

        distanceText = CreateLabel("Distance", new Vector2(-750f, 480f), new Vector2(400f, 50f), 32, TextAnchor.MiddleLeft);
        AddOutline(distanceText);
        distanceText.gameObject.SetActive(false);

        statusText = CreateLabel("Status", new Vector2(0f, -450f), new Vector2(900f, 50f), 30, TextAnchor.MiddleCenter);
        AddOutline(statusText);

        LoadHeartSprites();
        CreateHeartDisplay(3);
        CreateGameOverPanel();
        CreateClearPanel();
    }

    void LoadHeartSprites()
    {
        string dataPath = Application.dataPath;

        string fillPath = dataPath + "/HeartSystem/Sprites/Fill.png";
        if (File.Exists(fillPath))
        {
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(fillPath));
            heartFillSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        string outlinePath = dataPath + "/HeartSystem/Sprites/Outline.png";
        if (File.Exists(outlinePath))
        {
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(outlinePath));
            heartOutlineSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }

    void CreateHeartDisplay(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject($"Heart_{i}");
            obj.transform.SetParent(transform, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-750f + i * 90f, 400f);
            rect.sizeDelta = new Vector2(80f, 80f);

            var img = obj.AddComponent<Image>();
            if (heartFillSprite != null)
                img.sprite = heartFillSprite;
            img.color = Color.red;
            img.raycastTarget = false;

            heartImages.Add(img);
        }
    }

    void AddOutline(Text text)
    {
        var outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    void CreateGameOverPanel()
    {
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(transform, false);
        gameOverPanel.SetActive(false);

        var rect = gameOverPanel.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700f, 350f);

        var bg = gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

        var title = CreateLabel("GameOverTitle", new Vector2(0f, 80f), new Vector2(600f, 70f), 44, TextAnchor.MiddleCenter);
        title.transform.SetParent(gameOverPanel.transform, false);
        title.text = "GAME OVER";
        title.color = new Color(1f, 0.45f, 0.45f);
        AddOutline(title);

        var retry = CreateLabel("Retry", new Vector2(0f, -20f), new Vector2(600f, 50f), 26, TextAnchor.MiddleCenter);
        retry.transform.SetParent(gameOverPanel.transform, false);
        retry.text = "Press ENTER to retry";
    }

    void CreateClearPanel()
    {
        clearPanel = new GameObject("ClearPanel");
        clearPanel.transform.SetParent(transform, false);
        clearPanel.SetActive(false);

        var rect = clearPanel.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700f, 350f);

        var bg = clearPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

        var title = CreateLabel("ClearTitle", new Vector2(0f, 80f), new Vector2(600f, 70f), 44, TextAnchor.MiddleCenter);
        title.transform.SetParent(clearPanel.transform, false);
        title.text = "CLEAR!";
        title.color = new Color(0.45f, 1f, 0.45f);
        AddOutline(title);

        var retry = CreateLabel("ClearRetry", new Vector2(0f, -20f), new Vector2(600f, 50f), 26, TextAnchor.MiddleCenter);
        retry.transform.SetParent(clearPanel.transform, false);
        retry.text = "Press ENTER to retry";
    }

    Text CreateLabel(string name, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(transform, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
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

    public void Bind(AutoScrollPlayer scrollPlayer)
    {
        if (player != null)
        {
            player.CommandInput.OnBufferChanged -= OnBufferChanged;
            player.CommandInput.OnCommandExecuted -= OnCommandExecuted;
        }

        player = scrollPlayer;
        player.CommandInput.OnBufferChanged += OnBufferChanged;
        player.CommandInput.OnCommandExecuted += OnCommandExecuted;
        OnBufferChanged("");
    }

    void OnDestroy()
    {
        if (player == null) return;
        player.CommandInput.OnBufferChanged -= OnBufferChanged;
        player.CommandInput.OnCommandExecuted -= OnCommandExecuted;
    }

    void OnBufferChanged(string buffer)
    {
        if (string.IsNullOrEmpty(buffer))
        {
            inputText.text = "▶";
            hintText.text = "";
            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(BlinkCursor());
            return;
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        inputText.enabled = true;
        inputText.text = buffer.ToUpper();
        string remaining = player.CommandInput.GetHint();
        hintText.text = string.IsNullOrEmpty(remaining) ? "" : $"+ {remaining.ToUpper()}";
    }

    IEnumerator BlinkCursor()
    {
        while (true)
        {
            inputText.enabled = !inputText.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnCommandExecuted(string command)
    {
        statusText.text = command switch
        {
            "jump" or "hop" => "JUMP!",
            "dash" or "run" => "DASH! Speed boost for 5 seconds!",
            _ => statusText.text
        };
    }

    void Update()
    {
    }

    public void UpdateLives(int current, int max)
    {
        for (int i = 0; i < heartImages.Count; i++)
            heartImages[i].enabled = i < current;
    }

    public void ShowClear(float distance)
    {
        clearPanel.SetActive(true);
        statusText.text = $"Distance: {Mathf.FloorToInt(distance)}m";
    }

    public void ShowGameOver(float distance, string reason)
    {
        gameOverPanel.SetActive(true);
        statusText.text = $"{reason}  Distance: {Mathf.FloorToInt(distance)}m - Enterでリトライ";
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
        clearPanel.SetActive(false);
        statusText.text = "";
    }
}
