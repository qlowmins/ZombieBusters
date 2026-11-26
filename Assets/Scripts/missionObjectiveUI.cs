using UnityEngine;
using System.Collections;

public class MissionObjectiveUI : MonoBehaviour
{
    public float displayDuration = 5f;

    private IEnumerator Start()
    {
        // Display the mission objective at the start
        yield return new WaitForSeconds(displayDuration);

        // Hide the mission objective
        gameObject.SetActive(false);
    }
}