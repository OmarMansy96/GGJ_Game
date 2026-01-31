using System;
using UnityEngine;

public class Health: MonoBehaviour
{



    private int maxHealth = 100;
    private int currentHealth = 100;

    bool isDead = false;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;







    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
            Die();


    }


    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

    }


    public void Die()
    {


        if (isDead) return;
        isDead = true;
        OnDied?.Invoke();


    }








}
