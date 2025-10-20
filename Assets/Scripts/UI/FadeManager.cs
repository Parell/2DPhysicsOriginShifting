using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Decel
{
    public enum FadeDirection { In, Out }

    [DefaultExecutionOrder(-1)]
    public class FadeManager : MonoBehaviour
    {
        public static FadeManager Instance { get; private set; }
        [SerializeField] private float _fadeTime = 0.2f;
        [SerializeField] private CanvasGroup loadingScreen;
        public CanvasGroup fightScreen;
        [SerializeField] private int previousScene;
        public int currentScene;

        public static float fadeTime
        {
            get { return Instance._fadeTime; }
        }

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            previousScene = currentScene;
            currentScene = SceneManager.GetActiveScene().buildIndex;
            GetComponent<Canvas>().worldCamera = Camera.main;

            if (!FindObjectOfType<EventSystem>())
            {
                var newObject = new GameObject("EventSystem");
                newObject.AddComponent<EventSystem>();
                newObject.AddComponent<StandaloneInputModule>().forceModuleActive = true;
            }
        }

        private static void SetAlpha(CanvasGroup fade, ref float alpha, FadeDirection fadeDirection, float fadeTime)
        {
            fade.alpha = alpha;
            alpha += Time.unscaledDeltaTime * (1f / fadeTime) * ((fadeDirection == FadeDirection.Out) ? -1 : 1);
        }

        public IEnumerator Fade(FadeDirection fadeDirection, float fadeTime)
        {
            yield return Fade(loadingScreen, fadeDirection, fadeTime);
        }

        public static IEnumerator Fade(CanvasGroup fade, FadeDirection fadeDirection, float fadeTime)
        {
            fade.gameObject.SetActive(true);
            fade.alpha = fadeDirection == FadeDirection.Out ? 1 : 0;
            float alpha = (fadeDirection == FadeDirection.Out) ? 1 : 0;
            float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 0 : 1;
            if (fadeDirection == FadeDirection.Out)
            {
                while (alpha >= fadeEndValue)
                {
                    SetAlpha(fade, ref alpha, fadeDirection, fadeTime);
                    yield return null;
                }
                fade.gameObject.SetActive(false);
                fade.alpha = 0;
            }
            else
            {
                fade.gameObject.SetActive(true);
                while (alpha <= fadeEndValue)
                {
                    SetAlpha(fade, ref alpha, fadeDirection, fadeTime);
                    yield return null;
                }
                fade.alpha = 1;
            }
        }

        public static void LoadScene(int sceneToLoad, LoadSceneMode loadSceneMode)
        {
            SceneManager.LoadScene(sceneToLoad, loadSceneMode);
        }

        public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, int sceneToLoad, LoadSceneMode loadSceneMode, float fadeTime)
        {
            yield return Fade(fadeDirection, fadeTime);
            yield return SceneManager.LoadSceneAsync(sceneToLoad, loadSceneMode);
            yield return Fade(fadeDirection == FadeDirection.In ? FadeDirection.Out : FadeDirection.In, fadeTime);
        }

        public static int CurrentScene
        {
            get { return Instance.currentScene; }
        }

        public static int PreviousScene
        {
            get { return Instance.previousScene; }
        }
    }
}