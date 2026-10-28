using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    public static CollectibleSpawner Instance;

    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private Transform[] spawnPoints;

    private GameObject currentCollectible;
    private int lastSpawnIndex = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnCollectible();
    }

    public void SpawnCollectible()
    {
        if (collectiblePrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        int newIndex = GetNextSpawnIndex();

        Transform spawnPoint = spawnPoints[newIndex];
        currentCollectible = Instantiate(collectiblePrefab, spawnPoint.position, Quaternion.identity);

        lastSpawnIndex = newIndex;
    }

    public void OnCollectiblePickedUp(GameObject collectedItem)
    {
        if (collectedItem != null)
            Destroy(collectedItem);

        currentCollectible = null;
        SpawnCollectible();
    }

    public void RespawnForNewRound()
    {
        if (currentCollectible != null)
            Destroy(currentCollectible);

        currentCollectible = null;
        SpawnCollectible();
    }

    private int GetNextSpawnIndex()
    {
        if (spawnPoints.Length == 1)
            return 0;

        int newIndex = lastSpawnIndex;

        while (newIndex == lastSpawnIndex)
        {
            newIndex = Random.Range(0, spawnPoints.Length);
        }

        return newIndex;
    }
}