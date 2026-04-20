using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    private SurvivalGamePrototype game;
    private Camera mainCamera;
    private Vector2 targetPosition;
    private Vector2 facing = Vector2.up;
    private Vector2 velocity;
    private float speed;
    private float cooldownRemaining;
    private const float Acceleration = 18f;
    private const float DecelerationDistance = 1.15f;
    private const float StopDistance = 0.04f;
    private const float RotationLerp = 18f;

    public float CooldownRemaining => Mathf.Max(0f, cooldownRemaining);

    public void Initialize(SurvivalGamePrototype owner, float moveSpeed)
    {
        game = owner;
        speed = moveSpeed;
        mainCamera = Camera.main;
        targetPosition = transform.position;
        velocity = Vector2.zero;
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
        velocity = Vector2.zero;
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
            RotateToFacing(true);
        }
    }

    private void HandleShootInput()
    {
        if (!PrototypeInput.FireHeld() || cooldownRemaining > 0f)
        {
            return;
        }

        FacePointerWhenPossible();
        cooldownRemaining = game.FireCooldown;
        game.CreateBullet((Vector2)transform.position + facing * 0.65f, facing);
    }

    private void MoveTowardTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 toTarget = targetPosition - currentPosition;

        if (toTarget.magnitude <= StopDistance)
        {
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, Acceleration * Time.deltaTime);
            transform.position = currentPosition + velocity * Time.deltaTime;
            return;
        }

        float slowDown = Mathf.Clamp01(toTarget.magnitude / DecelerationDistance);
        Vector2 desiredVelocity = toTarget.normalized * (speed * slowDown);
        velocity = Vector2.MoveTowards(velocity, desiredVelocity, Acceleration * Time.deltaTime);
        Vector2 nextPosition = currentPosition + velocity * Time.deltaTime;
        transform.position = ClampToMap(nextPosition);

        if (velocity.sqrMagnitude > 0.0001f)
        {
            facing = velocity.normalized;
            RotateToFacing(false);
        }
    }

    private Vector2 ClampToMap(Vector2 position)
    {
        float limit = game.MapHalfSize - 0.55f;
        return new Vector2(Mathf.Clamp(position.x, -limit, limit), Mathf.Clamp(position.y, -limit, limit));
    }

    private void FacePointerWhenPossible()
    {
        if (mainCamera == null || !PrototypeInput.PointerPosition(out Vector2 pointerPosition))
        {
            return;
        }

        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(pointerPosition);
        Vector2 aimDirection = mouseWorld - (Vector2)transform.position;
        if (aimDirection.sqrMagnitude <= 0.01f)
        {
            return;
        }

        facing = aimDirection.normalized;
        RotateToFacing(true);
    }

    private void RotateToFacing(bool snap)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f);
        transform.rotation = snap ? targetRotation : Quaternion.Lerp(transform.rotation, targetRotation, RotationLerp * Time.deltaTime);
    }
}
