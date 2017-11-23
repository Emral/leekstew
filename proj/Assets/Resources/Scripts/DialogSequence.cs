using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSequence : MonoBehaviour
{
    [ReorderableList] public string[] lines;

    private Dialog dialogScr;



    void Update()
    {
        if (dialogScr == null)
        {
            dialogScr = gameObject.GetComponent<Dialog>();
            if (dialogScr == null)
                dialogScr = gameObject.GetComponentInChildren<Dialog>();
            if (dialogScr == null)
                dialogScr = gameObject.AddComponent<Dialog>();
        }
    }


    public IEnumerator RunSequence()
    {
        if (dialogScr != null && lines.Length > 0)
        {
            foreach (string line in lines)
            {
                print("starting line: " + line);

                // Change the dialogue
                yield return dialogScr.StartCoroutine(dialogScr.ChangeText(line));
                //yield return new WaitForSeconds(0.5f);

                // Wait for the player to press the button
                while (!GameManager.inputPress["Run"])
                {
                    yield return null;
                }
            }
            yield return dialogScr.StartCoroutine(dialogScr.Close());
        }
    }
}
