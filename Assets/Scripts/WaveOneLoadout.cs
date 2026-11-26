using TMPro;
using UnityEngine;

[RequireComponent(typeof(WaveProgression))]
public sealed class WaveOneLoadout : MonoBehaviour
{
    [SerializeField, Min(1)] private int kitHealing = 35;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text kitsText;

    private WaveProgression player;
    private AudioSource pickupAudio;
    private WeaponSpec weapon;

    public WeaponId EquippedWeapon { get; private set; }
    public int ShotDamage => weapon.Damage;
    public int Magazine { get; private set; }
    public int Reserve { get; private set; }
    public int HealthKits { get; private set; }

    private void Awake()
    {
        player = GetComponent<WaveProgression>();
        pickupAudio = GetComponent<AudioSource>();
        EquippedWeapon = WeaponProfile.Equipped;
        weapon = WeaponCatalog.Get(EquippedWeapon);
        Magazine = weapon.MagazineSize;
        Reserve = weapon.StartingReserve;
        RefreshHud();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseHealthKit();
        }
    }

    public bool TrySpendRound()
    {
        if (!CanAct || Magazine == 0)
        {
            return false;
        }

        Magazine--;
        RefreshHud();
        return true;
    }

    public bool Reload()
    {
        if (!CanAct || Magazine == weapon.MagazineSize || Reserve == 0)
        {
            return false;
        }

        int transferred = Mathf.Min(weapon.MagazineSize - Magazine, Reserve);
        Magazine += transferred;
        Reserve -= transferred;
        RefreshHud();
        return true;
    }

    public bool UseHealthKit()
    {
        if (!CanAct || HealthKits == 0 || !player.TryHeal(kitHealing))
        {
            return false;
        }

        HealthKits--;
        RefreshHud();
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player.CurrentHealth <= 0 || !other.TryGetComponent(out SupplyPickup pickup) || !pickup.TryCollect())
        {
            return;
        }

        if (pickup.Kind == SupplyPickup.SupplyKind.Ammo)
        {
            Reserve += pickup.Amount;
        }
        else
        {
            HealthKits += pickup.Amount;
        }

        pickupAudio.Play();
        RefreshHud();
    }

    private bool CanAct => !WavePause.BlocksGameplayInput && player.CurrentHealth > 0;

    private void RefreshHud()
    {
        string hint = Magazine != 0 ? "" : Reserve > 0 ? "   R TO RELOAD" : "   FIND AMMO";
        ammoText.text = $"AMMO  {Magazine} / {Reserve}{hint}";
        kitsText.text = $"HEALTH KITS  {HealthKits}   [E]";
    }
}
