using System;
using System.IO;
using UnityEngine;

public static class WeaponProfile
{
    [Serializable]
    private sealed class SaveData
    {
        public int bankedCoins;
        public bool heavyPistolOwned;
        public WeaponId equipped;
    }

    private const string SaveFile = "weapon-profile.json";
    private static string profilePath;
    private static SaveData profile;

    public static int BankedCoins => Profile.bankedCoins;
    public static WeaponId Equipped => Profile.equipped;

    public static bool Owns(WeaponId weapon)
    {
        return weapon == WeaponId.StarterPistol || (weapon == WeaponId.HeavyPistol && Profile.heavyPistolOwned);
    }

    public static bool TryBuyHeavyPistol()
    {
        int price = WeaponCatalog.Get(WeaponId.HeavyPistol).Price;
        if (Profile.heavyPistolOwned || Profile.bankedCoins < price)
        {
            return false;
        }

        return TrySave(new SaveData
        {
            bankedCoins = BankedCoins - price,
            heavyPistolOwned = true,
            equipped = Equipped
        });
    }

    public static bool TryEquip(WeaponId weapon)
    {
        if (!Owns(weapon))
        {
            return false;
        }

        if (Equipped == weapon)
        {
            return true;
        }

        return TrySave(new SaveData
        {
            bankedCoins = BankedCoins,
            heavyPistolOwned = Profile.heavyPistolOwned,
            equipped = weapon
        });
    }

    public static int? Deposit(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int deposit = Mathf.Min(amount, int.MaxValue - BankedCoins);
        var updated = new SaveData
        {
            bankedCoins = BankedCoins + deposit,
            heavyPistolOwned = Profile.heavyPistolOwned,
            equipped = Equipped
        };
        return TrySave(updated) ? deposit : (int?)null;
    }

    private static SaveData Profile
    {
        get
        {
            if (profile == null)
            {
                profile = Load();
            }

            return profile;
        }
    }

    private static SaveData Load()
    {
        try
        {
            if (File.Exists(profilePath))
            {
                var saved = JsonUtility.FromJson<SaveData>(File.ReadAllText(profilePath));
                if (saved != null)
                {
                    saved.bankedCoins = Mathf.Max(0, saved.bankedCoins);
                    if (saved.equipped != WeaponId.HeavyPistol || !saved.heavyPistolOwned)
                    {
                        saved.equipped = WeaponId.StarterPistol;
                    }
                    return saved;
                }
            }
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
        {
            Debug.LogWarning($"Could not read the weapon profile: {exception.Message}");
        }

        return new SaveData();
    }

    private static bool TrySave(SaveData updated)
    {
        string pendingPath = $"{profilePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(profilePath));
            File.WriteAllText(pendingPath, JsonUtility.ToJson(updated));
            if (File.Exists(profilePath))
            {
                File.Replace(pendingPath, profilePath, null);
            }
            else
            {
                File.Move(pendingPath, profilePath);
            }
            profile = updated;
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            Debug.LogWarning($"Could not save the weapon profile: {exception.Message}");
            try
            {
                File.Delete(pendingPath);
            }
            catch (Exception cleanupError) when (cleanupError is IOException || cleanupError is UnauthorizedAccessException)
            {
                Debug.LogWarning($"Could not remove the temporary weapon profile: {cleanupError.Message}");
            }
            return false;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        profile = null;
        string directory = Application.persistentDataPath;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // A separate directory keeps development playtests away from the regular save.
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-zb-profile") + 1;
        if (index > 0 && index < args.Length && Path.IsPathRooted(args[index]))
        {
            directory = args[index];
        }
#endif
        profilePath = Path.Combine(directory, SaveFile);
    }
}
