using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] GameObject muzzleFlash;
    public float fireRate = 0.5f;  
    public float damage = 1f;    
    private float lastFiredTime; 

    public void Fire(IEnemyAttackable enemyAttackable)
    {
        if (Time.time - lastFiredTime < fireRate) return;
        lastFiredTime = Time.time;

        enemyAttackable.SetHealth(enemyAttackable.GetHealth() - damage);
        StartCoroutine(FireCR());
    }

    IEnumerator FireCR()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        muzzleFlash.SetActive(false);   
    }
}
