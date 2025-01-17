using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] GameObject muzzleFlash;
    public float fireRate = 0.5f;  
    public float damage = 10f;    
    private float lastFiredTime; 

    public void Fire()
    {
        if (Time.time - lastFiredTime < fireRate) return;
        lastFiredTime = Time.time;

        StartCoroutine(FireCR());
    }

    IEnumerator FireCR()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        muzzleFlash.SetActive(false);   
    }
}
