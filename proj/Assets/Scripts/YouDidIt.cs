using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YouDidIt : MonoBehaviour
{
    [ReorderableList] public AudioClip[] fixedYouDidIts;
    [ReorderableList] public AudioClip[] randomYouDidIts;

    // Use this for initialization
    void Start ()
    {
        GameManager.youDidIt++;
        AudioSource aud = UIManager.instance.canvasObj.GetComponent<AudioSource>();

        AudioClip randYouDidIt;
        if (GameManager.youDidIt > fixedYouDidIts.Length-1)
            randYouDidIt = randomYouDidIts[(int)Random.Range(0, randomYouDidIts.Length)];
        else
            randYouDidIt = fixedYouDidIts[GameManager.youDidIt];

        aud.PlayOneShot(randYouDidIt);
	}
}
