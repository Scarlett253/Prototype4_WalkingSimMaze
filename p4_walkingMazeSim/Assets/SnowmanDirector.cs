using System.Collections.Generic;
using UnityEngine;

public class SnowmanDirector : MonoBehaviour
{
   public static SnowmanDirector Instance { get; private set; }

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Camera playerCamera;
    [SerializeField] GameObject snowmanPrefab;

    [Header("Quantity")]
    [SerializeField] int maxSnowmen = 10;
    [SerializeField] float spawnInterval = 18f;

    [Header("Spawn")]
    [SerializeField] float minSpawnDistance = 12f;
    [SerializeField] float maxSpawnDistance = 28f;
    [SerializeField] float spawnHeight = 1f;
    [SerializeField] LayerMask wallMask;

    [Header("Debug")]
    [SerializeField] bool logSpawns = false;

    readonly List<SnowmanController> _snowmen = new List<SnowmanController>();
    readonly Plane[] _frustumPlanes = new Plane[6];
    float _timer;

    public int ActiveCount => _snowmen.Count;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (playerCamera == null && player != null)
            playerCamera = player.GetComponentInChildren<Camera>();

        _timer = spawnInterval;
    }

    void Update()
    {
        if (player == null || playerCamera == null) return;

        UpdateVisibility();
        UpdateSpawnTimer();
    }

    void UpdateVisibility()
    {
        GeometryUtility.CalculateFrustumPlanes(playerCamera, _frustumPlanes);

        foreach (SnowmanController snowman in _snowmen)
        {
            if (snowman == null) continue;

            Bounds bounds = new Bounds(snowman.transform.position + Vector3.up, Vector3.one * 1.6f);
            snowman.SetWatched(GeometryUtility.TestPlanesAABB(_frustumPlanes, bounds));
        }
    }

    void UpdateSpawnTimer()
    {
        _timer -= Tieme.deltaTime;
        if (_timer > 0f) return;

        _timer = spawnInterval;
        SpawnOrRecycle();
    }

    void SpawnOrRecycle()
    {
        if (_snowmen.Count < maxSnowmen)
            SpawnSnowman();
        else
            RecycleSnowman();
    }

    void SpawnSnowman()
    {
        GameObject instance = Instantiate(snowmanPrefab);
        SnowmanController snowman = instance.GetComponent<SnowmanController>();

        if (snowman == null)
        {
            Debug.LogError("Snowman's prefab doesn't has SnowmanController.");
            Destroy(instance);
            return;
        }

        _snowmen.Add(snowman);
        PlaceOutOfSight(snowman);
        snowman.Init(player);
    }

    void RecycleSnowman()
    {
        SnowmanController snowman = _snowmen[Random.Range(0, _snowmen.Count)];
        if (snowman != null) PlaceOutOfSight(snowman);
    }

    void PlaceOutOfSight(SnowmanController snowman)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            Vector3 candidate = GetRandomCandidate();
            if (IsInsideFrustum(candidate)) continue;
            if (IsBlocked(candidate)) continue;

            snowman.transform.position = candidate;
            if (logSpawns) Debug.Log($"Snowman puts a {candidate}");
            return;
        }
    }

    Vector3 GetRandomCandidate()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 offset = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * distance;
        Vector3 candidate = player.position + offset;
        candidate.y = spawnHeight;
        return candidate;
    }

    bool IsInsideFrustum(Vector3 position)
    {
        Bounds bounds = new Bounds(position + Vector3.up, Vector3.one * 1.6f);
        return GeometryUtility.TestPlanesAABB(_frustumPlanes, bounds);
    }

    bool IsBlocked(Vector3 position)
    {
        return Physics.CheckSphere(position + Vector3.up * 0.5f, 0.6f, wallMask);
    }

    public void OnPlayerCaught()
    {
        Debug.Log("Snowman got you.");
        //Gameover...
    }
}
