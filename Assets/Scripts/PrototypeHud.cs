using UnityEngine;
using UnityEngine.UI;

public static class PrototypeHud
{
    public static Text CreateText(Transform parent, string objectName, Vector2 anchoredPosition, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        bool upperLeft = alignment == TextAnchor.UpperLeft;
        rectTransform.anchorMin = upperLeft ? new Vector2(0f, 1f) : new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = rectTransform.anchorMin;
        rectTransform.pivot = upperLeft ? new Vector2(0f, 1f) : new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = upperLeft ? new Vector2(520f, 160f) : new Vector2(900f, 180f);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
