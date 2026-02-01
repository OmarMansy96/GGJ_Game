using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [SerializeField] private Slider healthSlider;

    private bool isDead = false;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    void Awake()
    {
        currentHealth = maxHealth;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateSlider();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateSlider();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void UpdateSlider()
    {
        healthSlider.value = currentHealth;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDied?.Invoke();
    }
}
