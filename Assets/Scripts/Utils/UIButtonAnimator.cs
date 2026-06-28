using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Small, reusable button feedback animation. Scales its target up on
/// hover/selection and down on press, then eases smoothly back to rest.
///
/// All values are Inspector-adjustable so the same component can be reused on
/// menu buttons, pause buttons, level-up buttons, the game-over restart button
/// and any other UI button. It only animates a Transform's localScale; it never
/// creates or styles UI.
///
/// Uses unscaled time so the animation still plays while Time.timeScale == 0
/// (pause menu, level-up panel, game-over screen).
/// </summary>
[DisallowMultipleComponent]
public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler,
    ISubmitHandler
{
    private const float ClickSoundDebounceSeconds = 0.05f;

    [Header("Scale Multipliers (relative to authored scale)")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressedScale = 0.94f;

    [Header("Animation")]
    [Tooltip("How quickly the button approaches its target scale. Higher = snappier.")]
    [SerializeField] private float animationSpeed = 12f;

    [Header("Target")]
    [Tooltip("Transform to scale. Defaults to this object's RectTransform when left empty.")]
    [SerializeField] private RectTransform target;

    [Header("Audio")]
    [Tooltip("Fallback click sound when there is no AudioController in the scene.")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)] private float clickSoundVolume = 1f;

    private Button button;
    private bool isHovered;
    private bool isPressed;
    private bool isSelected;
    private Vector3 baseScale = Vector3.one;
    private static AudioSource sharedClickSource;
    private static float lastClickSoundTime = -1f;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (target == null)
        {
            target = transform as RectTransform;
        }

        if (target != null)
        {
            baseScale = target.localScale;
        }
    }

    private void OnEnable()
    {
        isHovered = false;
        isPressed = false;
        isSelected = false;

        if (target != null)
        {
            target.localScale = baseScale * normalScale;
        }
    }

    private void OnDisable()
    {
        if (target != null)
        {
            target.localScale = baseScale * normalScale;
        }
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        float targetMultiplier = normalScale;
        if (isPressed)
        {
            targetMultiplier = pressedScale;
        }
        else if (isHovered || isSelected)
        {
            targetMultiplier = hoverScale;
        }

        Vector3 desiredScale = baseScale * targetMultiplier;
        float t = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);
        target.localScale = Vector3.Lerp(target.localScale, desiredScale, t);
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        PlayClickSound();
    }
    public void OnPointerUp(PointerEventData eventData) => isPressed = false;
    public void OnSelect(BaseEventData eventData) => isSelected = true;
    public void OnDeselect(BaseEventData eventData) => isSelected = false;
    public void OnSubmit(BaseEventData eventData) => PlayClickSound();

    private void PlayClickSound()
    {
        if (button != null && (!button.IsActive() || !button.IsInteractable()))
        {
            return;
        }

        if (Time.unscaledTime - lastClickSoundTime < ClickSoundDebounceSeconds)
        {
            return;
        }

        if (clickSound != null)
        {
            lastClickSoundTime = Time.unscaledTime;
            PlaySharedClickSound(clickSound, clickSoundVolume);
            return;
        }

        if (AudioController.Instance != null && AudioController.Instance.PlayUIButtonClick())
        {
            lastClickSoundTime = Time.unscaledTime;
        }
    }

    private static void PlaySharedClickSound(AudioClip clip, float volume)
    {
        if (sharedClickSource == null)
        {
            GameObject sourceObject = new GameObject("UI Button Click Audio");
            DontDestroyOnLoad(sourceObject);

            sharedClickSource = sourceObject.AddComponent<AudioSource>();
            sharedClickSource.playOnAwake = false;
            sharedClickSource.loop = false;
            sharedClickSource.spatialBlend = 0f;
            sharedClickSource.ignoreListenerPause = true;
        }

        sharedClickSource.PlayOneShot(clip, volume);
    }
}
