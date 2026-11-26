public enum WeaponId
{
    StarterPistol = 0,
    HeavyPistol = 1
}

public readonly struct WeaponSpec
{
    public readonly string Name;
    public readonly int Price;
    public readonly int Damage;
    public readonly int MagazineSize;
    public readonly int StartingReserve;

    public WeaponSpec(string name, int price, int damage, int magazineSize, int startingReserve)
    {
        Name = name;
        Price = price;
        Damage = damage;
        MagazineSize = magazineSize;
        StartingReserve = startingReserve;
    }
}

public static class WeaponCatalog
{
    public static WeaponSpec Get(WeaponId id)
    {
        return id == WeaponId.HeavyPistol
            ? new WeaponSpec("Heavy Pistol", 18, 54, 4, 16)
            : new WeaponSpec("Starter Pistol", 0, 34, 6, 18);
    }
}
