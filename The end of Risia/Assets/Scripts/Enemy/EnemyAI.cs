using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Для работы с 2D светом

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    public float detectionRadius = 5f; // Радиус обнаружения
    public float chaseSpeedMultiplier = 1.5f;
    public LayerMask playerLayer;
    public Transform upperBody;
    public GameObject corpsePrefab;
    public Animator animator;

    private int currentPointIndex = 0;
    private Transform player;
    private bool isChasing = false;
    private bool returningToPatrol = false;
    private Rigidbody2D rb;
    public float disableDuration = 1f;
    public int health = 1;
    public float rotationSpeed = 5f;
    private bool playerInSight = false;

    private Light2D light2D; // Компонент 2D света

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        light2D = GetComponentInChildren<Light2D>(); // Получаем компонент 2D света
    }

    private void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
        }
        else if (returningToPatrol)
        {
            ReturnToPatrol();
        }
        else
        {
            Patrol();
        }

        DetectPlayer();
        HandleMovementAnimation();
    }

    private void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];

        MoveTowards(targetPoint.position, moveSpeed);
        SmoothRotateTowards(targetPoint.position);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void DetectPlayer()
    {
        // Если игрок в радиусе действия света, начинаем преследование
        if (PlayerInLight())
        {
            playerInSight = true;
            if (!isChasing)
            {
                isChasing = true;
                moveSpeed *= chaseSpeedMultiplier;  // Увеличиваем скорость при преследовании
            }
        }
        else if (PlayerInHearingRange() && !playerInSight)
        {
            // Повернуться к игроку, если он был услышан
            FaceTowards(player.position);
        }
        else if (isChasing)
        {
            playerInSight = false;
            returningToPatrol = true;
            isChasing = false;
            moveSpeed /= chaseSpeedMultiplier;
        }
    }

    private bool PlayerInLight()
    {
        if (player == null) return false;

        // Проверка, находится ли игрок в радиусе действия света
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= light2D.pointLightOuterRadius) // Проверяем, находится ли игрок в радиусе света
        {
            // Проверка на наличие препятствий между врагом и игроком
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, playerLayer);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                FaceTowards(player.position); // Вращаемся в сторону игрока
                return true;
            }
        }

        return false;
    }

    private bool PlayerInHearingRange()
    {
        return player != null && Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        // Враг должен продолжать преследование, даже если игрок находится не сверху или снизу
        MoveTowards(player.position, moveSpeed);
        FaceTowards(player.position);  // Всегда поворачиваем врага к игроку
    }

    private void ReturnToPatrol()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];

        MoveTowards(targetPoint.position, moveSpeed);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            returningToPatrol = false;
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    // Плавное вращение в сторону цели
    private void SmoothRotateTowards(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Вращение врага в сторону игрока
    private void FaceTowards(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void HandleMovementAnimation()
    {
        bool isMoving = rb.velocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 offset = new Vector3(0, 0, 0);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + offset, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (transform.up * light2D.pointLightOuterRadius));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage();
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DisableMovement());
        }
    }

    private void TakeDamage()
    {
        health--;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private IEnumerator DisableMovement()
    {
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(disableDuration);
    }
}
