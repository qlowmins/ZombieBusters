using TMPro;
using UnityEngine;

public sealed class WaveDepositNotice : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private static WaveDepositNotice visible;
    private float dismissAt;

    public void Show(int? amount)
    {
        if (visible != null)
        {
            Destroy(visible.gameObject);
        }
        visible = this;
        label.text = amount.HasValue
            ? $"+{amount} {(amount == 1 ? "coin" : "coins")} deposited"
            : "Coin deposit could not be saved";
        dismissAt = Time.unscaledTime + 4f;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Time.unscaledTime >= dismissAt)
        {
            Destroy(gameObject);
        }
    }
}
