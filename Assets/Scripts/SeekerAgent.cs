using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SeekerAgent : Agent
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private VisionCone2D fov;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float catchDistance = 0.75f;
    [SerializeField] private float spriteRotationOffset = 0f;

    [Header("Episode / Spawn")]
    [SerializeField] private float minSpawnDistanceFromPlayer = 2f;
    [SerializeField] private float spawnRangeX = 3f;
    [SerializeField] private float spawnRangeY = 3f;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float spawnCheckRadius = 0.2f;

    [Header("Anti-Stuck")]
    [SerializeField] private float checkpointInterval = 2f;
    [SerializeField] private float minDistanceMovedPerCheck = 0.2f;
    [SerializeField] private float stuckTimeLimit = 6f;

    [Header("Anti-Hover")]
    [SerializeField] private float hoverDistance = 1.25f;
    [SerializeField] private float hoverGraceTime = 1.5f;
    [SerializeField] private float hoverPenaltyInterval = 0.5f;
    [SerializeField] private float hoverPenalty = -0.01f;


    [Header("Rewards")]
    [SerializeField] private float stepPenalty = -0.0002f;
    [SerializeField] private float idlePenalty = -0.0005f;
    [SerializeField] private float seenReward = 0.02f;
    [SerializeField] private float catchReward = 1.0f;
    [SerializeField] private float facingRewardScale = 0.002f;
    [SerializeField] private float pursueLastSeenReward = 0.003f;
    [SerializeField] private float stuckPenalty = -0.005f;

    [Header("Search Timeout")] // added this due to 13_4 issue with opponent getting stuck against outer wall for full episode.
    [SerializeField] private float maxTimeWithoutSeeingPlayer = 8f;
    [SerializeField] private float noSightPenalty = -0.05f;

    private Rigidbody2D rb;

    private Vector2 lastSeenPlayerPos;
    private float timeSinceSeen;

    private Vector2 checkpointPosition;
    private float checkpointTimer;
    private float stuckTimer;

    private float previousDistanceToLastSeen;
    private bool hadSeenPlayerThisEpisode;

    private float hoverTimer;
    private float hoverPenaltyTimer;

    private bool sawPlayerLastStep;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();

        if (fov == null)
            fov = GetComponentInChildren<VisionCone2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = false;
    }

    public override void OnEpisodeBegin()
    {
        if (rb == null || player == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = GetValidSpawnPosition();
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        lastSeenPlayerPos = transform.position;
        timeSinceSeen = 999f;
        hadSeenPlayerThisEpisode = false;
        previousDistanceToLastSeen = float.MaxValue;

        checkpointPosition = rb.position;
        checkpointTimer = 0f;
        stuckTimer = 0f;

        hoverTimer = 0f;
        hoverPenaltyTimer = 0f;
        sawPlayerLastStep = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (player == null || fov == null)
            return;

        bool canSeePlayer = fov.IsTarget;

        if (canSeePlayer)
        {
            lastSeenPlayerPos = player.position;
            timeSinceSeen = 0f;
            hadSeenPlayerThisEpisode = true;
        }

        Vector2 toLastSeen = lastSeenPlayerPos - rb.position;

        sensor.AddObservation(rb.linearVelocity.x / Mathf.Max(moveSpeed, 0.001f));
        sensor.AddObservation(rb.linearVelocity.y / Mathf.Max(moveSpeed, 0.001f));

        sensor.AddObservation(transform.right.x);
        sensor.AddObservation(transform.right.y);

        sensor.AddObservation(canSeePlayer ? 1f : 0f);
        sensor.AddObservation(hadSeenPlayerThisEpisode ? 1f : 0f);

        sensor.AddObservation(Mathf.Clamp(toLastSeen.x / 10f, -1f, 1f));
        sensor.AddObservation(Mathf.Clamp(toLastSeen.y / 10f, -1f, 1f));

        sensor.AddObservation(Mathf.Clamp01(timeSinceSeen / 5f));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (player == null || rb == null)
            return;

        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveY = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector2 move = new Vector2(moveX, moveY);
        rb.linearVelocity = move * moveSpeed;

        if (move.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle + spriteRotationOffset);
        }

        AddReward(stepPenalty);

        bool seesPlayer = fov != null && fov.IsTarget;

        if (seesPlayer)
        {
            timeSinceSeen = 0f;
            hadSeenPlayerThisEpisode = true;
            lastSeenPlayerPos = player.position;

            // Reward only when the player is newly spotted
            if (!sawPlayerLastStep)
                AddReward(seenReward);

            float distanceToPlayer = Vector2.Distance(rb.position, player.position);
            if (distanceToPlayer <= catchDistance)
            {
                AddReward(catchReward);
                EndEpisode();
                return;
            }

            previousDistanceToLastSeen = distanceToPlayer;
        }
        else
        {
            timeSinceSeen += Time.fixedDeltaTime;

            if (hadSeenPlayerThisEpisode && timeSinceSeen < 2f)
            {
                float distanceToLastSeen = Vector2.Distance(rb.position, lastSeenPlayerPos);

                if (distanceToLastSeen < previousDistanceToLastSeen)
                    AddReward(pursueLastSeenReward);

                previousDistanceToLastSeen = distanceToLastSeen;
            }
        }

        sawPlayerLastStep = seesPlayer;

        if (rb.linearVelocity.magnitude < 0.05f)
            AddReward(idlePenalty);

        // Only run these during ML training, not normal gameplay
        if (Academy.Instance.IsCommunicatorOn)
        {
            UpdateHoverPenalty();
            UpdateNoSightTimeout();
            UpdateStuckCheck();
        }
    }

    private void UpdateHoverPenalty()
    {
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);

        bool isHoveringNearPlayer =
            distanceToPlayer <= hoverDistance &&
            distanceToPlayer > catchDistance;

        if (isHoveringNearPlayer)
        {
            hoverTimer += Time.fixedDeltaTime;

            if (hoverTimer >= hoverGraceTime)
            {
                hoverPenaltyTimer += Time.fixedDeltaTime;

                if (hoverPenaltyTimer >= hoverPenaltyInterval)
                {
                    AddReward(hoverPenalty);
                    hoverPenaltyTimer = 0f;
                }
            }
        }
        else
        {
            hoverTimer = 0f;
            hoverPenaltyTimer = 0f;
        }
    }

    private void UpdateNoSightTimeout()
    {
        if (timeSinceSeen >= maxTimeWithoutSeeingPlayer)
        {
            AddReward(noSightPenalty);
            EndEpisode();
        }
    }

    private void UpdateStuckCheck()
    {
        checkpointTimer += Time.fixedDeltaTime;

        if (checkpointTimer < checkpointInterval)
            return;

        float distanceMoved = Vector2.Distance(rb.position, checkpointPosition);

        if (distanceMoved < minDistanceMovedPerCheck)
        {
            stuckTimer += checkpointInterval;
            AddReward(stuckPenalty);

            if (stuckTimer >= stuckTimeLimit)
            {
                EndEpisode();
                return;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        checkpointPosition = rb.position;
        checkpointTimer = 0f;
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 testPos = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                Random.Range(-spawnRangeY, spawnRangeY),
                0f
            );

            bool tooCloseToPlayer = Vector2.Distance(testPos, player.position) < minSpawnDistanceFromPlayer;
            bool collidesWithWall = Physics2D.OverlapCircle(testPos, spawnCheckRadius, wallMask) != null;

            if (!tooCloseToPlayer && !collidesWithWall)
                return testPos;
        }

        return Vector3.zero;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxisRaw("Horizontal");
        actions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}