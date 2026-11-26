using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class WaveProgression : MonoBehaviour
{
    [SerializeField] private TMP_Text vesselText;
    [SerializeField, Min(1)] private int requiredSamples;
    [SerializeField] private bool defeatBossToClear;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private int health = 100;
    [SerializeField] private Image healthFill;
    [SerializeField] private string enemyTag;
    [SerializeField] private int contactDamage;
    [SerializeField] private int sustainedDamage;
    [SerializeField] private string clearedScene;
    [SerializeField] private string gameOverScene;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private CoinPickup coinPrefab;
    [SerializeField] private WaveDepositNotice depositNotice;

    private int samplesCollected;
    private int unbankedCoins;
    private bool bossDefeated;
    private bool outcomeResolved;
    private AudioSource pickupAudio;

    public int CurrentHealth => health;
    public int MaxHealth { get; private set; }

    private void Awake()
    {
        pickupAudio = GetComponent<AudioSource>();
        MaxHealth = health;
        UpdateHealthDisplay();
        UpdateSampleText();
        UpdateCoinText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (outcomeResolved || health <= 0)
        {
            return;
        }

        if (other.TryGetComponent(out CoinPickup coin))
        {
            if (coin.TryCollect())
            {
                unbankedCoins += coin.Amount;
                UpdateCoinText();
                pickupAudio.Play();
            }
            return;
        }

        GameObject pickup = other.gameObject;
        if (defeatBossToClear || !pickup.CompareTag("vessel") || !pickup.activeSelf)
        {
            return;
        }

        // Claim the pickup immediately, even if physics has queued another contact.
        pickup.SetActive(false);
        Destroy(pickup);
        pickupAudio.Play();

        samplesCollected++;
        UpdateSampleText();
    }

    public void DropCoins(Vector3 position, int amount)
    {
        if (!outcomeResolved && health > 0 && coinPrefab != null)
        {
            Instantiate(coinPrefab, position, Quaternion.identity).Amount = amount;
        }
    }

    public void AbandonWave()
    {
        outcomeResolved = true;
        unbankedCoins = 0;
        UpdateCoinText();
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"UNBANKED COINS   {unbankedCoins}";
        }
    }

    private void UpdateSampleText()
    {
        if (vesselText != null)
        {
            vesselText.text = $"{samplesCollected} / {requiredSamples}";
        }
    }

    public void SignalBossDefeated()
    {
        bossDefeated = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ApplyContactDamage(collision, contactDamage);
    }

    private void OnCollisionStay(Collision collision)
    {
        ApplyContactDamage(collision, sustainedDamage);
    }

    private void ApplyContactDamage(Collision collision, int damage)
    {
        if (outcomeResolved || damage == 0 || !collision.gameObject.CompareTag(enemyTag))
        {
            return;
        }

        TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        if (outcomeResolved || health <= 0 || damage <= 0)
        {
            return;
        }

        health = Mathf.Max(0, health - damage);
        UpdateHealthDisplay();
    }

    public bool TryHeal(int amount)
    {
        if (outcomeResolved || health <= 0 || health == MaxHealth || amount <= 0)
        {
            return false;
        }

        health += Mathf.Min(MaxHealth - health, amount);
        UpdateHealthDisplay();
        return true;
    }

    private void UpdateHealthDisplay()
    {
        healthText.text = health.ToString();
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)health / MaxHealth;
        }
    }

    private void LateUpdate()
    {
        // Both pickup and damage callbacks for this frame have already run.
        bool playerDied = health <= 0;
        bool cleared = defeatBossToClear ? bossDefeated : samplesCollected >= requiredSamples;
        if (outcomeResolved || (!playerDied && !cleared))
        {
            return;
        }

        outcomeResolved = true;
        if (!playerDied)
        {
            int? deposited = WeaponProfile.Deposit(unbankedCoins);
            if (depositNotice != null)
            {
                Instantiate(depositNotice).Show(deposited);
            }
        }
        unbankedCoins = 0;
        UpdateCoinText();
        SceneManager.LoadScene(playerDied ? gameOverScene : clearedScene);
    }
}
