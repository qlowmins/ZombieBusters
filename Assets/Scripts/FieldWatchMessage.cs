using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class FieldWatchMessage : MonoBehaviour
{
    public const float RescueDisplaySeconds = 4f;

    private const float OpeningDisplaySeconds = 10f;
    private const float StoryDisplaySeconds = 6f;

    [SerializeField] private GameObject openingObjective;
    [SerializeField] private GameObject card;
    [SerializeField] private TMP_Text messageText;

    private readonly Queue<(string Text, float Duration)> pending = new Queue<(string, float)>();
    private string openingText;
    private bool showingOpening;
    private float secondsRemaining;

    private void Awake()
    {
        if (openingObjective != null)
        {
            openingText = messageText.text;
        }

        card.SetActive(false);
    }

    public void ShowFirstSample()
    {
        QueueStory("First sample confirmed, Ryder. Keep the canister sealed.", StoryDisplaySeconds);
    }

    public void ShowGoliathWarning()
    {
        QueueStory("Goliath ahead, Ryder. Back away when it winds up to strike.", StoryDisplaySeconds);
    }

    public void ShowRescueUpdate()
    {
        QueueStory("Widowmayer is down. The rescue team is at the extraction point.", RescueDisplaySeconds);
    }

    private void QueueStory(string message, float duration)
    {
        pending.Enqueue((message, duration));
        if (showingOpening)
        {
            secondsRemaining = 0f;
            showingOpening = false;
        }
    }

    private void LateUpdate()
    {
        if (WavePause.BlocksGameplayInput)
        {
            card.SetActive(false);
            return;
        }

        secondsRemaining -= Time.unscaledDeltaTime;
        if (secondsRemaining <= 0f)
        {
            showingOpening = false;
            if (pending.Count > 0)
            {
                (messageText.text, secondsRemaining) = pending.Dequeue();
            }
            else if (openingText != null && !openingObjective.activeInHierarchy)
            {
                messageText.text = openingText;
                openingText = null;
                showingOpening = true;
                secondsRemaining = OpeningDisplaySeconds;
            }
        }

        card.SetActive(secondsRemaining > 0f);
    }
}
