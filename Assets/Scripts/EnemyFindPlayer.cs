using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFindPlayer : MonoBehaviour
{
    public EnemyAI enemyAI;
    public AudioSource audiosource;
    public AudioClip clip;

    void Start()
    {
        if (clip != null)
        {
            audiosource.clip = clip;
            audiosource.loop = true;
            audiosource.playOnAwake = false;
            audiosource.volume = 1f;
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
