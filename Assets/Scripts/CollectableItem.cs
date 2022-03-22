using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectableItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ( collision.CompareTag("Player"))
        {
            Destroy(this.gameObject);
            print("You have found GOBLET");
        }
    }
}