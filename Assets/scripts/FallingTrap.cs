using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    [Header("�����]�w")]
    public int damage = 20;                    // �y�����ˮ`
    public float detectionRange = 2f;          // �����d��]���a�b�U��h���|Ĳ�o�^
    public float fallSpeed = 10f;              // ���U�t��
    public float resetTime = 3f;               // ���m�ɶ��]��^
    public bool canReset = true;               // �O�_�i�H���m

    [Header("���ĳ]�w�]�i��^")]
    public AudioClip triggerSound;             // Ĳ�o����
    public AudioClip impactSound;              // ��������

    [Header("�����]�w")]
    public LayerMask playerLayer = -1;         // ���a�ϼh�]-1��ܩҦ��ϼh�^
    public bool showDebugRays = true;          // �O�_��ܰ����g�u

    private Vector3 originalPosition;          // ��l��m
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isTriggered = false;          // �O�_�wĲ�o
    private bool isFalling = false;            // �O�_���b���U
    private bool hasHitPlayer = false;         // �O�_�w�g���쪱�a�]�קK���ƶˮ`�^

    // �ե�ޥ�
    private SpriteRenderer spriteRenderer;
    private Collider2D trapCollider;

    void Start()
    {
        // �O����l��m
        originalPosition = transform.position;

        // ����ե�
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trapCollider = GetComponent<Collider2D>();

        // �]�w Rigidbody2D �� Kinematic�]�������O�v�T�A����Ĳ�o�^
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.isKinematic = true;
        rb.gravityScale = 0;

        // �T�O���I����
        if (trapCollider == null)
        {
            trapCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // �]�w��Ĳ�o���A�o�ˤ~�఻�����a
        trapCollider.isTrigger = false; // ���U�ɻݭn���z�I��
    }

    void Update()
    {
        // �p�G�٨SĲ�o�B���b���U���A�A�˴����a
        if (!isTriggered && !isFalling)
        {
            DetectPlayer();
        }
    }

    /// <summary>
    /// �������a�O�_�b�U��
    /// </summary>
    void DetectPlayer()
    {
        // ��k1�G�ϥήg�u�˴�
        Vector2 rayStart = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, detectionRange, playerLayer);

        // ��k2�G�ϥνd���˴��]��i�a�^
        Collider2D[] playersInRange = Physics2D.OverlapBoxAll(
            new Vector2(transform.position.x, transform.position.y - detectionRange / 2),
            new Vector2(1f, detectionRange),
            0f,
            playerLayer
        );

        bool playerDetected = false;

        // �ˬd�g�u�˴����G
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (hit.transform.position.y < transform.position.y)
            {
                playerDetected = true;
                Debug.Log($"[�g�u�˴�] �����쪱�a: {hit.collider.name}");
            }
        }

        // �ˬd�d���˴����G
        foreach (Collider2D col in playersInRange)
        {
            if (col.CompareTag("Player") && col.transform.position.y < transform.position.y)
            {
                playerDetected = true;
                Debug.Log($"[�d���˴�] �����쪱�a: {col.name}");
                break;
            }
        }

        if (playerDetected)
        {
            TriggerTrap();
        }

        // ��ܰ����d��]Debug�Ρ^
        if (showDebugRays)
        {
            Debug.DrawRay(rayStart, Vector2.down * detectionRange, Color.red);
            // ��ܰ������l���|����
            Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - detectionRange / 2);
            Vector2 boxSize = new Vector2(1f, detectionRange);

            Vector2 topLeft = boxCenter + new Vector2(-boxSize.x / 2, boxSize.y / 2);
            Vector2 topRight = boxCenter + new Vector2(boxSize.x / 2, boxSize.y / 2);
            Vector2 bottomLeft = boxCenter + new Vector2(-boxSize.x / 2, -boxSize.y / 2);
            Vector2 bottomRight = boxCenter + new Vector2(boxSize.x / 2, -boxSize.y / 2);

            Debug.DrawLine(topLeft, topRight, Color.green);
            Debug.DrawLine(topRight, bottomRight, Color.green);
            Debug.DrawLine(bottomRight, bottomLeft, Color.green);
            Debug.DrawLine(bottomLeft, topLeft, Color.green);
        }
    }

    /// <summary>
    /// Ĳ�o����
    /// </summary>
    void TriggerTrap()
    {
        if (isTriggered) return;

        isTriggered = true;
        isFalling = true;
        hasHitPlayer = false;

        // ����Ĳ�o����
        if (audioSource && triggerSound)
        {
            audioSource.PlayOneShot(triggerSound);
        }

        // �ҥΪ��z�t���������U
        rb.isKinematic = false;
        rb.gravityScale = 1;

        // ������l�V�U�t��
        rb.velocity = new Vector2(0, -fallSpeed);

        Debug.Log($"{gameObject.name} �����QĲ�o�I");
    }

    /// <summary>
    /// �I���˴�
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // �p�G���쪱�a�B�٨S�y���ˮ`
        if (collision.gameObject.CompareTag("Player") && !hasHitPlayer && isFalling)
        {
            hasHitPlayer = true;

            // �缾�a�y���ˮ`
            PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"{gameObject.name} �缾�a�y�� {damage} �I�ˮ`�I");
            }

            // ����������
            if (audioSource && impactSound)
            {
                audioSource.PlayOneShot(impactSound);
            }

            // �����
            StopTrap();
        }
        // �p�G����a���Ψ�L����
        else if (!collision.gameObject.CompareTag("Player"))
        {
            // �����
            StopTrap();
        }
    }

    /// <summary>
    /// ���������
    /// </summary>
    void StopTrap()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }

        isFalling = false;

        // �p�G�i�H���m�A�}�l���m�p��
        if (canReset)
        {
            StartCoroutine(ResetTrap());
        }
    }

    /// <summary>
    /// ���m�������l��m
    /// </summary>
    IEnumerator ResetTrap()
    {
        yield return new WaitForSeconds(resetTime);

        // ���m��m�M���A
        transform.position = originalPosition;
        isTriggered = false;
        isFalling = false;
        hasHitPlayer = false;

        // �T�O���z�]�w���T
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
        }

        Debug.Log($"{gameObject.name} �����w���m");
    }

    /// <summary>
    /// ���Ĳ�o�����]�Ѩ�L�}���եΡ^
    /// </summary>
    public void ManualTrigger()
    {
        if (!isTriggered)
        {
            TriggerTrap();
        }
    }

    /// <summary>
    /// ��ʭ��m����
    /// </summary>
    public void ManualReset()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        isTriggered = false;
        isFalling = false;
        hasHitPlayer = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// �b�s�边����ܰ����d��
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // ��ܰ����d��
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionRange);

        // ��ܰ����ϰ�
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(transform.position + Vector3.down * (detectionRange / 2), new Vector3(1, detectionRange, 1));
    }
}