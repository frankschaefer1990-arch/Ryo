using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 0.5f;
    public float crossfadeDuration = 1.5f;

    [Header("Scene Mappings")]
    public List<SceneMusic> sceneMusicList = new List<SceneMusic>();

    private AudioSource audioSourceA;
    private AudioSource audioSourceB;
    private bool usingSourceA = true;
    private string currentSceneName = "";
    private bool isCrossfading = false;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        // Two sources are needed for smooth crossfading
        audioSourceA = gameObject.AddComponent<AudioSource>();
        audioSourceB = gameObject.AddComponent<AudioSource>();

        audioSourceA.loop = true;
        audioSourceB.loop = true;
        
        audioSourceA.playOnAwake = false;
        audioSourceB.playOnAwake = false;
    }

    private void OnEnable()
    {
        // Subscribe to the global ready signal
        GameManager.OnSystemsReady += UpdateMusicForScene;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        GameManager.OnSystemsReady -= UpdateMusicForScene;
    }

    private void Update()
    {
        // Real-time volume update from Inspector
        if (!isCrossfading)
        {
            AudioSource activeSource = usingSourceA ? audioSourceA : audioSourceB;
            activeSource.volume = masterVolume;
        }
    }

    private void Start()
    {
        // Initial check in case scene was already loaded before subscription
        UpdateMusicForScene();
    }

    private void UpdateMusicForScene()
    {
        string newScene = SceneManager.GetActiveScene().name;
        Debug.Log($"AudioManager: Updating music for scene [{newScene}]. Current track for: [{currentSceneName}]");
        
        // Prevent restarting the same music when reloading or transitioning to same scene
        if (newScene == currentSceneName) 
        {
            Debug.Log("AudioManager: Scene unchanged, keeping current music.");
            return;
        }
        currentSceneName = newScene;

        AudioClip clipToPlay = null;
        foreach (var mapping in sceneMusicList)
        {
            if (mapping.sceneName == newScene)
            {
                clipToPlay = mapping.musicClip;
                break;
            }
        }

        if (clipToPlay != null)
        {
            StartCoroutine(CrossfadeMusic(clipToPlay));
        }
        else
        {
            // Optional: Fade out if no music is defined for the scene
            StartCoroutine(FadeOutCurrent());
        }
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        isCrossfading = true;
        AudioSource fadeOutSource = usingSourceA ? audioSourceA : audioSourceB;
        AudioSource fadeInSource = usingSourceA ? audioSourceB : audioSourceA;

        // If the same clip is already playing, just ensure it's at the right volume
        if (fadeOutSource.clip == newClip && fadeOutSource.isPlaying)
        {
            isCrossfading = false;
            yield break;
        }

        fadeInSource.clip = newClip;
        fadeInSource.volume = 0;
        fadeInSource.Play();

        float startVolume = fadeOutSource.volume;
        float elapsed = 0;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / crossfadeDuration;

            fadeOutSource.volume = Mathf.Lerp(startVolume, 0, normalizedTime);
            fadeInSource.volume = Mathf.Lerp(0, masterVolume, normalizedTime);

            yield return null;
        }

        fadeOutSource.Stop();
        fadeOutSource.volume = 0;
        fadeInSource.volume = masterVolume;

        usingSourceA = !usingSourceA;
        isCrossfading = false;
    }

    private IEnumerator FadeOutCurrent()
    {
        isCrossfading = true;
        AudioSource fadeOutSource = usingSourceA ? audioSourceA : audioSourceB;
        float startVolume = fadeOutSource.volume;
        float elapsed = 0;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeOutSource.volume = Mathf.Lerp(startVolume, 0, elapsed / crossfadeDuration);
            yield return null;
        }

        fadeOutSource.Stop();
        isCrossfading = false;
    }

    // Public method for special events like battles
    public void PlayOverrideMusic(AudioClip clip)
    {
        if (clip == null) return;
        StartCoroutine(CrossfadeMusic(clip));
    }

    public void StopAllMusic()
    {
        StopAllCoroutines();
        if (audioSourceA != null) audioSourceA.Stop();
        if (audioSourceB != null) audioSourceB.Stop();
        isCrossfading = false;
    }
    }