using UnityEngine;
using Managers;

namespace Core.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Clips")]
        [SerializeField] private AudioClip mainTheme;

        [Header("Gameplay SFX")]
        public AudioClip winSound;
        public AudioClip loseSound;
        public AudioClip stealSound;
        public AudioClip buzzSound;
        public AudioClip clickSound;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // --- NEW: Start listening for volume changes ---
        private void OnEnable()
        {
            SettingsManager.OnAudioSettingsChanged += UpdateVolumes;
        }

        private void OnDisable()
        {
            SettingsManager.OnAudioSettingsChanged -= UpdateVolumes;
        }

        private void Start()
        {
            UpdateVolumes(); // Set the volumes right when the game starts
            PlayMusic(mainTheme);
        }

        // --- MUSIC ---
        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        // --- SFX ---
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            
            // We set the volume one more time right before playing just to be 100% safe
            if (SettingsManager.Instance != null) sfxSource.volume = SettingsManager.Instance.SfxVolume;
            
            sfxSource.PlayOneShot(clip); 
        }

        // --- THE FIX: Adjust the actual AudioSources ---
        private void UpdateVolumes()
        {
            if (SettingsManager.Instance == null) return;

            // Apply the float (0.0 to 1.0) directly to the Unity AudioSource component!
            if (musicSource != null)
            {
                musicSource.volume = SettingsManager.Instance.MusicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = SettingsManager.Instance.SfxVolume;
            }
        }
    }
}