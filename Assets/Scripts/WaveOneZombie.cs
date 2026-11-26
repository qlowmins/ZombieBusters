using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public sealed class WaveOneZombie : MonoBehaviour
{
    [SerializeField] private WaveProgression target;
    [SerializeField, Min(1)] private int startingHealth = 100;
    [SerializeField, Min(1)] private int attackDamage = 12;
    [SerializeField, Min(0.1f)] private float detectionRange = 22f;
    [SerializeField, Min(0.1f)] private float attackRange = 1.85f;
    [SerializeField, Min(0.1f)] private float windUpSeconds = 0.55f;
    [SerializeField, Min(0.1f)] private float attackRecovery = 1.25f;
    [SerializeField] private bool holdPositionDuringRecovery;

    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private NavMeshAgent agent;
    private Collider body;
    private Collider targetBody;
    private Renderer[] visibleParts;
    private MaterialPropertyBlock tint;
    private float nextPathUpdate;
    private float nextAttack;
    private float strikeTime;
    private bool alert;
    private bool windingUp;

    public int Health { get; private set; }

    private void Awake()
    {
        Health = startingHealth;
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<CapsuleCollider>();
        targetBody = target.GetComponent<Collider>();
        visibleParts = GetComponentsInChildren<SkinnedMeshRenderer>();
        tint = new MaterialPropertyBlock();

        if (TryGetComponent(out Animation movement) && movement.clip != null)
        {
            movement.wrapMode = WrapMode.Loop;
            movement.Play();
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || Health <= 0 || !agent.isOnNavMesh)
        {
            return;
        }

        if (target.CurrentHealth <= 0)
        {
            agent.isStopped = true;
            SetWindUp(false);
            return;
        }

        Vector3 toPlayer = targetBody.bounds.center - body.bounds.center;
        if (windingUp)
        {
            FacePlayer(toPlayer);
            if (Time.time >= strikeTime)
            {
                SetWindUp(false);
                nextAttack = Time.time + attackRecovery;
                if (CanStrike(toPlayer))
                {
                    target.TakeDamage(attackDamage);
                }
            }
            return;
        }

        if (holdPositionDuringRecovery && Time.time < nextAttack)
        {
            return;
        }

        float hearingRange = alert ? detectionRange * 1.5f : detectionRange;
        alert = toPlayer.sqrMagnitude <= hearingRange * hearingRange;
        if (!alert)
        {
            agent.isStopped = true;
            return;
        }

        if (CanStrike(toPlayer))
        {
            agent.isStopped = true;
            FacePlayer(toPlayer);
            if (Time.time >= nextAttack)
            {
                strikeTime = Time.time + windUpSeconds;
                SetWindUp(true);
            }
            return;
        }

        agent.isStopped = false;
        if (Time.time >= nextPathUpdate)
        {
            nextPathUpdate = Time.time + 0.25f;
            if (NavMesh.SamplePosition(target.transform.position, out NavMeshHit nearby, 2.5f, agent.areaMask))
            {
                agent.SetDestination(nearby.position);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || Health <= 0)
        {
            return;
        }

        Health = Mathf.Max(0, Health - damage);
        if (Health == 0)
        {
            Bounds bounds = body.bounds;
            Vector3 dropPosition = new Vector3(bounds.center.x, bounds.min.y + 0.45f, bounds.center.z);
            target.DropCoins(dropPosition, CompareTag("enemyBoss") ? 8 : 3);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private bool CanStrike(Vector3 toPlayer)
    {
        return toPlayer.sqrMagnitude <= attackRange * attackRange &&
            Physics.Linecast(body.bounds.center, targetBody.bounds.center, out RaycastHit hit,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider == targetBody;
    }

    private void FacePlayer(Vector3 toPlayer)
    {
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion facing = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, 360f * Time.deltaTime);
        }
    }

    private void SetWindUp(bool enabled)
    {
        if (windingUp == enabled)
        {
            return;
        }

        windingUp = enabled;
        tint.SetColor(ColorProperty, new Color(1f, 0.42f, 0.22f));
        foreach (Renderer part in visibleParts)
        {
            part.SetPropertyBlock(enabled ? tint : null);
        }
    }
}
