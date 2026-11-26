using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CureEpilogue : MonoBehaviour
{
    [SerializeField] private Camera view;
    [SerializeField] private Transform intakeStart;
    [SerializeField] private Transform intakeEnd;
    [SerializeField] private Transform trialStart;
    [SerializeField] private Transform trialEnd;
    [SerializeField] private GameObject greyRig;
    [SerializeField] private GameObject assistantRig;
    [SerializeField] private AnimationClip greyReach;
    [SerializeField] private AnimationClip assistantReach;
    [SerializeField] private GameObject trialLighting;
    [SerializeField] private TMP_Text monitor;
    [SerializeField] private TMP_Text captionTitle;
    [SerializeField] private TMP_Text caption;
    [SerializeField] private Button continueButton;

    private const float IntakeSeconds = 5.5f;
    private const float TrialMoveSeconds = 7f;
    private float startedAt;
    private bool trialStarted;
    private bool leaving;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        startedAt = Time.unscaledTime;
        continueButton.gameObject.SetActive(false);
        continueButton.onClick.AddListener(ContinueToWinner);
        trialLighting.SetActive(false);
        FrameShot(intakeStart, intakeEnd, 0f);
    }

    private void Update()
    {
        float elapsed = Time.unscaledTime - startedAt;
        if (elapsed < IntakeSeconds)
        {
            FrameShot(intakeStart, intakeEnd, elapsed / IntakeSeconds);
            return;
        }

        if (!trialStarted)
        {
            trialStarted = true;
            trialLighting.SetActive(true);
            monitor.text = "TRIAL ACTIVE";
            captionTitle.text = "The first cure trial";
            caption.text = "<b>Dr. Grey</b>  Ryder got us the samples we needed. Let's start the first trial.";
            continueButton.gameObject.SetActive(true);
        }

        float trialElapsed = elapsed - IntakeSeconds;
        FrameShot(trialStart, trialEnd, trialElapsed / TrialMoveSeconds);
        greyReach.SampleAnimation(greyRig, Mathf.Min(trialElapsed, greyReach.length));
        assistantReach.SampleAnimation(assistantRig, Mathf.Min(trialElapsed, assistantReach.length));
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ContinueToWinner();
        }
    }

    private void FrameShot(Transform start, Transform end, float progress)
    {
        float ease = Mathf.SmoothStep(0f, 1f, progress);
        view.transform.SetPositionAndRotation(
            Vector3.Lerp(start.position, end.position, ease),
            Quaternion.Slerp(start.rotation, end.rotation, ease));
    }

    private void ContinueToWinner()
    {
        if (!trialStarted || leaving)
        {
            return;
        }

        leaving = true;
        SceneManager.LoadScene("Winner");
    }
}
