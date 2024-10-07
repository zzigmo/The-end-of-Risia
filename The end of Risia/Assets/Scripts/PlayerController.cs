using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Скорость движения
    public Rigidbody2D rb; // Rigidbody2D для физики
    public Camera mainCamera; // Камера для определения положения курсора

    public Transform upperBody; // Верхняя часть тела
    public Transform lowerBody; // Нижняя часть тела

    public GameObject bulletPrefab; // Префаб пули
    public Transform firePoint; // Точка вылета пули
    public float bulletSpeed = 10f; // Скорость пули
    public int numberOfBullets = 5; // Количество пуль (дробинки)
    public float spreadAngle = 15f; // Угол разброса дробинок
    public float bulletLifetime = 0.5f; // Время жизни пуль

    public AudioClip shootSound; // Звук выстрела
    public AudioClip reloadSound; // Звук перезарядки
    public AudioClip emptyMagSound; // Звук при пустом магазине
    private AudioSource audioSource; // Источник звука

    public Animator upperBodyAnimator; // Аниматор для верхней части
    public Animator lowerBodyAnimator; // Аниматор для нижней части

    private Vector2 movement; // Направление движения
    private int ammo = 2; // Боезапас
    private bool isReloading = false; // Флаг перезарядки

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Получаем компонент AudioSource
    }

    void Update()
    {
        HandleMovement(); // Обработка передвижения
        HandleShooting(); // Обработка стрельбы
        HandleReload(); // Обработка перезарядки
    }

    // Функция для обработки передвижения персонажа
    void HandleMovement()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized; // Нормализуем, чтобы не было превышения скорости по диагонали

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        // Поворот нижней части в сторону движения
        if (movement.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            lowerBody.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            lowerBodyAnimator.SetBool("isMoving", true); // Включаем анимацию движения
        }
        else
        {
            lowerBodyAnimator.SetBool("isMoving", false); // Анимация Idle
        }

        // Поворот верхней части к курсору
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDirection = new Vector2(mousePos.x - upperBody.position.x, mousePos.y - upperBody.position.y);
        float upperBodyAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        upperBody.rotation = Quaternion.Euler(new Vector3(0, 0, upperBodyAngle - 90f));
    }

    // Функция для стрельбы
    void HandleShooting()
    {
        if (Input.GetButtonDown("Fire1") && !isReloading) // Проверка на выстрел
        {
            if (ammo > 0) // Если есть патроны
            {
                Shoot();
                upperBodyAnimator.SetTrigger("Shoot"); // Проигрываем анимацию стрельбы
                ammo--;
            }
            else
            {
                // Если патронов нет, проигрываем звук пустого магазина
                audioSource.PlayOneShot(emptyMagSound);
            }
        }
    }

    // Функция стрельбы дробью
    void Shoot()
    {
        // Проигрываем звук выстрела
        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = Random.Range(-spreadAngle, spreadAngle); // Разброс угла
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
            Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
            if (rbBullet != null)
            {
                rbBullet.velocity = bulletRotation * Vector2.up * bulletSpeed;
                Destroy(bullet, bulletLifetime); // Удаляем пулю через определенное время
            }
        }
    }

    // Функция перезарядки
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
        upperBodyAnimator.SetTrigger("Reload"); // Включаем анимацию перезарядки
        audioSource.PlayOneShot(reloadSound); // Проигрываем звук перезарядки
        yield return new WaitForSeconds(0.3f); // Ждем 0.3 секунды анимации
        ammo = 2; // Восстанавливаем боезапас
        isReloading = false;
    }
}
