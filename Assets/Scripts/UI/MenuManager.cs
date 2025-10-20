using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Decel
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }
        public GameObject titleScreen;
        [SerializeField] private CanvasGroup buttonGroup;
        [SerializeField] private GameObject configScreen;
        [SerializeField] private GameObject pauseScreen;
        public GameObject saveLoadScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject deathScreen;

        private void Start()
        {
            Instance = this;

            StartCoroutine(TitleScreen());

            titleScreen.SetActive(true);
            configScreen.SetActive(false);
            pauseScreen.SetActive(false);
            saveLoadScreen.SetActive(false);
            hudScreen.SetActive(false);
            deathScreen.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseScreen(!pauseScreen.activeSelf);
            }
        }

        public void CursorVisible(bool state)
        {
            if (state && Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (!state && Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void ToGameFocused(bool state)
        {
            if (FadeManager.CurrentScene == 2)
            {
                if (state)
                {
                    CursorVisible(false);
                    // if (PhysicsManager.Instance != null)
                    // {
                    //     PhysicsManager.timeScaleIndex = 1;
                    // }
                    // FindObjectOfType<LocalCamera>(true).enabled = false;
                    // if (PhysicsManager.mainBody.TryGetComponent(out Vessel vessel))
                    // {
                    //     vessel.controlable = false;
                    // }
                }
                else
                {
                    CursorVisible(true);
                    // if (PhysicsManager.Instance != null)
                    // {
                    //     PhysicsManager.timeScaleIndex = 0;
                    // }
                    // FindObjectOfType<LocalCamera>(true).enabled = false;
                    // if (PhysicsManager.mainBody.TryGetComponent(out Vessel vessel))
                    // {
                    //     vessel.controlable = false;
                    // }
                }
            }
        }

        public void HudScreen(bool state)
        {
            if (state)
            {
                hudScreen.SetActive(true);
                ToGameFocused(true);
            }
            else
            {
                hudScreen.SetActive(false);
                ToGameFocused(false);
            }
        }

        public void PauseScreen(bool state)
        {
            if (!configScreen.activeSelf && !saveLoadScreen.activeSelf && FadeManager.CurrentScene != 1)
            {
                if (state)
                {
                    pauseScreen.SetActive(true);
                    ToGameFocused(false);
                }
                else
                {
                    pauseScreen.SetActive(false);
                    ToGameFocused(true);
                }
            }
        }

        public void ConfigScreen(bool state)
        {
            StartCoroutine(ConfigEnumerator(state));
        }

        public void SaveLoadScreen(bool state)
        {
            StartCoroutine(SaveLoadEnumerator(state));
        }

        public void DeathScreen()
        {
            StartCoroutine(DeathEnumerator());
        }

        public void NewGame()
        {
            StartCoroutine(NewGameEnumerator());
        }

        public void Play(int scene)
        {
            StartCoroutine(PlayEnumerator(scene));
        }

        public void Quit()
        {
            StartCoroutine(QuitEnumerator());
        }

        public void Exit()
        {
            StartCoroutine(ExitEnumerator());
        }

        private IEnumerator DeathEnumerator()
        {
            yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime * 4);
            deathScreen.SetActive(true);
            hudScreen.SetActive(false);
            yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime * 2);

            bool PressAnyKey()
            {
                return Input.anyKey;
            }

            CursorVisible(true);

            yield return new WaitUntil(PressAnyKey);
            yield return ExitEnumerator();
            deathScreen.SetActive(false);
        }

        private IEnumerator TitleScreen()
        {
            FadeManager.LoadScene(1, LoadSceneMode.Single);
            buttonGroup.alpha = 0;
            yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime * 4);
            yield return FadeManager.Fade(buttonGroup, FadeDirection.In, FadeManager.fadeTime);
        }

        private IEnumerator ConfigEnumerator(bool state)
        {
            if (state)
            {
                yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime);
                configScreen.SetActive(true);
                yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime);
            }
            else
            {
                yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime);
                configScreen.SetActive(false);
                yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime);
            }
        }

        private IEnumerator SaveLoadEnumerator(bool state)
        {
            if (state)
            {
                yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime);
                saveLoadScreen.SetActive(true);
                ToGameFocused(false);
                yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime);
            }
            else
            {
                yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime);
                saveLoadScreen.SetActive(false);
                ToGameFocused(true);
                yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime);
            }
        }

        private IEnumerator NewGameEnumerator()
        {
            buttonGroup.alpha = 1;
            yield return FadeManager.Fade(buttonGroup, FadeDirection.Out, FadeManager.fadeTime);
            yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime * 4);

            titleScreen.SetActive(false);
            //DialogueManager.Instance.dialogueContainer.root.SetActive(true);

            SavesManager.Instance.saveData = new SaveData()
            {
                saveName = "",
                playTime = 0,
            };

            CursorVisible(false);

            yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime * 4);

            // Play cutscene
            yield return new WaitForSeconds(2);

            //yield return StartConversation();

            yield return PlayEnumerator(2);
        }

        private IEnumerator PlayEnumerator(int scene)
        {
            yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime * 2);
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            titleScreen.SetActive(false);
            PoolManager.ClearAllPools();
            HudScreen(true);

            yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime * 2);
        }

        private IEnumerator QuitEnumerator()
        {
            yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime * 2);
            Application.Quit();
            if (Application.isEditor)
            {
                yield return FadeManager.Instance.Fade(FadeDirection.Out, FadeManager.fadeTime);
            }
        }

        private IEnumerator ExitEnumerator()
        {
            yield return FadeManager.Instance.Fade(FadeDirection.In, FadeManager.fadeTime);
            HudScreen(false);
            pauseScreen.SetActive(false);
            yield return SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
            Time.timeScale = 1;
            titleScreen.SetActive(true);
            StartCoroutine(TitleScreen());
        }
    }
}