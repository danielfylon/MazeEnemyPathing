using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [SerializeField] private float minPitch = 0.92f;
    [SerializeField] private float maxPitch = 1.08f;
    [SerializeField] private float volume = 0.7f;

    public void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[
            Random.Range(0, footstepClips.Length)
        ];

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }
}