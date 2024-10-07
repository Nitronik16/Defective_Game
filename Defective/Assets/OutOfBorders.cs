using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutOfBorders : MonoBehaviour
{
    [SerializeField] GameObject deathScreen;
    [SerializeField] GameObject enemy;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.SetActive(false);
            enemy.gameObject.SetActive(false);
            deathScreen.SetActive(true);

        }
    }
}
