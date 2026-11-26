using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveBulletForward : MonoBehaviour
{
    Rigidbody bulletRigidBody;

    void Start()
    {
        bulletRigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bulletRigidBody.AddForce(-transform.up * 150);
        Destroy(gameObject, .5f);
    }

    private void OnCollisionEnter(Collision bul)
    {
        GameObject target = bul.gameObject;

        if (target.CompareTag("enemy") ||
            target.CompareTag("enemyBoss"))
        {
            // Only zombies and bosses can drop coins
            if (target.CompareTag("enemy") ||
                target.CompareTag("enemyBoss"))
            {
                coinDrop dropScript =
                    target.GetComponentInParent<coinDrop>();

                if (dropScript != null)
                {
                    // Spawn coin before zombie disappears
                    dropScript.DropCoin();
                }
                else
                {
                    Debug.LogWarning(
                        "No coinDrop script found on " + target.name
                    );
                }
            }

            // Destroy the object hit by the bullet
            Destroy(target);

            // Destroy the bullet
            Destroy(gameObject);
        }
    }
}