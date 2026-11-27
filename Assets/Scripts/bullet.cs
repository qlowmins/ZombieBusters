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
    private Mesh starterPistolMesh;
    private Material starterPistolMaterial;
    private MeshRenderer gunRenderer;

    private void Start()
    {
        if (loadout != null)
        {
            gunRenderer = gunMesh.GetComponent<MeshRenderer>();
            starterPistolMesh = gunMesh.sharedMesh;
            starterPistolMaterial = gunRenderer.sharedMaterial;
            loadout.WeaponSwitched += ShowWeapon;
            ShowWeapon(loadout.EquippedWeapon);
        }
    }

    private void OnDestroy()
    {
        if (loadout != null)
        {
            loadout.WeaponSwitched -= ShowWeapon;
        }
    }

    private void ShowWeapon(WeaponId weapon)
    {
        bool heavy = weapon == WeaponId.HeavyPistol;
        gunMesh.sharedMesh = heavy ? heavyPistolMesh : starterPistolMesh;
        gunRenderer.sharedMaterial = heavy ? heavyPistolMaterial : starterPistolMaterial;
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
