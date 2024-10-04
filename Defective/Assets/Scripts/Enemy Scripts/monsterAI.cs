using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class monsterAI : MonoBehaviour
{
    [Range(0, 20)] public float speed;
    [SerializeField] GameObject deathScreen;

    // Update is called once per frame
    void Update()
    {

        transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
        
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.SetActive(false);
            deathScreen.SetActive(true);

        }
        if (col.gameObject.CompareTag("Obstacle"))
        {
            col.gameObject.SetActive(false);
        }
    }

   

}
