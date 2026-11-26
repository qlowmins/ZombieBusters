using UnityEngine;

public class bullet : MonoBehaviour
{
    public GameObject myBullet;
    public Transform bulletPosition;

    [SerializeField] private WaveOneLoadout loadout;
    [SerializeField] private Camera aimCamera;
    [SerializeField] private LineRenderer tracer;
    [SerializeField] private MeshFilter gunMesh;
    [SerializeField] private Mesh heavyPistolMesh;
    [SerializeField] private Material heavyPistolMaterial;

    private float hideTracerAt;

    private void Start()
    {
        if (loadout != null && loadout.EquippedWeapon == WeaponId.HeavyPistol)
        {
            gunMesh.sharedMesh = heavyPistolMesh;
            gunMesh.GetComponent<MeshRenderer>().sharedMaterial = heavyPistolMaterial;
        }
    }

    private void Update()
    {
        if (tracer != null && tracer.enabled && Time.time >= hideTracerAt)
        {
            tracer.enabled = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryFire();
        }
    }

    public bool TryFire()
    {
        if (WavePause.BlocksGameplayInput)
        {
            return false;
        }

        if (loadout == null)
        {
            Instantiate(myBullet, bulletPosition.position, bulletPosition.rotation);
            return true;
        }

        if (!loadout.TrySpendRound())
        {
            return false;
        }

        var aim = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        const float range = 80f;
        Vector3 end = aim.GetPoint(range);
        if (Physics.Raycast(aim, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            hit.collider.GetComponentInParent<WaveOneZombie>()?.TakeDamage(loadout.ShotDamage);
            hit.collider.GetComponentInParent<Widowmayer>()?.TakeDamage(loadout.ShotDamage);
        }

        tracer.SetPosition(0, aimCamera.ViewportToWorldPoint(new Vector3(0.36f, 0.38f, 0.65f)));
        tracer.SetPosition(1, end);
        tracer.enabled = true;
        hideTracerAt = Time.time + 0.06f;
        return true;
    }
}
