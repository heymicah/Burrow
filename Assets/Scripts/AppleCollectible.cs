using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AppleCollectible : MonoBehaviour
{
    [SerializeField] private AudioClip dingSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rabbit"))
        {
            if (dingSound != null)
                AudioSource.PlayClipAtPoint(dingSound, transform.position);

            GameManager.Instance.CollectApple();
            gameObject.SetActive(false);
        }
    }
}
