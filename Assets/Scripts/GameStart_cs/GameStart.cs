using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public bool isPlaying = false;

    public int randomRange;

    void Start()
    {
        Random_F();
    }

    void Update()
    {
        if (this.gameObject.activeSelf && !isPlaying)
        {
            StartCoroutine(Playing());
        }
    }

    IEnumerator Playing()
    {
        yield return new WaitForSeconds(1f);

        isPlaying = true;

        
    }

    void Random_F()
    {
        randomRange = Random.Range(0, 5);

        //Debug.Log(randomRange);
    }
}
