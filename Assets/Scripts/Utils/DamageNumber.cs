using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    private float floatSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        floatSpeed = Random.Range(0.1f, 1.5f);
        Destroy(gameObject, 1);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * floatSpeed;
    }

    public void SetValue(float value)
    {
        damageText.text = FormatValue(value);
    }

    private string FormatValue(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.##");
    }
}
