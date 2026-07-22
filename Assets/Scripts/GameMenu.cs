using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public enum Stage { Tutorial, Random }
    System.Action<Stage> onStart;

    public void Show(System.Action<Stage> onStartCallback)
    {
        onStart = onStartCallback;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<GraphicRaycaster>();

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var bgObj = new GameObject("MenuBG");
        bgObj.transform.SetParent(transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(1920, 1080);
        var bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        var title = CreateLabel("Title", new Vector2(0f, 160f), new Vector2(900f, 100f), 72, TextAnchor.MiddleCenter, font);
        title.transform.SetParent(transform, false);
        title.text = "ONLY TO TYPE";
        title.color = new Color(1f, 0.75f, 0.4f);
        title.fontStyle = FontStyle.Bold;
        AddOutline(title);

        var opt1 = CreateLabel("Opt1", new Vector2(0f, 20f), new Vector2(600f, 50f), 32, TextAnchor.MiddleCenter, font);
        opt1.transform.SetParent(transform, false);
        opt1.text = "1: Tutorial";
        opt1.color = new Color(0.7f, 0.9f, 1f);
        AddOutline(opt1);

        var opt2 = CreateLabel("Opt2", new Vector2(0f, -50f), new Vector2(600f, 50f), 32, TextAnchor.MiddleCenter, font);
        opt2.transform.SetParent(transform, false);
        opt2.text = "2: Random Stage";
        opt2.color = new Color(0.7f, 0.9f, 1f);
        AddOutline(opt2);

        var hint = CreateLabel("Hint", new Vector2(0f, -140f), new Vector2(700f, 40f), 20, TextAnchor.MiddleCenter, font);
        hint.transform.SetParent(transform, false);
        hint.text = "Press number key to start";
        hint.color = new Color(0.6f, 0.6f, 0.6f);
        AddOutline(hint);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            onStart?.Invoke(Stage.Tutorial);
            Destroy(gameObject);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            onStart?.Invoke(Stage.Random);
            Destroy(gameObject);
        }
    }

    Text CreateLabel(string name, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor, Font font)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(transform, false);

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

        return text;
    }

    void AddOutline(Text text)
    {
        var outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);
    }
}
