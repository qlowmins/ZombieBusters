using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CureEpilogue : MonoBehaviour
{
    private enum Phase { GreyRadio, RyderRadio, Intake, Trial }

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

    private const float RadioLineSeconds = 3f;
    private const float RadioSeconds = 2f * RadioLineSeconds;
    private const float IntakeSeconds = 5.5f;
    private const float TrialMoveSeconds = 7f;
    private string intakeTitle;
    private string intakeCaption;
    private float startedAt;
    private Phase phase;
    private bool leaving;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        startedAt = Time.unscaledTime;
        intakeTitle = captionTitle.text;
        intakeCaption = caption.text;
        captionTitle.text = "Extraction radio";
        caption.text = "<b>Dr. Grey</b>  Ryder, the rescue team has reached the extraction point.";
        continueButton.gameObject.SetActive(false);
        continueButton.onClick.AddListener(ContinueToWinner);
        trialLighting.SetActive(false);
        FrameShot(intakeStart, intakeEnd, 0f);
    }

    private void Update()
    {
        float elapsed = Time.unscaledTime - startedAt;
        if (elapsed < RadioSeconds)
        {
            if (phase == Phase.GreyRadio && elapsed >= RadioLineSeconds)
            {
                phase = Phase.RyderRadio;
                caption.text = "<b>Ryder</b>  Copy, Dr. Grey. I have the DNA samples.";
            }
            return;
        }

        float labElapsed = elapsed - RadioSeconds;
        if (labElapsed < IntakeSeconds)
        {
            if (phase != Phase.Intake)
            {
                phase = Phase.Intake;
                captionTitle.text = intakeTitle;
                caption.text = intakeCaption;
            }
            FrameShot(intakeStart, intakeEnd, labElapsed / IntakeSeconds);
            return;
        }

        if (phase != Phase.Trial)
        {
            phase = Phase.Trial;
            trialLighting.SetActive(true);
            monitor.text = "TRIAL ACTIVE";
            captionTitle.text = "The first cure trial";
            caption.text = "<b>Dr. Grey</b>  Ryder got us the samples we needed. Let's start the first trial.";
            continueButton.gameObject.SetActive(true);
        }

        float trialElapsed = labElapsed - IntakeSeconds;
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
        if (phase != Phase.Trial || leaving)
        {
            return;
        }

        leaving = true;
        SceneManager.LoadScene("Winner");
    }
}
