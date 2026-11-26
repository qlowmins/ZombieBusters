using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coinDestroy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Coin touched by: " + other.gameObject.name
            + " | Tag: " + other.tag);

        if (other.CompareTag("player") ||
            other.transform.root.CompareTag("player"))
        {
            Debug.Log("COIN COLLECTED!");
            Destroy(gameObject);
        }
    }
}
