using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HealthPickup : MonoBehaviour
{
    private static Sprite generatedSprite;

    [SerializeField] private float healAmount = 2f;
    [SerializeField] private float lifetime = 8f;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobFrequency = 4f;

    private SpriteRenderer spriteRenderer;
    private float remainingLifetime;
    private Vector3 startPosition;

    public static HealthPickup Create(Vector3 position)
    {
        GameObject pickupObject = new GameObject("Health Pickup");
        pickupObject.transform.position = position;
        return pickupObject.AddComponent<HealthPickup>();
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetGeneratedSprite();
        }

        spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, 5);

        CircleCollider2D pickupCollider = GetComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true;
        pickupCollider.radius = 0.35f;

        remainingLifetime = lifetime;
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;

        if (remainingLifetime <= warningDuration)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.PingPong(Time.time * 6f, 0.7f) + 0.3f;
            spriteRenderer.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || PlayerController.Instance == null)
        {
            return;
        }

        PlayerController.Instance.Heal(healAmount);
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
        Color red = new Color(0.95f, 0.08f, 0.12f, 1f);
        Color darkRed = new Color(0.55f, 0.02f, 0.05f, 1f);
        Color white = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        FillRect(texture, 4, 3, 8, 10, darkRed);
        FillRect(texture, 3, 4, 10, 8, darkRed);
        FillRect(texture, 5, 4, 6, 8, red);
        FillRect(texture, 4, 5, 8, 6, red);
        FillRect(texture, 7, 5, 2, 6, white);
        FillRect(texture, 5, 7, 6, 2, white);

        texture.Apply();
        generatedSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);

        generatedSprite.name = "Generated Health Pickup";
        return generatedSprite;
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
