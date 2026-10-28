using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collectible touched by: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Player collected item");

        if (RoundManager.Instance != null)
            RoundManager.Instance.RegisterItemCollected();

        if (CollectibleSpawner.Instance != null)
            CollectibleSpawner.Instance.OnCollectiblePickedUp(gameObject);
    }
}