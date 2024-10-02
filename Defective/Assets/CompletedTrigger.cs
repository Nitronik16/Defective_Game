using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletedTrigger : MonoBehaviour
{
    [SerializeField] GameObject ventOpened;
    [SerializeField] GameObject ventClosed;
    [SerializeField] GameObject enemy;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ventClosed.SetActive(true);
            ventOpened.SetActive(false);
            Destroy(enemy);
        }
    }
}
