using UnityEngine;

public class DamageNumberController : MonoBehaviour
{
    private const int DamageNumberSortingOrder = -10;

    public static DamageNumberController Instance;
    public DamageNumber prefab;

    private Canvas damageCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ConfigureCanvasSorting();
    }

    public void CreateNumber(float value, Vector3 location)
    {
        if (prefab == null || Time.timeScale <= 0f)
        {
            return;
        }

        DamageNumber damageNumber = Instantiate(
            prefab,
            location,
            transform.rotation,
            transform
        );

        damageNumber.SetValue(value);
    }

    public void ClearNumbers()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void ConfigureCanvasSorting()
    {
        damageCanvas = GetComponent<Canvas>();

        if (damageCanvas == null)
        {
            return;
        }

        damageCanvas.overrideSorting = true;
        damageCanvas.sortingOrder = DamageNumberSortingOrder;
    }
}
