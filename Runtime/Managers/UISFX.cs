using System;
using System.Reflection;
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
    /// Soft bridge to project SFXManager. Safe when SFXManager is missing.
    /// UI clips play on SFXManager's dedicated UI AudioSource when available.
    /// </summary>
    public static class UISFX
    {
        private const string ManagerTypeName = "SFXManager";
        private static bool resolved;
        private static PropertyInfo instanceProperty;
        private static MethodInfo clickMethod;
        private static MethodInfo showMethod;
        private static MethodInfo hideMethod;
        private static MethodInfo playClipMethod;
        private static MethodInfo playClipVolumeMethod;
        private static AudioSource fallbackSource;

        public static void Play(UISFXKind kind, AudioClip overrideClip, bool mute)
        {
            if (mute || !Application.isPlaying)
            {
                return;
            }

            if (overrideClip != null)
            {
                if (!TryPlayClipOnManager(overrideClip))
                {
                    PlayFallback(overrideClip);
                }

                return;
            }

            EnsureResolved();
            MethodInfo method = GetMethod(kind);
            object instance = GetInstance();
            if (method == null || instance == null)
            {
                return;
            }

            try
            {
                method.Invoke(instance, null);
            }
            catch (Exception)
            {
                // Ignore missing/broken SFXManager bindings.
            }
        }

        private static bool TryPlayClipOnManager(AudioClip clip)
        {
            EnsureResolved();
            object instance = GetInstance();
            if (instance == null || clip == null)
            {
                return false;
            }

            try
            {
                if (playClipVolumeMethod != null)
                {
                    playClipVolumeMethod.Invoke(instance, new object[] { clip, 0.4f });
                    return true;
                }

                if (playClipMethod != null)
                {
                    playClipMethod.Invoke(instance, new object[] { clip });
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private static void PlayFallback(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (fallbackSource == null)
            {
                GameObject go = new GameObject("[UI System Audio]");
                UnityEngine.Object.DontDestroyOnLoad(go);
                fallbackSource = go.AddComponent<AudioSource>();
                fallbackSource.playOnAwake = false;
                fallbackSource.spatialBlend = 0f;
            }

            fallbackSource.PlayOneShot(clip, 0.4f);
        }

        private static object GetInstance()
        {
            EnsureResolved();
            if (instanceProperty == null)
            {
                return null;
            }

            try
            {
                return instanceProperty.GetValue(null, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static MethodInfo GetMethod(UISFXKind kind)
        {
            switch (kind)
            {
                case UISFXKind.ContainerShow:
                    return showMethod;
                case UISFXKind.ContainerHide:
                    return hideMethod;
                default:
                    return clickMethod;
            }
        }

        private static void EnsureResolved()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            Type type = FindType(ManagerTypeName);
            if (type == null)
            {
                return;
            }

            instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            clickMethod = type.GetMethod("UIClickSFX", BindingFlags.Public | BindingFlags.Instance);
            showMethod = type.GetMethod("UIContainerShowSFX", BindingFlags.Public | BindingFlags.Instance);
            hideMethod = type.GetMethod("UIContainerHideSFX", BindingFlags.Public | BindingFlags.Instance);
            playClipMethod = type.GetMethod("PlayUIClip", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(AudioClip) }, null);
            playClipVolumeMethod = type.GetMethod("PlayUIClip", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(AudioClip), typeof(float) }, null);
        }

        private static Type FindType(string typeName)
        {
            Type direct = Type.GetType(typeName);
            if (direct != null)
            {
                return direct;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
