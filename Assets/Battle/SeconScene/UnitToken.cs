using UnityEngine;

public enum ShapeType { Square, Circle }

[RequireComponent(typeof(SpriteRenderer))]
public class UnitToken : MonoBehaviour
{
    public float radius = 0.5f;

    private static Sprite squareSprite;
    private static Sprite circleSprite;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        EnsureSprites();
        if (sr.sprite == null) sr.sprite = squareSprite;
        ApplyScale();
    }

    public void Init(ShapeType shape, Color color, float radius, int sortingOrder = 0, string sortingLayer = "Default")
    {
        this.radius = Mathf.Max(0.01f, radius);
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        EnsureSprites();
        sr.sprite = (shape == ShapeType.Circle) ? circleSprite : squareSprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        sr.sortingLayerName = sortingLayer;
        ApplyScale();
    }

    private void ApplyScale()
    {
        float d = radius * 2f; // el sprite base es de 1 unidad (ppu=64), escalamos al diámetro
        transform.localScale = new Vector3(d, d, 1f);
    }

    // Sprites reutilizables
    public static Sprite GetSquareSprite(){ EnsureSprites(); return squareSprite; }
    public static Sprite GetCircleSprite(){ EnsureSprites(); return circleSprite; }

    private static void EnsureSprites()
    {
        if (squareSprite != null && circleSprite != null) return;

        const int size = 64, ppu = 64;

        // Cuadrado
        var sq = new Texture2D(size, size, TextureFormat.ARGB32, false);
        sq.wrapMode = TextureWrapMode.Clamp;
        sq.filterMode = FilterMode.Bilinear;
        var px = new Color32[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
        sq.SetPixels32(px);
        sq.Apply();
        squareSprite = Sprite.Create(sq, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);

        // Círculo (alpha fuera del radio)
        var ci = new Texture2D(size, size, TextureFormat.ARGB32, false);
        ci.wrapMode = TextureWrapMode.Clamp;
        ci.filterMode = FilterMode.Bilinear;
        float cx = (size - 1) * 0.5f, cy = (size - 1) * 0.5f, r = (size - 2) * 0.5f, r2 = r * r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d2 = dx * dx + dy * dy;

                float a = d2 <= r2 ? 1f : 0f;
                if (d2 > r2 && d2 < (r + 1f) * (r + 1f))
                {
                    float d = Mathf.Sqrt(d2);
                    a = Mathf.Clamp01(1f - (d - r));
                }
                ci.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        ci.Apply();
        circleSprite = Sprite.Create(ci, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
    }
}