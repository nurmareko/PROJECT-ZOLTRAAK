using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameFeelFeedback : MonoBehaviour
{
    private const int EffectSortingOrder = 24;
    private const float XpTrailMinInterval = 0.08f;

    private static GameFeelFeedback instance;

    [Header("Camera Feedback")]
    [SerializeField] private bool enableCameraFeedback = true;
    [SerializeField, Range(0f, 1f)] private float impulseListenerGain = 0.65f;
    [SerializeField, Range(0.02f, 0.5f)] private float impulseDuration = 0.12f;
    [SerializeField, Range(0f, 0.5f)] private float playerDamageImpulse = 0.1f;
    [SerializeField, Range(0f, 0.5f)] private float shieldImpulse = 0.055f;
    [SerializeField, Range(0f, 0.5f)] private float repelImpulse = 0.14f;
    [SerializeField, Range(0f, 0.5f)] private float lungeImpulse = 0.11f;
    [SerializeField] private bool enableEnemyDeathImpulse;
    [SerializeField, Range(0f, 0.25f)] private float enemyDeathImpulse = 0.025f;
    [SerializeField, Range(0.02f, 1f)] private float enemyDeathImpulseCooldown = 0.28f;

    [Header("Atmosphere")]
    [SerializeField] private bool enableAmbientMotes = true;
    [SerializeField, Range(0f, 16f)] private float ambientEmissionRate = 6f;
    [SerializeField, Range(0, 200)] private int ambientMaxParticles = 70;
    [SerializeField] private Vector3 ambientAreaSize = new Vector3(22f, 15f, 0.1f);
    [SerializeField] private Vector2 ambientLifetimeRange = new Vector2(6f, 10f);
    [SerializeField] private Vector2 ambientSpeedRange = new Vector2(0.04f, 0.12f);
    [SerializeField] private Vector2 ambientSizeRange = new Vector2(0.3f, 0.65f);
    [SerializeField] private Vector2 ambientHorizontalDrift = new Vector2(-0.06f, 0.06f);
    [SerializeField] private Vector2 ambientVerticalDrift = new Vector2(0.03f, 0.12f);
    [SerializeField] private Color ambientMistColor = new Color(0.78f, 0.88f, 1f, 0.95f);
    [SerializeField] private Color ambientEmberColor = new Color(1f, 0.66f, 0.32f, 0.95f);
    // Motes live on the ground's sorting layer ("Background") but at a high order so they draw
    // above the terrain tiles, while still sitting behind actors on the "Objects" layer
    // (player, enemies, pickups) so they never hide gameplay.
    [SerializeField] private string ambientSortingLayer = "Background";
    [SerializeField] private int ambientSortingOrder = 100;

    [Header("Vignette")]
    [SerializeField] private bool enableVignette = true;
    [SerializeField, Range(0f, 1f)] private float vignetteIntensity = 0.42f;
    [SerializeField, Range(0.01f, 1f)] private float vignetteSmoothness = 0.45f;
    [SerializeField] private Color vignetteColor = new Color(0.02f, 0.01f, 0.03f, 1f);

    private CinemachineImpulseSource impulseSource;
    private CinemachineImpulseListener impulseListener;
    private ParticleSystem ambientParticles;
    private Transform ambientMotes;
    private Material ambientMaterial;
    private Texture2D ambientTexture;
    private Volume atmosphereVolume;
    private VolumeProfile atmosphereProfile;
    private Vignette vignette;
    private float lastEnemyDeathImpulseTime = -999f;
    private float lastXpTrailTime;

    public static GameFeelFeedback Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameFeelFeedback>();
            }

            if (instance == null)
            {
                GameObject feedbackObject = new GameObject("Game Feel Feedback (Runtime)");
                instance = feedbackObject.AddComponent<GameFeelFeedback>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ConfigureImpulse();
    }

    private void Start()
    {
        if (enableAmbientMotes)
        {
            CreateAmbientMotes();
        }

        if (enableVignette)
        {
            SetupVignette();
        }
    }

    private void OnValidate()
    {
        ambientMaxParticles = Mathf.Max(0, ambientMaxParticles);
        ambientAreaSize.x = Mathf.Max(0.1f, ambientAreaSize.x);
        ambientAreaSize.y = Mathf.Max(0.1f, ambientAreaSize.y);
        ambientAreaSize.z = Mathf.Max(0.01f, ambientAreaSize.z);
        ambientLifetimeRange = NormalizeRange(ambientLifetimeRange, 0.1f);
        ambientSpeedRange = NormalizeRange(ambientSpeedRange, 0f);
        ambientSizeRange = NormalizeRange(ambientSizeRange, 0.005f);
        ambientHorizontalDrift = NormalizeRange(ambientHorizontalDrift, -10f);
        ambientVerticalDrift = NormalizeRange(ambientVerticalDrift, -10f);
        ApplyImpulseSettings();
        ApplyAmbientSettings();
        ApplyVignetteSettings();
    }

    private void LateUpdate()
    {
        if (ambientMotes == null)
        {
            return;
        }

        Camera gameplayCamera = Camera.main;
        if (gameplayCamera == null)
        {
            return;
        }

        Vector3 cameraPosition = gameplayCamera.transform.position;
        ambientMotes.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);
    }

    public static void PlayEnemyHit(Vector3 position)
    {
        Instance.CreateBurst(
            "Enemy Hit Spark",
            position,
            new Color(0.9f, 0.08f, 0.05f, 0.95f),
            new Color(0.25f, 0.02f, 0.04f, 0f),
            7,
            0.16f,
            0.9f,
            0.05f,
            0.11f,
            ParticleSystemShapeType.Circle,
            0.14f);
    }

    public static void PlayEnemyDeath(Vector3 position)
    {
        GameFeelFeedback feedback = Instance;
        feedback.CreateBurst(
            "Enemy Death Dust",
            position,
            new Color(0.33f, 0.05f, 0.06f, 0.8f),
            new Color(0.04f, 0.02f, 0.03f, 0f),
            14,
            0.42f,
            1.2f,
            0.09f,
            0.2f,
            ParticleSystemShapeType.Circle,
            0.28f);

        feedback.CreateBurst(
            "Enemy Soul Puff",
            position + Vector3.up * 0.12f,
            new Color(0.42f, 0.9f, 1f, 0.42f),
            new Color(0.15f, 0.2f, 0.32f, 0f),
            6,
            0.55f,
            0.55f,
            0.07f,
            0.16f,
            ParticleSystemShapeType.Circle,
            0.18f);

        feedback.ShakeEnemyDeath(position);
    }

    public static void PlayPlayerHit(Vector3 position)
    {
        GameFeelFeedback feedback = Instance;
        feedback.CreateBurst(
            "Player Hit Flash",
            position,
            new Color(1f, 0.12f, 0.08f, 0.55f),
            new Color(0.4f, 0.02f, 0.02f, 0f),
            8,
            0.2f,
            0.7f,
            0.08f,
            0.18f,
            ParticleSystemShapeType.Circle,
            0.2f);
        feedback.Shake(position, feedback.playerDamageImpulse);
    }

    public static void PlayXpCollect(Vector3 position)
    {
        Instance.CreateBurst(
            "XP Collect Sparkle",
            position,
            new Color(0.48f, 0.95f, 1f, 0.95f),
            new Color(0.08f, 0.28f, 1f, 0f),
            10,
            0.22f,
            1.05f,
            0.05f,
            0.13f,
            ParticleSystemShapeType.Circle,
            0.16f);
    }

    public static void PlayXpTrail(Vector3 position)
    {
        GameFeelFeedback feedback = Instance;
        if (Time.time - feedback.lastXpTrailTime < XpTrailMinInterval)
        {
            return;
        }

        feedback.lastXpTrailTime = Time.time;
        feedback.CreateBurst(
            "XP Magnet Trail",
            position,
            new Color(0.2f, 0.75f, 1f, 0.35f),
            new Color(0.07f, 0.18f, 0.65f, 0f),
            2,
            0.18f,
            0.25f,
            0.04f,
            0.09f,
            ParticleSystemShapeType.Circle,
            0.04f);
    }

    public static void PlayShieldBurst(Vector3 position, float radius)
    {
        GameFeelFeedback feedback = Instance;
        feedback.CreateRing(
            "Shield Ward Ring",
            position,
            new Color(0.55f, 0.9f, 1f, 0.7f),
            Mathf.Max(0.25f, radius),
            20,
            0.34f,
            0.08f,
            0.16f);
        feedback.Shake(position, feedback.shieldImpulse);
    }

    public static void PlayShieldShimmer(Vector3 position, float radius)
    {
        Instance.CreateRing(
            "Shield Shimmer",
            position,
            new Color(0.5f, 0.85f, 1f, 0.34f),
            Mathf.Max(0.25f, radius * 0.86f),
            7,
            0.26f,
            0.05f,
            0.1f);
    }

    public static void PlayRepelBurst(Vector3 position, float radius)
    {
        GameFeelFeedback feedback = Instance;
        feedback.CreateRing(
            "Repel Pulse",
            position,
            new Color(0.65f, 1f, 0.52f, 0.72f),
            Mathf.Max(0.4f, radius),
            32,
            0.38f,
            0.07f,
            0.15f);
        feedback.Shake(position, feedback.repelImpulse);
    }

    public static void PlayLungeBurst(Vector3 position)
    {
        Instance.CreateBurst(
            "Lunge Spark",
            position,
            new Color(1f, 0.42f, 0.9f, 0.78f),
            new Color(0.35f, 0.04f, 0.35f, 0f),
            12,
            0.18f,
            1.6f,
            0.06f,
            0.14f,
            ParticleSystemShapeType.Circle,
            0.13f);
    }

    public static void PlayLungePath(Vector3 startPosition, Vector3 endPosition, float width)
    {
        GameFeelFeedback feedback = Instance;
        Vector3 direction = endPosition - startPosition;
        float distance = direction.magnitude;
        if (distance <= 0.01f)
        {
            return;
        }

        Vector3 midpoint = (startPosition + endPosition) * 0.5f;
        ParticleSystem particles = feedback.CreateBurst(
            "Lunge Streak",
            midpoint,
            new Color(1f, 0.35f, 0.92f, 0.5f),
            new Color(0.25f, 0.04f, 0.35f, 0f),
            24,
            0.18f,
            0.18f,
            0.08f,
            0.18f,
            ParticleSystemShapeType.Box,
            0.1f);

        particles.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.scale = new Vector3(distance, Mathf.Max(0.15f, width), 0.1f);
        feedback.Shake(startPosition, feedback.lungeImpulse);
    }

    public static void EnsureAmbientMotes()
    {
        Instance.CreateAmbientMotes();
    }

    private void ConfigureImpulse()
    {
        if (!enableCameraFeedback)
        {
            return;
        }

        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        ApplyImpulseSettings();

        EnsureImpulseListener();
    }

    private void ApplyImpulseSettings()
    {
        if (impulseSource == null)
        {
            return;
        }

        impulseSource.DefaultVelocity = Vector3.down * 0.08f;
        impulseSource.ImpulseDefinition.ImpulseChannel = 1;
        impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulseSource.ImpulseDefinition.ImpulseDuration = impulseDuration;
        impulseSource.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        impulseSource.ImpulseDefinition.DissipationDistance = 100f;
        impulseSource.ImpulseDefinition.DissipationRate = 0.25f;
        impulseSource.ImpulseDefinition.PropagationSpeed = 343f;
    }

    private void EnsureImpulseListener()
    {
        CinemachineCamera cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        if (cinemachineCamera == null)
        {
            return;
        }

        impulseListener = cinemachineCamera.GetComponent<CinemachineImpulseListener>();
        if (impulseListener == null)
        {
            impulseListener = cinemachineCamera.gameObject.AddComponent<CinemachineImpulseListener>();
        }

        impulseListener.ChannelMask = 1;
        impulseListener.Gain = impulseListenerGain;
        impulseListener.Use2DDistance = true;
        impulseListener.UseCameraSpace = true;
    }

    private void Shake(Vector3 position, float strength)
    {
        if (!enableCameraFeedback)
        {
            return;
        }

        if (impulseSource == null)
        {
            ConfigureImpulse();
        }

        if (impulseSource == null || strength <= 0f)
        {
            return;
        }

        Vector2 direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = Vector2.down;
        }

        impulseSource.GenerateImpulseAtPositionWithVelocity(position, new Vector3(direction.x, direction.y, 0f) * strength);
    }

    private void ShakeEnemyDeath(Vector3 position)
    {
        if (!enableEnemyDeathImpulse || Time.time - lastEnemyDeathImpulseTime < enemyDeathImpulseCooldown)
        {
            return;
        }

        lastEnemyDeathImpulseTime = Time.time;
        Shake(position, enemyDeathImpulse);
    }

    private ParticleSystem CreateRing(
        string objectName,
        Vector3 position,
        Color color,
        float radius,
        int particleCount,
        float lifetime,
        float minSize,
        float maxSize)
    {
        ParticleSystem particles = CreateParticleSystem(objectName, position, color, new Color(color.r, color.g, color.b, 0f), lifetime, 0.15f, minSize, maxSize);
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;
        shape.radiusThickness = 0.02f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(particleCount, 1, 96)) });

        particles.Play();
        Destroy(particles.gameObject, lifetime + 0.35f);
        return particles;
    }

    private ParticleSystem CreateBurst(
        string objectName,
        Vector3 position,
        Color startColor,
        Color endColor,
        int particleCount,
        float lifetime,
        float speed,
        float minSize,
        float maxSize,
        ParticleSystemShapeType shapeType,
        float radius)
    {
        ParticleSystem particles = CreateParticleSystem(objectName, position, startColor, endColor, lifetime, speed, minSize, maxSize);

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = shapeType;
        shape.radius = Mathf.Max(0.01f, radius);
        shape.radiusThickness = 0.25f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(particleCount, 1, 96)) });

        particles.Play();
        Destroy(particles.gameObject, lifetime + 0.35f);
        return particles;
    }

    private ParticleSystem CreateParticleSystem(
        string objectName,
        Vector3 position,
        Color startColor,
        Color endColor,
        float lifetime,
        float speed,
        float minSize,
        float maxSize)
    {
        GameObject effectObject = new GameObject(objectName);
        effectObject.transform.position = position;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.05f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.55f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.45f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = startColor;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(endColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(startColor.a, 0f),
                new GradientAlphaKey(endColor.a, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sortingOrder = EffectSortingOrder;

        return particles;
    }

    private void CreateAmbientMotes()
    {
        if (!enableAmbientMotes)
        {
            return;
        }

        Transform existingMotes = transform.Find("Atmosphere Motes");
        if (existingMotes != null)
        {
            ambientMotes = existingMotes;
            ambientParticles = ambientMotes.GetComponent<ParticleSystem>();
            if (ambientParticles == null)
            {
                ambientParticles = ambientMotes.gameObject.AddComponent<ParticleSystem>();
            }

            ambientParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ApplyAmbientSettings();
            ambientParticles.Play();
            return;
        }

        GameObject motesObject = new GameObject("Atmosphere Motes");
        motesObject.transform.SetParent(transform, false);
        ambientMotes = motesObject.transform;
        ambientParticles = motesObject.AddComponent<ParticleSystem>();
        ambientParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ApplyAmbientSettings();
        ambientParticles.Play();
    }

    private void ApplyAmbientSettings()
    {
        if (ambientParticles == null)
        {
            return;
        }

        ParticleSystem.MainModule main = ambientParticles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = ambientMaxParticles;
        main.startLifetime = new ParticleSystem.MinMaxCurve(ambientLifetimeRange.x, ambientLifetimeRange.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(ambientSpeedRange.x, ambientSpeedRange.y);
        main.startSize = new ParticleSystem.MinMaxCurve(ambientSizeRange.x, ambientSizeRange.y);
        main.startColor = new ParticleSystem.MinMaxGradient(ambientMistColor, ambientEmberColor);

        ParticleSystem.EmissionModule emission = ambientParticles.emission;
        emission.rateOverTime = ambientEmissionRate;

        ParticleSystem.ShapeModule shape = ambientParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = ambientAreaSize;

        // Fade each mote in and out over its lifetime so it reads as a soft floating speck
        // rather than a hard dot that pops in and out.
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ambientParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient lifetimeGradient = new Gradient();
        lifetimeGradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.25f),
                new GradientAlphaKey(1f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(lifetimeGradient);

        ParticleSystem.VelocityOverLifetimeModule velocity = ambientParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(ambientHorizontalDrift.x, ambientHorizontalDrift.y);
        velocity.y = new ParticleSystem.MinMaxCurve(ambientVerticalDrift.x, ambientVerticalDrift.y);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystemRenderer renderer = ambientParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (!string.IsNullOrEmpty(ambientSortingLayer) && SortingLayer.NameToID(ambientSortingLayer) != 0)
        {
            renderer.sortingLayerName = ambientSortingLayer;
        }
        renderer.sortingOrder = ambientSortingOrder;
        renderer.sharedMaterial = GetAmbientMaterial();
    }

    private void SetupVignette()
    {
        if (atmosphereVolume == null)
        {
            GameObject volumeObject = new GameObject("Atmosphere Volume");
            volumeObject.transform.SetParent(transform, false);
            atmosphereVolume = volumeObject.AddComponent<Volume>();
            atmosphereVolume.isGlobal = true;
            atmosphereVolume.priority = 10f;
        }

        if (atmosphereProfile == null)
        {
            atmosphereProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            atmosphereProfile.hideFlags = HideFlags.HideAndDontSave;
            atmosphereVolume.sharedProfile = atmosphereProfile;
        }

        if (vignette == null && !atmosphereProfile.TryGet(out vignette))
        {
            vignette = atmosphereProfile.Add<Vignette>(true);
        }

        EnableCameraPostProcessing();
        ApplyVignetteSettings();
    }

    private void EnableCameraPostProcessing()
    {
        Camera gameplayCamera = Camera.main;
        if (gameplayCamera == null)
        {
            return;
        }

        UniversalAdditionalCameraData cameraData = gameplayCamera.GetUniversalAdditionalCameraData();
        if (cameraData != null)
        {
            cameraData.renderPostProcessing = true;
        }
    }

    private void ApplyVignetteSettings()
    {
        if (vignette == null)
        {
            return;
        }

        vignette.active = enableVignette;
        vignette.color.Override(vignetteColor);
        vignette.intensity.Override(vignetteIntensity);
        vignette.smoothness.Override(vignetteSmoothness);
        vignette.rounded.Override(false);
    }

    private Material GetAmbientMaterial()
    {
        if (ambientMaterial != null)
        {
            return ambientMaterial;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        ambientMaterial = new Material(shader)
        {
            name = "Atmosphere Mote Material (Runtime)",
            hideFlags = HideFlags.HideAndDontSave
        };
        ambientMaterial.mainTexture = GetAmbientTexture();
        return ambientMaterial;
    }

    private Texture2D GetAmbientTexture()
    {
        if (ambientTexture != null)
        {
            return ambientTexture;
        }

        const int resolution = 32;
        ambientTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
        {
            name = "Atmosphere Mote Texture (Runtime)",
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        float center = (resolution - 1) * 0.5f;
        Color[] pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                float normalized = Mathf.Clamp01(distance / center);
                float alpha = Mathf.SmoothStep(1f, 0f, normalized);
                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha * alpha);
            }
        }

        ambientTexture.SetPixels(pixels);
        ambientTexture.Apply();
        return ambientTexture;
    }

    private Vector2 NormalizeRange(Vector2 range, float minimum)
    {
        range.x = Mathf.Max(minimum, range.x);
        range.y = Mathf.Max(minimum, range.y);

        if (range.y < range.x)
        {
            range.y = range.x;
        }

        return range;
    }
}
