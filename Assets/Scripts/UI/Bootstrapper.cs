using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Decel
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private new bool enabled = true;
        [SerializeField] private bool startImmediately = true;
        private FadeManager loadingManager;

        private void Awake()
        {
            if (!enabled) { return; }
            loadingManager = FadeManager.Instance;
            if (loadingManager == null)
            {
                if (startImmediately)
                {
                    DontDestroyOnLoad(gameObject);
                    var enumerator = ColdStartup();
                    StartCoroutine(enumerator);
                }
                else
                {
                    SceneManager.LoadScene(0, LoadSceneMode.Single);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator ColdStartup()
        {
            yield return SceneManager.LoadSceneAsync(0, 0);
            yield return new WaitUntil(MenuManagerIsInstanced);
            MenuManager.Instance.Play(2);
            Destroy(gameObject);

            bool MenuManagerIsInstanced()
            {
                return MenuManager.Instance != null;
            }
        }
    }
}