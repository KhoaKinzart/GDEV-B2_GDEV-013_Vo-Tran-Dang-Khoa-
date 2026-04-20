using UnityEngine;

public class Enemy : MonoBehaviour
{
    private SurvivalGamePrototype game;
    private Transform target;
    private float speed;
    private int health;
    private float contactCooldown;

    public void Initialize(SurvivalGamePrototype owner, Transform chaseTarget, float moveSpeed, int startingHealth)
    {
        game = owner;
        target = chaseTarget;
        speed = moveSpeed;
        health = startingHealth;
    }

    private void Update()
    {
        if (game.IsGameOver || target == null)
        {
            return;
        }

        contactCooldown -= Time.deltaTime;
        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
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
}
