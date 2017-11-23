using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog : MonoBehaviour {

    public string text = "DEFAULT DIALOGUE";
    public Font font;
    public Texture2D background;
    public bool passive = false;
    public bool stayOnScreen = false;

    [HideInInspector] public float animPercent = 0f;
    private Color textColor;
    private GUIStyle style;

    private float animRate = 0.1f;

    private bool guiInitialized = false;

    void GUIStart()
    {
        if (font == null)
            font = Resources.Load<Font>("BRITANIC");
        if (background == null)
            background = Resources.Load<Texture2D>("Textures/tex_dialogBubble");


        style = GUI.skin.box;//new GUIStyle(GUI.skin.box);

        style.alignment = TextAnchor.LowerCenter;
        style.wordWrap = true;
        style.font = font;
        textColor = Color.black;
        style.normal.background = background;
    }

    void OnGUI()
    {
        if (!guiInitialized)
            GUIStart();

        GUI.enabled = true;
        Camera cam = Camera.current;
        Transform playerTrans = GameManager.player.transform;
        float distance = Vector3.Distance(transform.position, playerTrans.position);

        // Handle animation percent
        if (passive)
        {
            animPercent = Mathf.Max(0f, animPercent - animRate);
            if (distance < 3f && !text.Equals(""))
            {
                animPercent = Mathf.Min(1f, animPercent + animRate*2f);
            }
        }

        // Adjust the style accordingly
        textColor.a = animPercent;
        style.normal.textColor = textColor;

        // Display the bubble
        if  (cam != null  &&  animPercent > 0f)
        {
            Vector3 worldPos = new Vector3(transform.position.x, transform.position.y + Mathf.Lerp(1f, 1.5f, animPercent), transform.position.z);
            Vector3 pos = cam.WorldToScreenPoint(worldPos);

            Vector2 textRegionSize = new Vector2(200, 1);
            GUIContent textContent = new GUIContent(text);
            textRegionSize.y = style.CalcHeight(textContent, textRegionSize.x);

            if  (textRegionSize.y > 140)
            {
                textRegionSize.x = 300;
                textRegionSize.y = style.CalcHeight(textContent, textRegionSize.x);
            }

            
            style.fontSize = Mathf.RoundToInt(Mathf.Lerp(1, 24, animPercent));

            Vector2 scaledRegionSize = new Vector2(Mathf.Lerp(1, textRegionSize.x, animPercent), Mathf.Lerp(1, textRegionSize.y, animPercent));
            if (stayOnScreen)
            {
                pos = new Vector3(Mathf.Clamp(pos.x, 0f, Screen.width-scaledRegionSize.x), Mathf.Clamp(pos.y, 0f, Screen.height - scaledRegionSize.y), pos.z);
            }
            Rect boxRect = new Rect(pos.x - (0.5f * scaledRegionSize.x), Screen.height - pos.y - scaledRegionSize.y, scaledRegionSize.x, scaledRegionSize.y);
            GUI.Label(boxRect, textContent, style);

            if (animPercent == 1 && !passive)
            {
                GUI.DrawTexture(new Rect(boxRect.right - Screen.width * 0.015f, 
                                         boxRect.bottom - Screen.width * 0.015f, 
                                         Screen.width * 0.03f, 
                                         Screen.width * 0.03f), 
                                GameManager.talkButtonTex);
            }

        }
    }


    public IEnumerator Open()
    {
        while(animPercent < 1f)
        {
            animPercent += animRate;
            yield return null;
        }
        animPercent = 1f;
    }

    public IEnumerator Close()
    {
        while (animPercent > 0f)
        {
            animPercent -= animRate;
            yield return null;
        }
        animPercent = 0f;
    }

    public IEnumerator ChangeText(string newText)
    {
        print("Changing text to " + newText);

        // If open, first close
        if (animPercent > 0f)
        {
            yield return StartCoroutine(Close());
        }

        // Change text
        text = newText;

        // Reopen
        yield return StartCoroutine(Open());
    }
}
