using Unity.Cinemachine;
using UnityEngine;

public class GameFeelFeedback : MonoBehaviour
{
    private const int EffectSortingOrder = 24;
    private const int AmbientSortingOrder = -8;
    private const float XpTrailMinInterval = 0.08f;

    private static GameFeelFeedback instance;

    private CinemachineImpulseSource impulseSource;
    private Transform ambientMotes;
    private float lastXpTrailTime;

    public static GameFeelFeedback Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject feedbackObject = new GameObject("Game Feel Feedback");
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
        feedback.Shake(position, 0.1f);
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
        feedback.Shake(position, 0.055f);
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
        feedback.Shake(position, 0.14f);
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
        feedback.Shake(startPosition, 0.11f);
    }

    public static void EnsureAmbientMotes()
    {
        Instance.CreateAmbientMotes();
    }

    private void ConfigureImpulse()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        impulseSource.DefaultVelocity = Vector3.down * 0.08f;
        impulseSource.ImpulseDefinition.ImpulseChannel = 1;
        impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulseSource.ImpulseDefinition.ImpulseDuration = 0.12f;
        impulseSource.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        impulseSource.ImpulseDefinition.DissipationDistance = 100f;
        impulseSource.ImpulseDefinition.DissipationRate = 0.25f;
        impulseSource.ImpulseDefinition.PropagationSpeed = 343f;

        CinemachineCamera cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        if (cinemachineCamera == null)
        {
            return;
        }

        CinemachineImpulseListener listener = cinemachineCamera.GetComponent<CinemachineImpulseListener>();
        if (listener == null)
        {
            listener = cinemachineCamera.gameObject.AddComponent<CinemachineImpulseListener>();
        }

        listener.ChannelMask = 1;
        listener.Gain = 0.65f;
        listener.Use2DDistance = true;
        listener.UseCameraSpace = true;
    }

    private void Shake(Vector3 position, float strength)
    {
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
        if (transform.Find("Atmosphere Motes") != null)
        {
            return;
        }

        GameObject motesObject = new GameObject("Atmosphere Motes");
        motesObject.transform.SetParent(transform, false);
        ambientMotes = motesObject.transform;
        ParticleSystem particles = motesObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 42;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.025f, 0.09f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.08f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.14f, 0.2f, 0.34f, 0.22f),
            new Color(0.55f, 0.22f, 0.12f, 0.18f));

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 2.2f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(22f, 15f, 0.1f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.04f, 0.04f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.015f, 0.08f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = AmbientSortingOrder;

        particles.Play();
    }
}
