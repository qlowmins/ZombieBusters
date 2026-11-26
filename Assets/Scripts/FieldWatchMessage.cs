using UnityEngine;

public sealed class FieldWatchMessage : MonoBehaviour
{
    private const float DisplaySeconds = 10f;

    [SerializeField] private GameObject openingObjective;
    [SerializeField] private GameObject card;

    private bool displayed;
    private float secondsRemaining;

    private void Awake()
    {
        card.SetActive(false);
    }

    private void LateUpdate()
    {
        if (WavePause.BlocksGameplayInput)
        {
            card.SetActive(false);
            return;
        }

        if (!displayed)
        {
            if (openingObjective.activeInHierarchy)
            {
                return;
            }

            displayed = true;
            secondsRemaining = DisplaySeconds;
        }
        else
        {
            secondsRemaining -= Time.deltaTime;
        }

        bool visible = secondsRemaining > 0f;
        card.SetActive(visible);
        if (!visible)
        {
            enabled = false;
        }
    }
}
