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

    public bool defaultActive = false;
    public BatteryType type = BatteryType.Continuous;

    public virtual void Start()
    {
        active = defaultActive;
    }

    public virtual void LateUpdate()
    {
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
        active = true;
    }

    public virtual void TogglePower()
    {
        active = !active;
    }

    public virtual void PowerOff()
    {
        active = false;
    }

    public bool GetActive()
    {
        return active;
    }
}
