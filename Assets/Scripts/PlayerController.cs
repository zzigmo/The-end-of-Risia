using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // �������� ��������
    public Rigidbody2D rb; // Rigidbody2D ��� ������
    public Camera mainCamera; // ������ ��� ����������� ��������� �������

    public Transform upperBody; // ������� ����� ����
    public Transform lowerBody; // ������ ����� ����

    public GameObject bulletPrefab; // ������ ����
    public Transform firePoint; // ����� ������ ����
    public float bulletSpeed = 10f; // �������� ����
    public int numberOfBullets = 5; // ���������� ���� (��������)
    public float spreadAngle = 15f; // ���� �������� ��������
    public float bulletLifetime = 0.5f; // ����� ����� ����

    public AudioClip shootSound; // ���� ��������
    public AudioClip reloadSound; // ���� �����������
    public AudioClip emptyMagSound; // ���� ��� ������ ��������
    private AudioSource audioSource; // �������� �����

    public Animator upperBodyAnimator; // �������� ��� ������� �����
    public Animator lowerBodyAnimator; // �������� ��� ������ �����

    private Vector2 movement; // ����������� ��������
    private int ammo = 2; // ��������
    private bool isReloading = false; // ���� �����������

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // �������� ��������� AudioSource
    }

    void Update()
    {
        HandleMovement(); // ��������� ������������
        HandleShooting(); // ��������� ��������
        HandleReload(); // ��������� �����������
    }

    // ������� ��� ��������� ������������ ���������
    void HandleMovement()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized; // �����������, ����� �� ���� ���������� �������� �� ���������

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        // ������� ������ ����� � ������� ��������
        if (movement.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            lowerBody.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            lowerBodyAnimator.SetBool("isMoving", true); // �������� �������� ��������
        }
        else
        {
            lowerBodyAnimator.SetBool("isMoving", false); // �������� Idle
        }

        // ������� ������� ����� � �������
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDirection = new Vector2(mousePos.x - upperBody.position.x, mousePos.y - upperBody.position.y);
        float upperBodyAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        upperBody.rotation = Quaternion.Euler(new Vector3(0, 0, upperBodyAngle - 90f));
    }

    // ������� ��� ��������
    void HandleShooting()
    {
        if (Input.GetButtonDown("Fire1") && !isReloading) // �������� �� �������
        {
            if (ammo > 0) // ���� ���� �������
            {
                Shoot();
                upperBodyAnimator.SetTrigger("Shoot"); // ����������� �������� ��������
                ammo--;
            }
            else
            {
                // ���� �������� ���, ����������� ���� ������� ��������
                audioSource.PlayOneShot(emptyMagSound);
            }
        }
    }

    // ������� �������� ������
    void Shoot()
    {
        // ����������� ���� ��������
        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = Random.Range(-spreadAngle, spreadAngle); // ������� ����
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
            Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
            if (rbBullet != null)
            {
                rbBullet.velocity = bulletRotation * Vector2.up * bulletSpeed;
                Destroy(bullet, bulletLifetime); // ������� ���� ����� ������������ �����
            }
        }
    }

    // ������� �����������
    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        upperBodyAnimator.SetTrigger("Reload"); // �������� �������� �����������
        audioSource.PlayOneShot(reloadSound); // ����������� ���� �����������
        yield return new WaitForSeconds(0.3f); // ���� 0.3 ������� ��������
        ammo = 2; // ��������������� ��������
        isReloading = false;
    }
}
