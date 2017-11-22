using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveDialog : MonoBehaviour {

    public string text;
    public Font font;
    public Texture2D background;

    private float animPercent;
    private Color textColor;
    private GUIStyle style;

    private bool guiInitialized = false;

    void GUIStart()
    {
        style = GUI.skin.box;

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
        animPercent = Mathf.Max(0f, animPercent - 0.05f);
        if (distance < 3f)
        {
            animPercent = Mathf.Min(1f, animPercent + 0.1f);
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
            GUI.Label(new Rect(pos.x - (0.5f* scaledRegionSize.x), Screen.height - pos.y - scaledRegionSize.y, scaledRegionSize.x, scaledRegionSize.y), textContent, style);
        }
    }
}
