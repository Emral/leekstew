using UnityEngine;
using System.Collections;

public class Battery : MonoBehaviour
{
    public enum BatteryType
    {
        Toggleable,
        Continuous
    }

    private bool active;

    public bool defaultActive = false;
    public BatteryType type = BatteryType.Continuous;

    private void Start()
    {
        active = defaultActive;
    }

    private void LateUpdate()
    {
        if (type == BatteryType.Continuous)
        {
            active = defaultActive;
        }
    }

    public void PowerOn()
    {
        active = true;
    }

    public void TogglePower()
    {
        active = !active;
    }

    public void PowerOff()
    {
        active = false;
    }

    public bool GetActive()
    {
        return active;
    }
}
