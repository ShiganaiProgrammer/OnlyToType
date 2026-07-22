using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class WaypointHint
    {
        public float distanceThreshold;
        [TextArea(2, 4)] public string message;
    }

    public AutoScrollPlayer player;
    public PlatformerUI ui;
    public WaypointHint[] hints;
    public float goalX = 100f;

    const float DeathY = -8f;
    const int MaxLives = 3;
    int lives = MaxLives;

    Text instructionText;
    string skipBuffer;
    bool isComplete;
    bool isSkipped;
    bool isGameOver;
    bool isRespawning;
    int nextHintIndex;

    void Start()
    {
        CreateInstructionUI();
        CreateGoal();
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += HandleTextInput;
        if (ui != null)
            ui.UpdateLives(lives, MaxLives);
        ShowMessage("チュートリアルを始めます！ SKIPと入力でスキップ可能");
        StartCoroutine(HideAfterDelay(3f));
    }

    System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMessage();
    }

    void OnDestroy()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleTextInput;
    }

    void HandleTextInput(char c)
    {
        if (isComplete || isSkipped || isGameOver) return;
        skipBuffer += char.ToLower(c);
        if (skipBuffer.Length > 4)
            skipBuffer = skipBuffer.Substring(skipBuffer.Length - 4);
        if (skipBuffer == "skip")
            SkipTutorial();
    }

    void SkipTutorial()
    {
        isSkipped = true;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    void CreateGoal()
    {
        float poleH = 3f;
        float flagW = 2f;
        float flagH = 1.25f;

        var root = new GameObject("Goal");
        root.transform.position = new Vector3(goalX, -2.5f, 0f);

        var flagTex = LoadFlagTexture();
        float renderW = flagW;
        float renderH = flagH;
        if (flagTex != null)
        {
            float pixelsToUnits = flagTex.width / flagW;
            renderW = flagW;
            renderH = flagTex.height / pixelsToUnits;
            var flag = new GameObject("Flag");
            flag.transform.SetParent(root.transform);
            var flagSr = flag.AddComponent<SpriteRenderer>();
            flagSr.sprite = Sprite.Create(flagTex, new Rect(0, 0, flagTex.width, flagTex.height), new Vector2(0.5f, 0.5f), pixelsToUnits);
            flagSr.color = Color.white;
            flagSr.sortingOrder = 11;
            flag.transform.localPosition = new Vector3(renderW * 0.5f, poleH + renderH * 0.5f - 4f, 0f);
        }

        var colliderH = poleH + renderH;
        var collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(renderW + 0.5f, colliderH);
        collider.offset = new Vector2(1f, colliderH * 0.5f - 4f);

        var goal = root.AddComponent<Goal>();
        goal.OnPlayerReached += HandleGoalReached;
    }

    void HandleGoalReached()
    {
        isComplete = true;
        player.enabled = false;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        player.DisableInput();
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        if (ui != null)
            ui.ShowClear(player.Distance);
    }

    void CreateInstructionUI()
    {
        var canvasObj = new GameObject("TutorialCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        instructionText = CreateText(canvasObj.transform, "Instruction", font,
            new Vector2(0f, -400f), new Vector2(1400f, 120f), 45, TextAnchor.MiddleCenter);
        instructionText.gameObject.SetActive(false);
    }

    Text CreateText(Transform parent, string name, Font font, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        var text = obj.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        var outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);

        return text;
    }

    void Update()
    {
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame))
        {
            if (isComplete || isSkipped)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
            if (isGameOver)
            {
                RestartTutorial();
                return;
            }
        }

        if (isComplete || isSkipped || isGameOver || isRespawning) return;
        if (player == null) return;

        if (player.transform.position.y < DeathY)
        {
            HandleDeath();
            return;
        }

        if (nextHintIndex < hints.Length &&
            player.Distance >= hints[nextHintIndex].distanceThreshold)
        {
            ShowMessage(hints[nextHintIndex].message);
            nextHintIndex++;
        }
    }

    void HandleDeath()
    {
        HideMessage();
        isRespawning = true;
        player.enabled = false;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        lives--;
        if (ui != null)
            ui.UpdateLives(lives, MaxLives);

        var cam = Camera.main?.GetComponent<AutoScrollCamera>();
        if (cam != null) cam.Shake(0.2f, 0.15f);

        if (lives <= 0)
        {
            isGameOver = true;
            player.DisableInput();
            if (ui != null)
                ui.ShowGameOver(player.Distance, "ライフがなくなった！");
            return;
        }

        ShowMessage($"穴に落ちた！ 残りライフ: {lives}");
        StartCoroutine(DelayedRespawn(1f));
    }

    System.Collections.IEnumerator DelayedRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        player.transform.position = new Vector3(0f, -0.9f, 0f);
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = new Vector2(0f, -0.9f);
            rb.linearVelocity = Vector2.zero;
        }
        player.ResetRun();
        player.enabled = true;
        isRespawning = false;
        nextHintIndex = 0;
        var cam = Camera.main?.GetComponent<AutoScrollCamera>();
        if (cam != null) cam.ResetPosition();
        HideMessage();
    }

    void RestartTutorial()
    {
        isGameOver = false;
        isRespawning = false;
        lives = MaxLives;
        nextHintIndex = 0;
        if (ui != null)
        {
            ui.UpdateLives(lives, MaxLives);
            ui.HideGameOver();
        }
        player.EnableInput();
        player.enabled = true;
        player.transform.position = new Vector3(0f, -0.9f, 0f);
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = new Vector2(0f, -0.9f);
            rb.linearVelocity = Vector2.zero;
        }
        player.ResetRun();
        var cam = Camera.main?.GetComponent<AutoScrollCamera>();
        if (cam != null) cam.ResetPosition();
        HideMessage();
    }

    void ShowMessage(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
            instructionText.gameObject.SetActive(true);
        }
    }

    void HideMessage()
    {
        if (instructionText != null)
            instructionText.gameObject.SetActive(false);
    }
}
