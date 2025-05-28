using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindPlayer : MonoBehaviour
{
    public EnemyAI enemyAI;
    public AudioSource audiosource;
    public AudioClip clip;

    void Start()
    {
        enemyAI = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyAI>();

        if (clip != null)
        {
            audiosource.clip = clip;
            audiosource.loop = true;
            audiosource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (enemyAI.isChasing && !audiosource.isPlaying)
        {
            audiosource.Play();
        }
        else if (!enemyAI.isChasing && audiosource.isPlaying)
        {
            audiosource.Stop();
        }
    }

}
