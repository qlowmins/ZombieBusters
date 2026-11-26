using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public sealed class CoinPickup : MonoBehaviour
{
    [SerializeField] private Transform visual;

    public int Amount { get; set; }

    private void Update()
    {
        visual.Rotate(0f, 110f * Time.deltaTime, 0f, Space.World);
    }

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
