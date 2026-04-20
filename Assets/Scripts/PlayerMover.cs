using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    private SurvivalGamePrototype game;
    private Camera mainCamera;
    private Vector2 targetPosition;
    private Vector2 facing = Vector2.up;
    private float speed;
    private float cooldownRemaining;

    public float CooldownRemaining => Mathf.Max(0f, cooldownRemaining);

    public void Initialize(SurvivalGamePrototype owner, float moveSpeed)
    {
        game = owner;
        speed = moveSpeed;
        mainCamera = Camera.main;
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (game.IsGameOver)
        {
            return;
        }

        cooldownRemaining -= Time.deltaTime;
        HandleMoveInput();
        HandleShootInput();
        MoveTowardTarget();
    }

    public void ResetMovement()
    {
        targetPosition = transform.position;
        facing = Vector2.up;
        cooldownRemaining = 0f;
        transform.rotation = Quaternion.identity;
    }

    private void HandleMoveInput()
    {
        if (!PrototypeInput.MovePressed(out Vector2 pointerPosition) || mainCamera == null)
        {
            return;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(pointerPosition);
        targetPosition = ClampToMap(mouseWorld);
        Vector2 direction = targetPosition - (Vector2)transform.position;
        if (direction.sqrMagnitude > 0.01f)
        {
            facing = direction.normalized;
            RotateToFacing();
        }
    }

    private void HandleShootInput()
    {
        if (!PrototypeInput.FirePressed() || cooldownRemaining > 0f)
        {
            return;
        }

        cooldownRemaining = game.FireCooldown;
        game.CreateBullet((Vector2)transform.position + facing * 0.65f, facing);
    }

    private void MoveTowardTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
        transform.position = ClampToMap(nextPosition);

        Vector2 movement = nextPosition - currentPosition;
        if (movement.sqrMagnitude > 0.0001f)
        {
            facing = movement.normalized;
            RotateToFacing();
        }
    }

    private Vector2 ClampToMap(Vector2 position)
    {
        float limit = game.MapHalfSize - 0.55f;
        return new Vector2(Mathf.Clamp(position.x, -limit, limit), Mathf.Clamp(position.y, -limit, limit));
    }

    private void RotateToFacing()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f);
    }
}
