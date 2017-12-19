using UnityEngine;
using System.Collections;


public enum BoolChange {On, Off, Toggle};


[System.Serializable]
public class BatteryCharge
{
    public Battery target;
    public BoolChange signal;
}


public class Battery : MonoBehaviour
{
    public enum BatteryType
    {
        Toggleable,
        Continuous
    }

    private bool active;
    private bool wasActive = false;

    public bool defaultActive = false;
    public BatteryType type = BatteryType.Continuous;
    public AudioClip powerOnSound;
    public AudioClip powerOffSound;

    public virtual void Start()
    {
        active = defaultActive;
    }

    public virtual void LateUpdate()
    {
        wasActive = active;
        if (type == BatteryType.Continuous)
        {
            active = defaultActive;
        }
    }

    public virtual void ReceiveCharge(BoolChange signal)
    {
        if (signal == BoolChange.On)
            PowerOn();
        else if (signal == BoolChange.Off)
            PowerOff();
        else
            TogglePower();
    }

    public virtual void PowerOn()
    {
        if (!active && !wasActive)
        {
            AudioManager.PlaySound(powerOnSound);
        }
        active = true;
    }

    public virtual void TogglePower()
    {
        if (!active && !wasActive)
        {
            AudioManager.PlaySound(powerOnSound);
        }
        if (active && wasActive)
        {
            AudioManager.PlaySound(powerOffSound);
        }
        active = !active;
    }

    public virtual void PowerOff()
    {
        if (active && wasActive)
        {
            AudioManager.PlaySound(powerOffSound);
        }
        active = false;
    }

    public bool GetActive()
    {
        return active;
    }
}
