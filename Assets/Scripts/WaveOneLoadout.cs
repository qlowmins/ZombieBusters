using System;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(WaveProgression))]
public sealed class WaveOneLoadout : MonoBehaviour
{
    [SerializeField, Min(1)] private int kitHealing = 35;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text kitsText;

    private WaveProgression player;
    private AudioSource pickupAudio;
    private sealed class WeaponAmmo
    {
        public int Magazine;
        public int Reserve;

        public WeaponAmmo(WeaponId weapon)
        {
            WeaponSpec spec = WeaponCatalog.Get(weapon);
            Magazine = spec.MagazineSize;
            Reserve = spec.StartingReserve;
        }
    }

    private WeaponAmmo starterAmmo;
    private WeaponAmmo heavyAmmo;
    private WeaponAmmo ActiveAmmo => EquippedWeapon == WeaponId.StarterPistol ? starterAmmo : heavyAmmo;

    public event Action<WeaponId> WeaponSwitched;
    public WeaponId EquippedWeapon { get; private set; }
    public int ShotDamage => WeaponProfile.Damage(EquippedWeapon);
    public int Magazine => ActiveAmmo.Magazine;
    public int Reserve => ActiveAmmo.Reserve;
    public int HealthKits { get; private set; }

    private void Awake()
    {
        player = GetComponent<WaveProgression>();
        pickupAudio = GetComponent<AudioSource>();
        EquippedWeapon = WeaponProfile.Equipped;
        starterAmmo = new WeaponAmmo(WeaponId.StarterPistol);
        if (WeaponProfile.Owns(WeaponId.HeavyPistol))
        {
            heavyAmmo = new WeaponAmmo(WeaponId.HeavyPistol);
        }
        RefreshHud();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TrySwitchWeapon(WeaponId.StarterPistol);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TrySwitchWeapon(WeaponId.HeavyPistol);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseHealthKit();
        }
    }

    public bool TrySwitchWeapon(WeaponId weapon)
    {
        if (!CanAct || weapon == EquippedWeapon || !WeaponProfile.Owns(weapon))
        {
            return false;
        }

        EquippedWeapon = weapon;
        RefreshHud();
        WeaponSwitched?.Invoke(weapon);
        return true;
    }

    public bool TrySpendRound()
    {
        if (!CanAct || Magazine == 0)
        {
            return false;
        }

        ActiveAmmo.Magazine--;
        RefreshHud();
        return true;
    }

    public bool Reload()
    {
        int capacity = WeaponCatalog.Get(EquippedWeapon).MagazineSize;
        if (!CanAct || Magazine == capacity || Reserve == 0)
        {
            return false;
        }

        int transferred = Mathf.Min(capacity - Magazine, Reserve);
        ActiveAmmo.Magazine += transferred;
        ActiveAmmo.Reserve -= transferred;
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
            ActiveAmmo.Reserve += pickup.Amount;
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
        ammoText.text = $"{WeaponCatalog.Get(EquippedWeapon).Name.ToUpperInvariant()}\nAMMO  {Magazine} / {Reserve}{hint}";
        kitsText.text = $"HEALTH KITS  {HealthKits}   [E]";
    }
}
