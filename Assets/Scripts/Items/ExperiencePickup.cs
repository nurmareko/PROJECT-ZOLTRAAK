using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ExperiencePickup : MonoBehaviour
{
    private static Sprite generatedSprite;

    [SerializeField] private int experienceValue = 1;
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobFrequency = 4.5f;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private bool collected;

    public static ExperiencePickup Create(Vector3 position, int value)
    {
        GameObject pickupObject = new GameObject("Experience Pickup");
        pickupObject.transform.position = position;

        ExperiencePickup pickup = pickupObject.AddComponent<ExperiencePickup>();
        pickup.SetValue(value);
        return pickup;
    }

    public void SetValue(int value)
    {
        experienceValue = Mathf.Max(1, value);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetGeneratedSprite();
        }

        spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, 6);

        CircleCollider2D pickupCollider = GetComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true;
        pickupCollider.radius = 0.28f;
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (collected || !other.CompareTag("Player") || PlayerController.Instance == null)
        {
            return;
        }

        collected = true;
        PlayerController.Instance.GetExperience(experienceValue);
        Destroy(gameObject);
    }

    private static Sprite GetGeneratedSprite()
    {
        if (generatedSprite != null)
        {
            return generatedSprite;
        }

        const int size = 16;
        Texture2D texture = new Texture2D(size, size)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color darkBlue = new Color(0.06f, 0.16f, 0.8f, 1f);
        Color blue = new Color(0.1f, 0.45f, 1f, 1f);
        Color cyan = new Color(0.4f, 0.9f, 1f, 1f);
        Color white = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        FillDiamond(texture, 8, 8, 6, darkBlue);
        FillDiamond(texture, 8, 8, 5, blue);
        FillDiamond(texture, 7, 9, 3, cyan);
        FillRect(texture, 6, 10, 3, 2, white);

        texture.Apply();
        generatedSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);

        generatedSprite.name = "Generated Experience Pickup";
        return generatedSprite;
    }

    private static void FillDiamond(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY) <= radius)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void FillRect(Texture2D texture, int startX, int startY, int width, int height, Color color)
    {
        for (int y = startY; y < startY + height; y++)
        {
            for (int x = startX; x < startX + width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }
}
