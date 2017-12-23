using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupCollectReward : MonoBehaviour
{
    public GameObject[] rewardObjs;
    private bool activated;

    // Update is called once per frame
    void OnTransformChildrenChanged()
    {
		if (Application.isPlaying && transform.childCount == 0 && !activated)
        {
            activated = true;
            foreach (GameObject prefab in rewardObjs)
            {
                GameObject obj = GameObject.Instantiate(prefab, GameManager.player.transform.position, Quaternion.identity);
                UnityStandardAssets.Utility.FollowTarget follow = obj.GetComponent<UnityStandardAssets.Utility.FollowTarget>();
                if (follow != null)
                {
                    follow.target = GameManager.player.transform;
                }
            }
        }
	}
}
