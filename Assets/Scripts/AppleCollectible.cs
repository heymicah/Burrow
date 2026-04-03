using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AppleCollectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rabbit"))
        {
            GameManager.Instance.CollectApple();
            gameObject.SetActive(false);
        }
    }
}
