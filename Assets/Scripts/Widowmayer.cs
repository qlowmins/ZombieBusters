using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(20)]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class Widowmayer : MonoBehaviour
{
    private enum Phase { Orienting, WindingUp, Pouncing, Recovering }
    private enum Attack { Pounce, Web }

    [SerializeField] private WaveProgression player;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackCue;
    [SerializeField] private TMP_Text slowedText;
    [SerializeField] private LineRenderer pounceRing;
    [SerializeField] private LineRenderer webLine;
    [SerializeField] private Transform webOrb;
    [SerializeField] private Animation visualAnimation;
    [SerializeField] private Transform webMuzzle;
    [SerializeField, Min(1)] private int maxHealth = 510;
    [SerializeField] private Vector2 arenaMin = new Vector2(-16f, -21f);
    [SerializeField] private Vector2 arenaMax = new Vector2(16f, 23f);

    private const float OrientingSeconds = 3.5f;
    private const float PounceWindup = 1.3f;
    private const float PounceDuration = 0.55f;
    private const float PounceRecovery = 2.1f;
    private const float PounceRadius = 3.25f;
    private const int PounceDamage = 34;
    private const float WebWindup = 0.9f;
    private const float WebRecovery = 2.2f;
    private const float WebSpeed = 18f;
    private const float WebRadius = 0.36f;
    private const float WebRange = 60f;
    private const int WebDamage = 12;
    private const float SlowDuration = 2.5f;
    private const float SlowMultiplier = 0.5f;
    private const float WebCastRelease = 0.63f;

    private readonly RaycastHit[] webHits = new RaycastHit[16];
    private FirstPersonMovement movement;
    private CapsuleCollider body;
    private Collider playerBody;
    private Phase phase;
    private Attack attack;
    private float nextPhaseAt;
    private float slowUntil;
    private Vector3 jumpStart;
    private Vector3 landingPoint;
    private Vector3 webTarget;
    private Vector3 webDirection;
    private float webRemaining;

    public int Health { get; private set; }

    private Vector3 WebLaunchPoint => webMuzzle != null ? webMuzzle.position : transform.position + Vector3.up * 1.45f;

    private void Awake()
    {
        body = GetComponent<CapsuleCollider>();
        playerBody = player.GetComponent<Collider>();
        movement = player.GetComponent<FirstPersonMovement>();
        Health = maxHealth;
        RefreshHealth();
        pounceRing.enabled = false;
        webLine.enabled = false;
        webOrb.gameObject.SetActive(false);
        slowedText.gameObject.SetActive(false);
        attackCue.text = string.Empty;
        nextPhaseAt = Time.time + OrientingSeconds;
        if (visualAnimation != null)
        {
            visualAnimation.Play("Idle");
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || player.CurrentHealth <= 0)
        {
            return;
        }

        if (slowUntil > 0f && Time.time >= slowUntil)
        {
            ClearSlow();
        }

        if (phase == Phase.WindingUp && attack == Attack.Web)
        {
            webLine.SetPosition(0, WebLaunchPoint);
        }

        // An attack already at impact can still resolve on the lethal shot's frame.
        MoveWeb();
        if (phase == Phase.Pouncing)
        {
            UpdatePounce();
        }
        else if (Health > 0 && Time.time >= nextPhaseAt)
        {
            if (phase == Phase.WindingUp)
            {
                ReleaseAttack();
            }
            else
            {
                BeginWindup();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (Health <= 0 || damage <= 0)
        {
            return;
        }

        Health = Mathf.Max(0, Health - damage);
        RefreshHealth();
        if (Health == 0)
        {
            player.SignalBossDefeated();
        }
    }

    private void BeginWindup()
    {
        Vector3 target = playerBody.bounds.center;
        Vector3 horizontal = target - transform.position;
        horizontal.y = 0f;
        if (horizontal.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(horizontal);
        }

        phase = Phase.WindingUp;
        if (attack == Attack.Pounce)
        {
            landingPoint = new Vector3(
                Mathf.Clamp(target.x, arenaMin.x, arenaMax.x),
                transform.position.y,
                Mathf.Clamp(target.z, arenaMin.y, arenaMax.y));
            DrawPounceRing();
            ShowCue("POUNCE   /   LEAVE THE RING", new Color(1f, 0.52f, 0.2f));
            nextPhaseAt = Time.time + PounceWindup;
            PlayAttackAnimation("Pounce Windup", PounceWindup);
            return;
        }

        webTarget = target;
        webLine.SetPosition(0, WebLaunchPoint);
        webLine.SetPosition(1, webTarget);
        webLine.enabled = true;
        ShowCue("WEB   /   MOVE SIDEWAYS", new Color(0.43f, 1f, 0.92f));
        nextPhaseAt = Time.time + WebWindup;
        PlayAttackAnimation("Web Cast", WebWindup / WebCastRelease);
    }

    private void ReleaseAttack()
    {
        if (attack == Attack.Pounce)
        {
            jumpStart = transform.position;
            body.enabled = false;
            phase = Phase.Pouncing;
            nextPhaseAt = Time.time + PounceDuration;
            PlayAttackAnimation("Pounce", PounceDuration);
            return;
        }

        webLine.enabled = false;
        webOrb.position = WebLaunchPoint;
        webDirection = (webTarget - webOrb.position).normalized;
        webOrb.gameObject.SetActive(true);
        webRemaining = WebRange;
        Recover(WebRecovery);
    }

    private void UpdatePounce()
    {
        float progress = Mathf.Clamp01(1f - (nextPhaseAt - Time.time) / PounceDuration);
        transform.position = Vector3.Lerp(jumpStart, landingPoint, progress)
            + Vector3.up * (Mathf.Sin(progress * Mathf.PI) * 3.6f);
        if (progress < 1f)
        {
            return;
        }

        Vector3 offset = playerBody.bounds.center - landingPoint;
        float height = offset.y;
        offset.y = 0f;
        if (offset.sqrMagnitude <= PounceRadius * PounceRadius && height <= 2f)
        {
            player.TakeDamage(PounceDamage);
        }

        pounceRing.enabled = false;
        body.enabled = true;
        Recover(PounceRecovery);
    }

    private void MoveWeb()
    {
        if (webRemaining <= 0f)
        {
            return;
        }

        float distance = Mathf.Min(WebSpeed * Time.deltaTime, webRemaining);
        int count = Physics.SphereCastNonAlloc(webOrb.position, WebRadius, webDirection, webHits,
            distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        Collider hit = null;
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            if (webHits[i].collider != body && webHits[i].distance < nearest)
            {
                nearest = webHits[i].distance;
                hit = webHits[i].collider;
            }
        }

        if (hit == playerBody)
        {
            player.TakeDamage(WebDamage);
            slowUntil = Time.time + SlowDuration;
            movement.MovementMultiplier = SlowMultiplier;
            slowedText.gameObject.SetActive(true);
        }

        webOrb.position += webDirection * distance;
        webRemaining = hit == null ? webRemaining - distance : 0f;
        if (webRemaining <= 0f)
        {
            webOrb.gameObject.SetActive(false);
        }
    }

    private void DrawPounceRing()
    {
        for (int i = 0; i < pounceRing.positionCount; i++)
        {
            float angle = i * (Mathf.PI * 2f / pounceRing.positionCount);
            pounceRing.SetPosition(i, landingPoint + new Vector3(
                Mathf.Cos(angle) * PounceRadius, 0.1f, Mathf.Sin(angle) * PounceRadius));
        }
        pounceRing.enabled = true;
    }

    private void Recover(float seconds)
    {
        phase = Phase.Recovering;
        attack = attack == Attack.Pounce ? Attack.Web : Attack.Pounce;
        nextPhaseAt = Time.time + seconds;
        ShowCue("WIDOWMAYER RECOVERING", new Color(0.85f, 0.89f, 0.81f));
    }

    private void PlayAttackAnimation(string clip, float duration)
    {
        if (visualAnimation == null)
        {
            return;
        }

        AnimationState state = visualAnimation[clip];
        state.speed = state.length / duration;
        visualAnimation.CrossFade(clip, 0.08f, PlayMode.StopAll);
        visualAnimation.CrossFadeQueued("Idle", 0.12f);
    }

    private void RefreshHealth()
    {
        healthFill.fillAmount = (float)Health / maxHealth;
        healthText.text = $"WIDOWMAYER   {Health} / {maxHealth}";
    }

    private void ShowCue(string label, Color color)
    {
        attackCue.text = label;
        attackCue.color = color;
    }

    private void ClearSlow()
    {
        slowUntil = 0f;
        if (movement != null)
        {
            movement.MovementMultiplier = 1f;
        }
        if (slowedText != null)
        {
            slowedText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (slowUntil > 0f)
        {
            ClearSlow();
        }
    }
}
