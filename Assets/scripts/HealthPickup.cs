using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("血包設定")]
    public int healAmount = 25;              // 回復的血量
    public AudioClip pickupSound;            // 拾取音效
    public GameObject pickupEffect;          // 拾取特效（可選）

    [Header("動畫設定")]
    public bool enableFloating = true;       // 是否啟用浮動動畫
    public float floatSpeed = 2f;           // 浮動速度
    public float floatHeight = 0.5f;        // 浮動高度

    [Header("功能設定")]
    [Tooltip("是否啟用滿血判斷，若滿血則不拾取血包")]
    public bool useCheckHealthFull = true;

    private Vector3 startPosition;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;

        // 獲取或添加AudioSource組件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // 浮動動畫
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 檢查是否碰到玩家
        if (other.CompareTag("Player"))
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();

            if (player != null)
            {
                if (useCheckHealthFull)
                {
                    // 檢查玩家是否已經滿血
                    if (player.GetCurrentHealth() >= player.GetMaxHealth())
                    {
                        Debug.Log("玩家血量已滿，無法使用血包！");
                        return; // 血量滿時不拾取，保留血包
                    }
                }

                // 治療玩家
                player.Heal(healAmount);

                // 播放音效
                PlayPickupSound();

                // 播放特效
                PlayPickupEffect();

                // 顯示提示訊息（可選）
                Debug.Log($"玩家拾取血包，回復 {healAmount} 點血量！");

                // 銷毀血包
                Destroy(gameObject, 0.1f); // 稍微延遲銷毀，讓音效有時間播放
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
            // 在血包位置生成特效
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }
    }

    // 可選：在Scene視窗中顯示偵測範圍
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
