using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class WeaponStore : MonoBehaviour
{
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text balance;
    [SerializeField] private TMP_Text feedback;
    [SerializeField] private TMP_Text starterStats;
    [SerializeField] private TMP_Text heavyStats;
    [SerializeField] private Button buyHeavy;
    [SerializeField] private TMP_Text buyLabel;
    [SerializeField] private Button equipStarter;
    [SerializeField] private TMP_Text starterEquipLabel;
    [SerializeField] private Button equipHeavy;
    [SerializeField] private TMP_Text heavyEquipLabel;

    private void Awake()
    {
        panel.SetActive(false);
        openButton.onClick.AddListener(Open);
        backButton.onClick.AddListener(Close);
        buyHeavy.onClick.AddListener(BuyHeavyPistol);
        equipStarter.onClick.AddListener(() => Equip(WeaponId.StarterPistol));
        equipHeavy.onClick.AddListener(() => Equip(WeaponId.HeavyPistol));
        starterStats.text = Describe(WeaponId.StarterPistol);
        heavyStats.text = Describe(WeaponId.HeavyPistol);
    }

    private void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    private void Open()
    {
        feedback.text = string.Empty;
        Refresh();
        panel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
    }

    private void Close()
    {
        panel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(openButton.gameObject);
    }

    private void BuyHeavyPistol()
    {
        if (WeaponProfile.TryBuyHeavyPistol())
        {
            feedback.text = "Heavy Pistol purchased.";
        }
        else if (!WeaponProfile.Owns(WeaponId.HeavyPistol))
        {
            int shortfall = WeaponCatalog.Get(WeaponId.HeavyPistol).Price - WeaponProfile.BankedCoins;
            feedback.text = shortfall > 0
                ? $"Insufficient funds. You need {shortfall} more {(shortfall == 1 ? "coin" : "coins")}."
                : "Could not save purchase. Try again.";
        }

        Refresh();
    }

    private void Equip(WeaponId weapon)
    {
        if (WeaponProfile.TryEquip(weapon))
        {
            feedback.text = $"{WeaponCatalog.Get(weapon).Name} equipped.";
            Refresh();
        }
        else if (WeaponProfile.Owns(weapon))
        {
            feedback.text = "Could not save equipped weapon. Try again.";
        }
    }

    private void Refresh()
    {
        balance.text = $"BANKED COINS   {WeaponProfile.BankedCoins}";
        bool heavyOwned = WeaponProfile.Owns(WeaponId.HeavyPistol);
        buyHeavy.interactable = !heavyOwned;
        buyLabel.text = heavyOwned ? "Owned" : "Buy";
        SetEquipState(WeaponId.StarterPistol, equipStarter, starterEquipLabel);
        SetEquipState(WeaponId.HeavyPistol, equipHeavy, heavyEquipLabel);
    }

    private static void SetEquipState(WeaponId weapon, Button button, TMP_Text label)
    {
        bool owned = WeaponProfile.Owns(weapon);
        bool equipped = WeaponProfile.Equipped == weapon;
        button.interactable = owned && !equipped;
        label.text = equipped ? "Equipped" : owned ? "Equip" : "Locked";
    }

    private static string Describe(WeaponId weapon)
    {
        WeaponSpec spec = WeaponCatalog.Get(weapon);
        string price = spec.Price == 0 ? "Free" : $"{spec.Price} coins";
        return $"Price<pos=62%>{price}\nDamage<pos=62%>{spec.Damage}\nMagazine<pos=62%>{spec.MagazineSize}\nStarting reserve<pos=62%>{spec.StartingReserve}";
    }
}
