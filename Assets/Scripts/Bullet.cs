using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletLifetime = 0.5f;

    void Start()
    {
            Destroy(gameObject, bulletLifetime); // Удаляем пулю через определенное время
    }

    // Обработка столкновений
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall") // Проверяем тэг стены
        {
            Destroy(gameObject);  // Уничтожаем пулю
        }
    }
}
