using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class monsterAI : MonoBehaviour
{
    [Range(0, 20)] public float speed;
    

    // Update is called once per frame
    void Update()
    {

        transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
        
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(3);
        }
        if (col.gameObject.CompareTag("Obstacle"))
        {
            Destroy(col.gameObject);
        }
    }

   

}
