using UnityEngine;

public static class SpriteFactory
{
    public static Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = (size - 2) * 0.5f;
        float radiusSqr = radius * radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 offset = new Vector2(x, y) - center;
                pixels[y * size + x] = offset.sqrMagnitude <= radiusSqr ? color : clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateBoxSprite(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
    }

    public static Sprite CreateTriangleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float rowWidth = Mathf.Lerp(4f, size - 4f, y / (float)(size - 1));
            int minX = Mathf.RoundToInt((size - rowWidth) * 0.5f);
            int maxX = Mathf.RoundToInt((size + rowWidth) * 0.5f);

            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = x >= minX && x <= maxX ? color : clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
