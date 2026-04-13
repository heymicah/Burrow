using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float minInterval = 3f;
    [SerializeField] private float maxInterval = 10f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        float delay = Random.Range(minInterval, maxInterval);
        Invoke(nameof(PlayRandom), delay);
    }

    private void PlayRandom()
    {
        if (clips.Length == 0) return;
        audioSource.clip = clips[Random.Range(0, clips.Length)];
        audioSource.Play();
        ScheduleNext();
    }
}