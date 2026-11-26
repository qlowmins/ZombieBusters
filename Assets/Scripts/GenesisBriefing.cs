using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GenesisBriefing : MonoBehaviour
{
    private enum Subject { None, ResearchFile, Doctor, Exit }

    [SerializeField] private Camera view;
    [SerializeField] private WavePause pause;
    [SerializeField] private Collider researchFile;
    [SerializeField] private Collider doctor;
    [SerializeField] private Collider exit;
    [SerializeField] private TMP_Text exitStatus;
    [SerializeField] private Renderer exitIndicator;
    [SerializeField] private Material unlockedIndicator;
    [SerializeField] private TMP_Text checklist;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text prompt;
    [SerializeField] private GameObject reticle;
    [SerializeField] private GameObject briefingCard;
    [SerializeField] private TMP_Text cardTitle;
    [SerializeField] private TMP_Text cardBody;
    [SerializeField] private Button acknowledgeButton;
    [SerializeField, TextArea(4, 8)] private string researchNotes;
    [SerializeField, TextArea(4, 8)] private string missionBrief;

    private FirstPersonLook look;
    private FirstPersonMovement movement;
    private Rigidbody playerBody;
    private Subject cardSubject;
    private bool readFile;
    private bool spokeToDoctor;
    private bool leaving;

    private bool Briefed => readFile && spokeToDoctor;

    private void Awake()
    {
        look = view.GetComponent<FirstPersonLook>();
        movement = view.GetComponentInParent<FirstPersonMovement>();
        playerBody = movement.GetComponent<Rigidbody>();
        briefingCard.SetActive(false);
        acknowledgeButton.onClick.AddListener(Acknowledge);
        SetPrompt(string.Empty);
        UpdateChecklist();
    }

    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        if (cardSubject != Subject.None)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                Acknowledge();
            }
            return;
        }

        if (WavePause.BlocksGameplayInput || leaving)
        {
            SetPrompt(string.Empty);
            reticle.SetActive(false);
            return;
        }

        reticle.SetActive(true);
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void LateUpdate()
    {
        if (cardSubject == Subject.None && !WavePause.BlocksGameplayInput && !leaving)
        {
            SetPrompt(TargetInView() switch
            {
                Subject.ResearchFile => "[E]   Read the research file",
                Subject.Doctor => "[E]   Speak to Dr. Grey",
                Subject.Exit => Briefed ? "[E]   Deploy to Wave 1" : "Field exit locked. Complete the briefing.",
                _ => string.Empty
            });
        }
    }

    private Subject TargetInView()
    {
        const float reach = 3.3f;
        Ray aim = view.ViewportPointToRay(new Vector3(.5f, .5f));
        if (!Physics.Raycast(aim, out RaycastHit hit, reach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            return Subject.None;
        }

        if (hit.collider == researchFile) return Subject.ResearchFile;
        if (hit.collider == doctor) return Subject.Doctor;
        return hit.collider == exit ? Subject.Exit : Subject.None;
    }

    private void TryInteract()
    {
        Subject target = TargetInView();
        if (target == Subject.Exit && Briefed)
        {
            leaving = true;
            SceneManager.LoadScene("MainGame");
        }
        else if (target == Subject.ResearchFile || target == Subject.Doctor)
        {
            cardSubject = target;
            bool isFile = target == Subject.ResearchFile;
            cardTitle.text = isFile ? "Research file" : "Dr. Grey";
            cardBody.text = isFile ? researchNotes : missionBrief;
            briefingCard.SetActive(true);
            reticle.SetActive(false);
            SetPrompt(string.Empty);
            SetControlsEnabled(false);
            EventSystem.current.SetSelectedGameObject(acknowledgeButton.gameObject);
        }
    }

    private void Acknowledge()
    {
        if (cardSubject == Subject.None)
        {
            return;
        }

        readFile |= cardSubject == Subject.ResearchFile;
        spokeToDoctor |= cardSubject == Subject.Doctor;
        cardSubject = Subject.None;
        briefingCard.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        SetControlsEnabled(true);
        UpdateChecklist();
    }

    private void SetControlsEnabled(bool enabled)
    {
        pause.enabled = enabled;
        look.enabled = enabled;
        movement.enabled = enabled;
        playerBody.velocity = Vector3.zero;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    private void SetPrompt(string value)
    {
        prompt.text = value;
        promptPanel.SetActive(value.Length > 0);
    }

    private void UpdateChecklist()
    {
        if (Briefed)
        {
            checklist.text = "<color=#79E5C5>BRIEFING COMPLETE</color>\nFollow the floor stripe to the field exit.";
            exitStatus.text = "READY FOR DEPLOYMENT";
            exitStatus.color = new Color(.49f, .91f, .78f);
            exitIndicator.sharedMaterial = unlockedIndicator;
            return;
        }

        string file = readFile ? "<color=#79E5C5>DONE</color>   Research file" : "Read the research file at the left desk";
        string talk = spokeToDoctor ? "<color=#79E5C5>DONE</color>   Dr. Grey" : "Speak to Dr. Grey at the right station";
        checklist.text = $"{file}\n{talk}";
    }
}
