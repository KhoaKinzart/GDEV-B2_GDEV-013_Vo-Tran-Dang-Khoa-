using UnityEngine;
using UnityEngine.UI;

public class SurvivalGamePrototype : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Match")]
    [SerializeField] private float survivalDuration = 60f;
    [SerializeField] private float mapHalfSize = 10f;
    [SerializeField] private int startingPlayerHealth = 3;

    [Header("Player")]
    [SerializeField] private float playerSpeed = 6f;
    [SerializeField] private float fireCooldown = 0.35f;

    [Header("Enemy Spawning")]
    [SerializeField] private float enemySpeed = 2.6f;
    [SerializeField] private float enemyDetectionRadius = 3.5f;
    [SerializeField] private float spawnInterval = 1.6f;
    [SerializeField] private float minimumSpawnInterval = 0.45f;
    [SerializeField] private float minSpawnDistanceFromPlayer = 5f;
    [SerializeField] private float spawnRampPower = 1.25f;
    [SerializeField] private int maxEnemies = 45;

    [Header("Bullet")]
    [SerializeField] private float bulletSpeed = 12f;
    [Tooltip("How long a bullet stays alive, including after bouncing.")]
    public float bulletLifetime = 3.5f;
    [Tooltip("How many times a bullet can bounce off the square map boundary before being destroyed.")]
    public int bulletMaxBounces = 2;
    [SerializeField] private int bulletDamage = 1;

    private PlayerMover player;
    private Text hudText;
    private Text messageText;
    private float remainingTime;
    private float spawnTimer;
    private int playerHealth;
    private int kills;
    private bool gameOver;

    public float MapHalfSize => mapHalfSize;
    public float FireCooldown => fireCooldown;
    public float BulletSpeed => bulletSpeed;
    public float BulletLifetime => bulletLifetime;
    public int BulletMaxBounces => bulletMaxBounces;
    public int BulletDamage => bulletDamage;
    public bool IsGameOver => gameOver;

    private void OnValidate()
    {
        survivalDuration = Mathf.Max(1f, survivalDuration);
        mapHalfSize = Mathf.Max(3f, mapHalfSize);
        spawnInterval = Mathf.Max(0.05f, spawnInterval);
        minimumSpawnInterval = Mathf.Clamp(minimumSpawnInterval, 0.05f, spawnInterval);
        minSpawnDistanceFromPlayer = Mathf.Max(0f, minSpawnDistanceFromPlayer);
        spawnRampPower = Mathf.Max(0.1f, spawnRampPower);
        maxEnemies = Mathf.Max(1, maxEnemies);
        bulletLifetime = Mathf.Max(0.1f, bulletLifetime);
        bulletMaxBounces = Mathf.Max(0, bulletMaxBounces);
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        playerHealth = startingPlayerHealth;
        remainingTime = survivalDuration;
        BuildScene();
    }

    private void Update()
    {
        if (gameOver)
        {
            if (PrototypeInput.RestartPressed())
            {
                RestartMatch();
            }

            return;
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndGame(true);
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = GetCurrentSpawnInterval();
            SpawnEnemy();
        }

        UpdateHud();
    }

    public void RegisterKill()
    {
        kills++;
        UpdateHud();
    }

    public void DamagePlayer()
    {
        if (gameOver)
        {
            return;
        }

        playerHealth--;
        if (playerHealth <= 0)
        {
            playerHealth = 0;
            EndGame(false);
        }

        UpdateHud();
    }

    public GameObject CreateBullet(Vector2 position, Vector2 direction)
    {
        GameObject bulletObject = new GameObject("Bullet");
        bulletObject.transform.position = position;
        bulletObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        SpriteRenderer renderer = bulletObject.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.CreateBoxSprite(32, 12, Color.yellow);
        renderer.sortingOrder = 4;

        CircleCollider2D collider = bulletObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.16f;

        Rigidbody2D body = bulletObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        Bullet bullet = bulletObject.AddComponent<Bullet>();
        bullet.Initialize(this, direction.normalized);
        return bulletObject;
    }

    private void BuildScene()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = mapHalfSize + 2f;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
        }

        CreateMap();
        CreatePlayer();
        CreateHud();
        UpdateHud();
    }

    private void CreateMap()
    {
        GameObject floor = new GameObject("Square Survival Map");
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = Vector3.one * (mapHalfSize * 2f);
        SpriteRenderer floorRenderer = floor.AddComponent<SpriteRenderer>();
        floorRenderer.sprite = SpriteFactory.CreateBoxSprite(32, 32, new Color(0.16f, 0.18f, 0.20f));
        floorRenderer.sortingOrder = -2;

        CreateWall("North Wall", new Vector2(0f, mapHalfSize + 0.25f), new Vector2(mapHalfSize * 2f + 1f, 0.5f));
        CreateWall("South Wall", new Vector2(0f, -mapHalfSize - 0.25f), new Vector2(mapHalfSize * 2f + 1f, 0.5f));
        CreateWall("East Wall", new Vector2(mapHalfSize + 0.25f, 0f), new Vector2(0.5f, mapHalfSize * 2f + 1f));
        CreateWall("West Wall", new Vector2(-mapHalfSize - 0.25f, 0f), new Vector2(0.5f, mapHalfSize * 2f + 1f));
    }

    private void CreateWall(string wallName, Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.position = position;
        wall.transform.localScale = size;
        SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.CreateBoxSprite(16, 16, new Color(0.38f, 0.42f, 0.48f));
        renderer.sortingOrder = -1;
    }

    private void CreatePlayer()
    {
        GameObject playerObject = playerPrefab != null ? Instantiate(playerPrefab) : CreateDefaultPlayerObject();
        playerObject.name = "Player";
        playerObject.transform.position = Vector3.zero;
        ConfigurePlayerObject(playerObject);

        player = playerObject.GetComponent<PlayerMover>();
        if (player == null)
        {
            player = playerObject.AddComponent<PlayerMover>();
        }

        player.Initialize(this, playerSpeed);
    }

    private void CreateHud()
    {
        GameObject canvasObject = new GameObject("Prototype HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        hudText = PrototypeHud.CreateText(canvasObject.transform, "HUD Text", new Vector2(16f, -16f), TextAnchor.UpperLeft, 24, Color.white);
        messageText = PrototypeHud.CreateText(canvasObject.transform, "Message Text", Vector2.zero, TextAnchor.MiddleCenter, 42, new Color(1f, 0.95f, 0.55f));
        messageText.text = string.Empty;
    }

    private void SpawnEnemy()
    {
        if (FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length >= maxEnemies)
        {
            return;
        }

        Vector2 spawnPosition = GetEdgeSpawnPosition();
        GameObject enemyObject = enemyPrefab != null ? Instantiate(enemyPrefab) : CreateDefaultEnemyObject();
        enemyObject.name = "Enemy";
        enemyObject.transform.position = spawnPosition;
        ConfigureEnemyObject(enemyObject);

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = enemyObject.AddComponent<Enemy>();
        }

        enemy.Initialize(this, player.transform, enemySpeed, 1, enemyDetectionRadius, mapHalfSize);
    }

    private GameObject CreateDefaultPlayerObject()
    {
        GameObject playerObject = new GameObject("Player");
        ConfigurePlayerObject(playerObject);
        return playerObject;
    }

    private void ConfigurePlayerObject(GameObject playerObject)
    {
        SpriteRenderer renderer = GetOrAddComponent<SpriteRenderer>(playerObject);
        renderer.sprite = SpriteFactory.CreateCircleSprite(48, new Color(0.2f, 0.75f, 1f));
        renderer.sortingOrder = 3;

        CircleCollider2D collider = GetOrAddComponent<CircleCollider2D>(playerObject);
        collider.isTrigger = true;
        collider.radius = 0.42f;

        Rigidbody2D body = GetOrAddComponent<Rigidbody2D>(playerObject);
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        GetOrAddComponent<PlayerMover>(playerObject);
    }

    private GameObject CreateDefaultEnemyObject()
    {
        GameObject enemyObject = new GameObject("Enemy");
        ConfigureEnemyObject(enemyObject);
        return enemyObject;
    }

    private void ConfigureEnemyObject(GameObject enemyObject)
    {
        SpriteRenderer renderer = GetOrAddComponent<SpriteRenderer>(enemyObject);
        renderer.sprite = SpriteFactory.CreateCircleSprite(42, new Color(1f, 0.2f, 0.16f));
        renderer.sortingOrder = 2;

        CircleCollider2D collider = GetOrAddComponent<CircleCollider2D>(enemyObject);
        collider.isTrigger = true;
        collider.radius = 0.45f;

        Rigidbody2D body = GetOrAddComponent<Rigidbody2D>(enemyObject);
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        GetOrAddComponent<Enemy>(enemyObject);
    }

    private T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private float GetCurrentSpawnInterval()
    {
        float safeMinimum = Mathf.Max(0.05f, minimumSpawnInterval);
        float startInterval = Mathf.Max(spawnInterval, safeMinimum);
        float elapsedProgress = survivalDuration <= 0f ? 1f : 1f - remainingTime / survivalDuration;
        float ramp = Mathf.Pow(Mathf.Clamp01(elapsedProgress), Mathf.Max(0.1f, spawnRampPower));
        return Mathf.Lerp(startInterval, safeMinimum, ramp);
    }

    private Vector2 GetEdgeSpawnPosition()
    {
        Vector2 playerPosition = player != null ? (Vector2)player.transform.position : Vector2.zero;
        Vector2 bestPosition = Vector2.zero;
        float bestDistanceSqr = -1f;
        float minimumDistanceSqr = minSpawnDistanceFromPlayer * minSpawnDistanceFromPlayer;

        for (int i = 0; i < 24; i++)
        {
            Vector2 candidate = GetRandomEdgePosition();
            float distanceSqr = (candidate - playerPosition).sqrMagnitude;
            if (distanceSqr >= minimumDistanceSqr)
            {
                return candidate;
            }

            if (distanceSqr > bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestPosition = candidate;
            }
        }

        return bestPosition;
    }

    private Vector2 GetRandomEdgePosition()
    {
        float edge = Random.Range(0, 4);
        float paddedEdge = mapHalfSize - 0.7f;
        float randomAxis = Random.Range(-paddedEdge, paddedEdge);

        if (edge < 1f)
        {
            return new Vector2(randomAxis, paddedEdge);
        }

        if (edge < 2f)
        {
            return new Vector2(randomAxis, -paddedEdge);
        }

        if (edge < 3f)
        {
            return new Vector2(paddedEdge, randomAxis);
        }

        return new Vector2(-paddedEdge, randomAxis);
    }

    private void EndGame(bool survived)
    {
        gameOver = true;
        messageText.text = survived ? "YOU WIN\nPress R to restart" : "YOU LOSE\nPress R to restart";
        messageText.color = survived ? new Color(0.65f, 1f, 0.55f) : new Color(1f, 0.35f, 0.3f);
    }

    private void RestartMatch()
    {
        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            Destroy(enemy.gameObject);
        }

        foreach (Bullet bullet in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
        {
            Destroy(bullet.gameObject);
        }

        playerHealth = startingPlayerHealth;
        remainingTime = survivalDuration;
        kills = 0;
        spawnTimer = 0f;
        gameOver = false;
        player.transform.position = Vector3.zero;
        player.ResetMovement();
        messageText.text = string.Empty;
        UpdateHud();
    }

    private void UpdateHud()
    {
        if (hudText == null || player == null)
        {
            return;
        }

        hudText.text = "Survive: " + Mathf.CeilToInt(remainingTime) + "s"
            + "\nHealth: " + playerHealth
            + "\nKills: " + kills
            + "\nSpawn interval: " + GetCurrentSpawnInterval().ToString("0.00") + "s"
            + "\nFire cooldown: " + player.CooldownRemaining.ToString("0.00") + "s"
            + "\nLeft click: move | Space: fire | R: restart";
    }
}
