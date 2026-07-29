using UnityEngine;

namespace Project.UI
{
    public enum UISFXKind
    {
        Click = 0,
        ContainerShow = 1,
        ContainerHide = 2
    }

    /// <summary>
    /// Bridge to project SFXManager. SFXManager registers a handler in Awake.
    /// <see cref="UIButton.customClickSound"/> overrides the global clip only when not null.
    /// </summary>
    public static class UISFX
    {
        public delegate void PlayHandler(UISFXKind kind, AudioClip overrideClip);

        static PlayHandler handler;
        static AudioSource fallbackSource;

        public static void SetHandler(PlayHandler playHandler)
        {
            handler = playHandler;
        }

        public static void Play(UISFXKind kind, AudioClip overrideClip, bool mute)
        {
            if (mute || !Application.isPlaying)
                return;

            if (handler != null)
            {
                handler(kind, overrideClip);
                return;
            }

            // SFXManager not ready — only custom clips can still play.
            if (overrideClip != null)
            {
                Debug.LogWarning("[UISFX] SFXManager handler missing — fallback for custom clip.");
                PlayFallback(overrideClip);
            }
            else
            {
                Debug.LogWarning("[UISFX] SFXManager handler missing and no custom clip — UI SFX skipped.");
            }
        }

        public static void PlayFallback(AudioClip clip)
        {
            if (clip == null)
                return;

            if (fallbackSource == null)
            {
                GameObject go = new GameObject("[UI System Audio]");
                Object.DontDestroyOnLoad(go);
                fallbackSource = go.AddComponent<AudioSource>();
                fallbackSource.playOnAwake = false;
            }

            fallbackSource.spatialBlend = 0f;
            fallbackSource.mute = false;
            fallbackSource.ignoreListenerPause = true;
            fallbackSource.ignoreListenerVolume = true;
            fallbackSource.PlayOneShot(clip, 0.4f);
        }
    }
}
