using UnityEngine;

public class EnemyDummy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int goldDrop = 10;

    private int _currentHealth;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        Debug.Log(string.Format("[EnemyDummy] {0} took {1} damage. HP: {2}/{3}",
            gameObject.name, damage, _currentHealth, maxHealth));

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(string.Format("[EnemyDummy] {0} died. Dropped {1} gold.", gameObject.name, goldDrop));

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddGold(goldDrop);

        Destroy(gameObject);
    }
}
