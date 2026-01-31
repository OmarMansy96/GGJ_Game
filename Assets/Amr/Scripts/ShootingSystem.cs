using UnityEngine;
using System.Collections;
public class ShootingSystem : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float bulletLifeTime = 5f;

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

        if (muzzleFlash != null)
            muzzleFlash.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }
    }

    void Shoot()
    {
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

            Destroy(bullet, bulletLifeTime);

            if (muzzleFlash != null)
            {
                muzzleFlash.gameObject.SetActive(true);
                muzzleFlash.Play();
                StartCoroutine(DisableMuzzleFlash());
            }

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

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    IEnumerator DisableMuzzleFlash()
    {
        yield return new WaitForSeconds(0.1f);

        if (muzzleFlash != null)
            muzzleFlash.gameObject.SetActive(false);
    }
}