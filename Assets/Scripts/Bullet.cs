using UnityEngine;

public class Bullet : MonoBehaviour
{
    private SurvivalGamePrototype game;
    private Vector2 direction;
    private float lifetime;

    public void Initialize(SurvivalGamePrototype owner, Vector2 travelDirection)
    {
        game = owner;
        direction = travelDirection;
        lifetime = game.BulletLifetime;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * game.BulletSpeed * Time.deltaTime);
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
}
