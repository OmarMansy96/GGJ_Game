using System;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public int MaxShield { get; private set; }
    public int CurrentShield { get; private set; }

    public event Action<int, int> OnShieldChanged;
    public event Action OnShieldBroken;

    bool brokenFired;

    public void SetMaxAndRefill(int max)
    {
        MaxShield = Mathf.Max(0, max);
        CurrentShield = MaxShield;
        brokenFired = false;
        OnShieldChanged?.Invoke(CurrentShield, MaxShield);
    }

    public int Absorb(int damage)
    {
        if (damage <= 0) return 0;
        if (CurrentShield <= 0) return damage;

        int before = CurrentShield;
        CurrentShield = Mathf.Max(0, CurrentShield - damage);
        OnShieldChanged?.Invoke(CurrentShield, MaxShield);

        int absorbed = before - CurrentShield;
        int leftover = damage - absorbed;

        if (CurrentShield == 0 && !brokenFired)
        {
            brokenFired = true;
            OnShieldBroken?.Invoke();
        }

        return leftover;
    }
}
