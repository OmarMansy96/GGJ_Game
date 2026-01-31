using UnityEngine;

public class Mask : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] private int maxShield = 50;
    private int currentShield;

    private void Awake()
    {
        currentShield = maxShield;
    }

    public void Equip()
    {
        currentShield = maxShield;
        gameObject.SetActive(true);
    }

    public void Unequip()
    {
        gameObject.SetActive(false);
    }

    public bool TakeDamage(ref int damage)
    {
        if (currentShield <= 0)
            return false;

        int absorbed = Mathf.Min(currentShield, damage);
        currentShield -= absorbed;
        damage -= absorbed;

        if (currentShield <= 0)
            OnShieldBroken();

        return true;
    }

    void OnShieldBroken()
    {
        Unequip();
    }

    public int GetShield()
    {
        return currentShield;
    }
}
