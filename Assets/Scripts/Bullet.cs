using UnityEngine;

public class Bullet : MonoBehaviour
{
    private SurvivalGamePrototype game;
    private Vector2 direction;
    private float lifetime;
    private int bouncesRemaining;
    private const float BoundaryPadding = 0.18f;

    public void Initialize(SurvivalGamePrototype owner, Vector2 travelDirection)
    {
        game = owner;
        direction = travelDirection;
        lifetime = game.BulletLifetime;
        bouncesRemaining = game.BulletMaxBounces;
    }

    private void Update()
    {
        Vector2 nextPosition = (Vector2)transform.position + direction * game.BulletSpeed * Time.deltaTime;
        if (!TryBounceAtBoundary(ref nextPosition))
        {
            Destroy(gameObject);
            return;
        }

        transform.position = nextPosition;
        RotateToDirection();

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null)
        {
            return;
        }

        enemy.TakeDamage(game.BulletDamage);
        Destroy(gameObject);
    }

    private bool TryBounceAtBoundary(ref Vector2 nextPosition)
    {
        float limit = game.MapHalfSize - BoundaryPadding;
        bool hitHorizontalWall = nextPosition.x < -limit || nextPosition.x > limit;
        bool hitVerticalWall = nextPosition.y < -limit || nextPosition.y > limit;

        if (!hitHorizontalWall && !hitVerticalWall)
        {
            return true;
        }

        if (bouncesRemaining <= 0)
        {
            return false;
        }

        bouncesRemaining--;

        if (hitHorizontalWall)
        {
            direction.x *= -1f;
            nextPosition.x = Mathf.Clamp(nextPosition.x, -limit, limit);
        }

        if (hitVerticalWall)
        {
            direction.y *= -1f;
            nextPosition.y = Mathf.Clamp(nextPosition.y, -limit, limit);
        }

        return true;
    }

    private void RotateToDirection()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }
}
