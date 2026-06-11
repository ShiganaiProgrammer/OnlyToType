using UnityEngine;
using UnityEngine.UI;

public class TypingEnemy : MonoBehaviour
{
    public string Word { get; private set; }
    public int TypedIndex { get; private set; }
    public bool IsDefeated { get; private set; }

    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float defeatFadeSpeed = 4f;

    Text wordText;
    Image bodyImage;
    RectTransform rectTransform;
    float targetX;
    bool isActive;

    public void Initialize(string word, float spawnX, float y, float speed, Font font)
    {
        Word = word;
        TypedIndex = 0;
        IsDefeated = false;
        moveSpeed = speed * 100f;
        isActive = true;
        targetX = -750f;

        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(spawnX, y);

        bodyImage = gameObject.AddComponent<Image>();
        bodyImage.color = new Color(0.85f, 0.25f, 0.3f, 1f);
        bodyImage.raycastTarget = false;

        rectTransform.sizeDelta = new Vector2(80f, 80f);

        var labelObj = new GameObject("WordLabel");
        labelObj.transform.SetParent(transform, false);

        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(0f, 55f);
        labelRect.sizeDelta = new Vector2(200f, 40f);

        wordText = labelObj.AddComponent<Text>();
        wordText.font = font;
        wordText.fontSize = 22;
        wordText.alignment = TextAnchor.MiddleCenter;
        wordText.raycastTarget = false;
        wordText.color = Color.white;

        UpdateWordDisplay();
    }

    void Update()
    {
        if (!isActive || IsDefeated) return;

        var pos = rectTransform.anchoredPosition;
        pos.x -= moveSpeed * Time.deltaTime;
        rectTransform.anchoredPosition = pos;

        if (pos.x <= targetX)
        {
            TypingGameManager.Instance?.OnEnemyReachedPlayer(this);
        }
    }

    public bool TryTypeChar(char c)
    {
        if (!isActive || IsDefeated || TypedIndex >= Word.Length) return false;

        if (Word[TypedIndex] == c)
        {
            TypedIndex++;
            UpdateWordDisplay();

            if (TypedIndex >= Word.Length)
            {
                Defeat();
                return true;
            }

            return true;
        }

        return false;
    }

    void UpdateWordDisplay()
    {
        if (wordText == null) return;

        string display = "";
        for (int i = 0; i < Word.Length; i++)
        {
            if (i < TypedIndex)
                display += $"<color=#88FF88>{Word[i]}</color>";
            else if (i == TypedIndex)
                display += $"<color=#FFFF66>{Word[i]}</color>";
            else
                display += $"<color=#FFFFFF>{Word[i]}</color>";
        }

        wordText.supportRichText = true;
        wordText.text = display;
    }

    void Defeat()
    {
        IsDefeated = true;
        TypingGameManager.Instance?.OnEnemyDefeated(this);
        StartCoroutine(FadeOutAndDestroy());
    }

    System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= defeatFadeSpeed * Time.deltaTime;
            if (bodyImage != null)
                bodyImage.color = new Color(bodyImage.color.r, bodyImage.color.g, bodyImage.color.b, alpha);
            if (wordText != null)
                wordText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    public void ForceDestroy()
    {
        isActive = false;
        Destroy(gameObject);
    }
}
