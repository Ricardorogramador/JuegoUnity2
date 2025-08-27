using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BattleMusicRandom : MonoBehaviour
{
    [Header("Clips para batalla (elige 1 al azar)")]
    [SerializeField] private AudioClip[] tracks;

    [Header("Opciones")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.45f;
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private bool avoidRepeatConsecutive = true;

    private AudioSource src;

    // Mantiene el último índice para evitar repetir consecutivamente (por sesión)
    private static int lastIndex = -1;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
        src.volume = volume;
    }

    private void Start()
    {
        if (autoPlayOnStart) PlayRandom();
    }

    public void PlayRandom()
    {
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogWarning("[BattleMusicRandom] No hay tracks asignados.");
            return;
        }

        int index = GetRandomIndex();
        var clip = tracks[index];
        lastIndex = index;

        src.clip = clip;
        src.volume = volume; // por si cambió en el inspector
        src.Play();

        Debug.Log($"[BattleMusicRandom] Reproduciendo: {clip.name}");
    }

    private int GetRandomIndex()
    {
        if (tracks.Length == 1 || !avoidRepeatConsecutive || lastIndex < 0 || lastIndex >= tracks.Length)
            return Random.Range(0, tracks.Length);

        // Evitar repetir el último inmediatamente
        int idx;
        do { idx = Random.Range(0, tracks.Length); }
        while (idx == lastIndex && tracks.Length > 1);

        return idx;
    }

    // Por si quieres forzar un cambio en runtime (botón, evento, etc.)
    public void SwitchToAnotherRandom()
    {
        PlayRandom();
    }
}