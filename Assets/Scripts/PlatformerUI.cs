using UnityEngine;
using UnityEngine.UI;

public class PlatformerUI : MonoBehaviour
{
    Text titleText;
    Text inputText;
    Text hintText;
    Text distanceText;
    Text dashText;
    Text statusText;
    GameObject gameOverPanel;

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

        titleText = CreateLabel("Title", new Vector2(0f, 460f), new Vector2(800f, 60f), 36, TextAnchor.MiddleCenter);
        titleText.text = "ONLY TO TYPE - Auto Runner";
        titleText.color = new Color(1f, 0.92f, 0.5f);
        AddOutline(titleText);

        inputText = CreateLabel("Input", new Vector2(0f, 380f), new Vector2(600f, 50f), 36, TextAnchor.MiddleCenter);
        AddOutline(inputText);

        hintText = CreateLabel("Hint", new Vector2(0f, 330f), new Vector2(600f, 40f), 28, TextAnchor.MiddleCenter);
        hintText.color = new Color(0.7f, 0.9f, 1f);
        AddOutline(hintText);

        distanceText = CreateLabel("Distance", new Vector2(-750f, 460f), new Vector2(400f, 50f), 32, TextAnchor.MiddleLeft);
        AddOutline(distanceText);

        dashText = CreateLabel("Dash", new Vector2(550f, 460f), new Vector2(400f, 50f), 32, TextAnchor.MiddleRight);
        AddOutline(dashText);

        statusText = CreateLabel("Status", new Vector2(0f, -450f), new Vector2(900f, 50f), 30, TextAnchor.MiddleCenter);
        statusText.text = "jump/hopでジャンプ / run/dashで加速";
        AddOutline(statusText);

        CreateCommandGuide();
        CreateGameOverPanel();
    }

    void AddOutline(Text text)
    {
        var outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    void CreateCommandGuide()
    {
        var guide = CreateLabel("Guide", new Vector2(0f, 260f), new Vector2(700f, 80f), 24, TextAnchor.MiddleCenter);
        guide.text = "<color=#88FF88>jump</color> / <color=#88FF88>hop</color> = Jump over gaps\n<color=#FFCC66>dash</color> / <color=#FFCC66>run</color> = Speed x2 for 5s";
        guide.supportRichText = true;
        guide.color = new Color(0.85f, 0.85f, 0.85f);
        AddOutline(guide);
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
            inputText.text = "_";
            hintText.text = "";
            return;
        }

        inputText.text = buffer;
        string remaining = player.CommandInput.GetHint();
        hintText.text = string.IsNullOrEmpty(remaining) ? "" : $"+ {remaining}";
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
        if (player == null) return;

        distanceText.text = $"Distance: {Mathf.FloorToInt(player.Distance)}m";

        if (player.IsDashing)
            dashText.text = $"DASH: {player.DashTimeRemaining:F1}s";
        else
            dashText.text = "";
    }

    public void ShowGameOver(float distance, string reason)
    {
        gameOverPanel.SetActive(true);
        statusText.text = $"{reason}  Distance: {Mathf.FloorToInt(distance)}m - Enterでリトライ";
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
        statusText.text = "jump/hopでジャンプ / run/dashで加速";
    }
}
