using UnityEngine;
using System.Collections;
public class ShootingSystem : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    [Header("Ammo Settings")]
    public int maxAmmo = 2;
    private int currentAmmo;
    private bool isReloading = false;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip emptySound;
    public AudioClip reloadSound;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        // Shoot
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }
    }

    void Shoot()
    {
        // Block shooting while reloading
        if (isReloading) return;

        if (currentAmmo > 0)
        {
            currentAmmo--;

            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);

            if (muzzleFlash != null)
                muzzleFlash.Play();

            audioSource.PlayOneShot(shootSound);
        }
        else
        {
            audioSource.PlayOneShot(emptySound);
        }
    }

    void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo == maxAmmo) return;

        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(1f);

        // Shoot sound only (no bullet, no effect)
        audioSource.PlayOneShot(shootSound);

        currentAmmo = maxAmmo;
        isReloading = false;
    }
}