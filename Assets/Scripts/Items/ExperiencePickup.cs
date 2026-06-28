using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ExperiencePickup : MonoBehaviour
{
    private const string PickupSortingLayer = "Objects";
    private const int PickupSortingOrder = 4;
    private const int ShadowSortingOrderOffset = -1;

    private static Sprite generatedSprite;
    private static AudioClip generatedCollectSound;

    [SerializeField] private int experienceValue = 1;
    [SerializeField] private float collectRadius = 0.45f;
    [SerializeField] private float magnetRadius = 2.4f;
    [SerializeField] private float magnetSpeed = 5f;
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobFrequency = 4.5f;
    [SerializeField] private float collectPopDuration = 0.18f;
    [SerializeField] private float collectPopScale = 1.45f;
    [SerializeField] private float collectFloatDistance = 0.18f;
    [SerializeField] private AudioClip collectSound;
    [SerializeField, Range(0f, 1f)] private float collectSoundVolume = 0.7f;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shadowRenderer;
    private CircleCollider2D pickupCollider;
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

        ConfigureSpriteRenderer(spriteRenderer, PickupSortingOrder, Color.white);
        EnsureShadowRenderer();

        pickupCollider = GetComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true;
        pickupCollider.radius = collectRadius;
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeSelf)
        {
            Vector3 playerPosition = PlayerController.Instance.transform.position;
            if (Vector2.Distance(transform.position, playerPosition) <= magnetRadius)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    playerPosition,
                    magnetSpeed * Time.deltaTime);
                startPosition = transform.position;
                return;
            }
        }

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
        PlayCollectSound();

        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        StartCoroutine(PlayCollectFeedback());
    }

    private IEnumerator PlayCollectFeedback()
    {
        Vector3 initialScale = transform.localScale;
        Vector3 initialPosition = transform.position;
        Color initialColor = spriteRenderer.color;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, collectPopDuration);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float pop = Mathf.Sin(t * Mathf.PI);
            transform.localScale = Vector3.LerpUnclamped(initialScale, initialScale * collectPopScale, pop);
            transform.position = initialPosition + Vector3.up * (collectFloatDistance * t);

            Color color = Color.Lerp(initialColor, Color.white, pop);
            color.a = Mathf.Lerp(initialColor.a, 0f, t);
            spriteRenderer.color = color;

            if (shadowRenderer != null)
            {
                Color shadowColor = shadowRenderer.color;
                shadowColor.a = Mathf.Lerp(0.35f, 0f, t);
                shadowRenderer.color = shadowColor;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void ConfigureSpriteRenderer(SpriteRenderer renderer, int sortingOrder, Color fallbackColor)
    {
        if (renderer == null)
        {
            return;
        }

        int sortingLayerId = SortingLayer.NameToID(PickupSortingLayer);
        if (sortingLayerId != 0)
        {
            renderer.sortingLayerID = sortingLayerId;
        }

        renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, sortingOrder);
        if (renderer.color.a <= 0.01f)
        {
            renderer.color = fallbackColor;
        }
    }

    private void EnsureShadowRenderer()
    {
        if (shadowRenderer != null || spriteRenderer == null)
        {
            return;
        }

        GameObject shadowObject = new GameObject("XP Pickup Shadow");
        shadowObject.transform.SetParent(transform, false);
        shadowObject.transform.localPosition = new Vector3(0.06f, -0.06f, 0f);
        shadowObject.transform.localScale = Vector3.one * 1.1f;

        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = spriteRenderer.sprite;
        shadowRenderer.color = new Color(0f, 0f, 0f, 0.35f);
        ConfigureSpriteRenderer(
            shadowRenderer,
            PickupSortingOrder + ShadowSortingOrderOffset,
            new Color(0f, 0f, 0f, 0.35f));
    }

    private void PlayCollectSound()
    {
        AudioClip soundToPlay = collectSound != null ? collectSound : GetGeneratedCollectSound();
        if (soundToPlay == null || collectSoundVolume <= 0f)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(soundToPlay, transform.position, collectSoundVolume);
    }

    private static AudioClip GetGeneratedCollectSound()
    {
        if (generatedCollectSound != null)
        {
            return generatedCollectSound;
        }

        const int sampleRate = 44100;
        const float duration = 0.16f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float normalized = i / (float)sampleCount;
            float envelope = Mathf.Sin(normalized * Mathf.PI);
            float frequency = Mathf.Lerp(880f, 1320f, normalized);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.35f;
        }

        generatedCollectSound = AudioClip.Create(
            "Generated XP Collect",
            sampleCount,
            1,
            sampleRate,
            false);
        generatedCollectSound.SetData(samples, 0);
        return generatedCollectSound;
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
