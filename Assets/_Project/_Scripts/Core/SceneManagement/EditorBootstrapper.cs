// Core/SceneManagement/EditorBootstrapper.cs

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SabanCoreTemplate.SceneManagement
{
    [InitializeOnLoad]
    public static class EditorBootstrapper
    {
        private const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";
        private const string PreviousSceneKey = "EditorBootstrapper_PreviousScene";

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            // --- OYUNA GİRERKEN ---
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                string currentScene = SceneManager.GetActiveScene().path;

                // Eğer zaten Boot sahnesindeysek bir şey yapmaya gerek yok
                if (currentScene == BootScenePath) return;

                // Mevcut sahneyi hafızaya kaydet
                EditorPrefs.SetString(PreviousSceneKey, currentScene);

                // Değişiklikleri kaydetmeyi sor (İptal edilirse Play'i durdur)
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorApplication.isPlaying = false;
                    return;
                }

                // Boot sahnesine geç
                EditorSceneManager.OpenScene(BootScenePath);
            }

            // --- OYUNDAN ÇIKARKEN ---
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Hafızada kayıtlı bir sahne var mı bak
                if (EditorPrefs.HasKey(PreviousSceneKey))
                {
                    string lastScene = EditorPrefs.GetString(PreviousSceneKey);

                    // Kayıtlı sahne mevcut sahneden farklıysa (yani şu an Boot'taysak) geri dön
                    if (!string.IsNullOrEmpty(lastScene) && SceneManager.GetActiveScene().path != lastScene)
                    {
                        EditorSceneManager.OpenScene(lastScene);
                    }
                }
            }
        }
    }
}
#endif