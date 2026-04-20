using UnityEngine;

public class Enemy : MonoBehaviour
{
    private SurvivalGamePrototype game;
    private Transform target;
    private float speed;
    private float chaseSpeed;
    private float detectionRadius;
    private float mapHalfSize;
    private int health;
    private float contactCooldown;
    private float randomMoveTimer;
    private Vector2 randomDirection;

    public void Initialize(SurvivalGamePrototype owner, Transform chaseTarget, float moveSpeed, int startingHealth, float senseRadius, float mapLimit)
    {
        game = owner;
        target = chaseTarget;
        speed = moveSpeed;
        chaseSpeed = moveSpeed * 1.2f;
        detectionRadius = senseRadius;
        mapHalfSize = mapLimit;
        health = startingHealth;
        PickRandomDirection();
        ConfigureDetectionRadiusVisual();
    }

    private void Update()
    {
        if (game.IsGameOver || target == null)
        {
            return;
        }

        contactCooldown -= Time.deltaTime;

        Vector2 toPlayer = target.position - transform.position;
        if (toPlayer.sqrMagnitude <= detectionRadius * detectionRadius)
        {
            Move(toPlayer.normalized, chaseSpeed);
            return;
        }

        randomMoveTimer -= Time.deltaTime;
        if (randomMoveTimer <= 0f || IsNearMapEdge())
        {
            PickRandomDirection();
        }

        Move(randomDirection, speed);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            game.RegisterKill();
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerMover>() || contactCooldown > 0f)
        {
            return;
        }

        contactCooldown = 0.75f;
        game.DamagePlayer();
    }

    private void Move(Vector2 direction, float moveSpeed)
    {
        Vector2 nextPosition = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;
        float limit = mapHalfSize - 0.55f;

        if (nextPosition.x < -limit || nextPosition.x > limit)
        {
            randomDirection.x *= -1f;
        }

        if (nextPosition.y < -limit || nextPosition.y > limit)
        {
            randomDirection.y *= -1f;
        }

        nextPosition.x = Mathf.Clamp(nextPosition.x, -limit, limit);
        nextPosition.y = Mathf.Clamp(nextPosition.y, -limit, limit);
        transform.position = nextPosition;
    }

    private bool IsNearMapEdge()
    {
        float limit = mapHalfSize - 0.8f;
        Vector2 position = transform.position;
        return Mathf.Abs(position.x) > limit || Mathf.Abs(position.y) > limit;
    }

    private void PickRandomDirection()
    {
        randomDirection = Random.insideUnitCircle.normalized;
        if (randomDirection.sqrMagnitude < 0.01f)
        {
            randomDirection = Vector2.right;
        }

        randomMoveTimer = Random.Range(1.2f, 2.8f);
    }

    private void ConfigureDetectionRadiusVisual()
    {
        Transform existing = transform.Find("Detection Radius");
        GameObject radiusObject = existing != null ? existing.gameObject : new GameObject("Detection Radius");
        radiusObject.transform.SetParent(transform, false);
        radiusObject.transform.localPosition = Vector3.zero;

        LineRenderer line = radiusObject.GetComponent<LineRenderer>();
        if (line == null)
        {
            line = radiusObject.AddComponent<LineRenderer>();
        }

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 48;
        line.widthMultiplier = 0.035f;
        line.sortingOrder = 1;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1f, 0.25f, 0.2f, 0.35f);
        line.endColor = new Color(1f, 0.25f, 0.2f, 0.35f);

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i / (float)line.positionCount * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * detectionRadius, Mathf.Sin(angle) * detectionRadius, 0f));
        }
    }
}
