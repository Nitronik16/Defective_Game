using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathSceneManager : MonoBehaviour
{

    [SerializeField] Transform checkpoint;
    [SerializeField] Transform enemyRestartPoint;
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;
    [SerializeField] GameObject[] obstacles;
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        player.transform.position = checkpoint.transform.position;
        enemy.transform.position = enemyRestartPoint.transform.position;   
        enemy.gameObject.SetActive(false);
        for (int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].gameObject.SetActive(true);
        }
    }
}
