using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public sealed class WavePause : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private static int inputBlockedFrame = -1;

    public bool IsPaused { get; private set; }
    public static bool BlocksGameplayInput => Time.timeScale == 0f || Time.frameCount == inputBlockedFrame;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void ReturnToMenu()
    {
        FindObjectOfType<WaveProgression>()?.AbandonWave();
        SetPaused(false);
        ReleaseCursor();
        SceneManager.LoadScene("MainMenu");
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        panel.SetActive(paused);
        inputBlockedFrame = Time.frameCount;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    private void OnDisable()
    {
        if (IsPaused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            IsPaused = false;
        }
        ReleaseCursor();
    }

    private static void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
