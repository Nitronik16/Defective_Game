using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class monsterAI : MonoBehaviour
{
    [Range(0, 20)] public float speed;
    public bool isChasing;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isChasing)
        {
            transform.Translate(Vector3.right * speed);

        }
    }

}
