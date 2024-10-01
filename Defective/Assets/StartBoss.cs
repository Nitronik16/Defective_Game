using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBoss : MonoBehaviour
{
    [SerializeField] GameObject monster;
 
 

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            monster.GetComponent<monsterAI>().isChasing = true;
            monster.gameObject.SetActive(true);    
        }
    }
}
