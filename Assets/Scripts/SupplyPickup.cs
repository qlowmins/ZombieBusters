using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class SupplyPickup : MonoBehaviour
{
    public enum SupplyKind
    {
        Ammo,
        HealthKit
    }

    [SerializeField] private SupplyKind kind;
    [SerializeField, Min(1)] private int amount = 1;

    public SupplyKind Kind => kind;
    public int Amount => amount;

    public bool TryCollect()
    {
        if (!gameObject.activeSelf)
        {
            return false;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }
}
