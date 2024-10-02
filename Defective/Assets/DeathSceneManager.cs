using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathSceneManager : MonoBehaviour
{

    [SerializeField] Transform checkpoint;
    [SerializeField] GameObject player;
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void Restart()
    {
        player.transform.position = checkpoint.transform.position;
    }
}
