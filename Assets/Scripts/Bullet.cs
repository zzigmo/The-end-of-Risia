using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletLifetime = 0.5f;

    void Start()
    {
            Destroy(gameObject, bulletLifetime); // ������� ���� ����� ������������ �����
    }

    // ��������� ������������
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall") // ��������� ��� �����
        {
            Destroy(gameObject);  // ���������� ����
        }
    }
}
