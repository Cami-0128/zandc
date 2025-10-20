using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("��]�]�w")]
    public int healAmount = 25;              // �^�_����q
    public AudioClip pickupSound;            // �B������
    public GameObject pickupEffect;          // �B���S�ġ]�i��^

    [Header("�ʵe�]�w")]
    public bool enableFloating = true;       // �O�_�ҥίB�ʰʵe
    public float floatSpeed = 2f;           // �B�ʳt��
    public float floatHeight = 0.5f;        // �B�ʰ���

    [Header("�\��]�w")]
    [Tooltip("�O�_�ҥκ���P�_�A�Y����h���B����]")]
    public bool useCheckHealthFull = true;

    private Vector3 startPosition;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;

        // ����βK�[AudioSource�ե�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // �B�ʰʵe
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �ˬd�O�_�I�쪱�a
        if (other.CompareTag("Player"))
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();

            if (player != null)
            {
                if (useCheckHealthFull)
                {
                    // �ˬd���a�O�_�w�g����
                    if (player.GetCurrentHealth() >= player.GetMaxHealth())
                    {
                        Debug.Log("���a��q�w���A�L�k�ϥΦ�]�I");
                        return; // ��q���ɤ��B���A�O�d��]
                    }
                }

                // �v�����a
                player.Heal(healAmount);

                // ���񭵮�
                PlayPickupSound();

                // ����S��
                PlayPickupEffect();

                // ��ܴ��ܰT���]�i��^
                Debug.Log($"���a�B����]�A�^�_ {healAmount} �I��q�I");

                // �P����]
                Destroy(gameObject, 0.1f); // �y�L����P���A�����Ħ��ɶ�����
            }
        }
    }

    void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.clip = pickupSound;
            audioSource.Play();
        }
    }

    void PlayPickupEffect()
    {
        if (pickupEffect != null)
        {
            // �b��]��m�ͦ��S��
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }
    }

    // �i��G�bScene��������ܰ����d��
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
