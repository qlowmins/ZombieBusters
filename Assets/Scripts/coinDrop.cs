using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coinDrop : MonoBehaviour
{
    public GameObject PirateCoin;

    public void DropCoin()
    {
        if (PirateCoin == null)
        {
            Debug.LogWarning("PirateCoin is not assigned!");
            return;
        }

        Vector3 position = transform.position;

        GameObject coin = Instantiate(
            PirateCoin,
            position + new Vector3(0f, 2f, 0f),
            PirateCoin.transform.rotation
        );

        coin.SetActive(true);

        //Destroy(coin, 5f);
    }
}