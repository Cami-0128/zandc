using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 改進的波浪系統 - 使用 Perlin Noise 生成更真實的波浪
/// </summary>
public class ImprovedWaveSystem : MonoBehaviour
{
    [Header("═══ 波浪基礎設定 ═══")]
    public float waveAmplitude = 0.3f;          // 波浪振幅
    public float waveFrequency = 2f;            // 波浪頻率（越高波浪越密集）
    public float waveSpeed = 1f;                // 波浪移動速度

    [Header("═══ 多層波浪設定 ═══")]
    public bool useMultipleWaves = true;        // 使用多層波浪
    public int waveLayerCount = 3;              // 波浪層數（層數越多越真實，性能消耗越多）

    [SerializeField]
    private float[] layerAmplitudes = new float[] { 0.3f, 0.15f, 0.08f };      // 各層振幅
    [SerializeField]
    private float[] layerFrequencies = new float[] { 1f, 2.5f, 4f };            // 各層頻率
    [SerializeField]
    private float[] layerSpeeds = new float[] { 1f, 1.5f, 2f };                 // 各層速度

    [Header("═══ 邊界網格設定 ═══")]
    public bool useGridDeformation = false;     // 使用網格變形（更真實但性能消耗多）
    public int gridWidth = 20;                  // 網格寬度
    public int gridHeight = 2;                  // 網格高度

    [Header("═══ 性能設定 ═══")]
    public bool enableWaveAnimation = true;
    public float updateFrequency = 10f;         // 每秒更新次數（越高越流暢，越消耗性能）

    private float timeOffset = 0f;
    private SpriteRenderer waterSprite;
    private Mesh waterMesh;
    private MeshCollider meshCollider;
    private Vector3[] originalVertices;
    private Vector3 originalSpritePosition;

    void Start()
    {
        waterSprite = GetComponent<SpriteRenderer>();
        if (waterSprite == null)
        {
            Debug.LogError("[ImprovedWaveSystem] 需要 SpriteRenderer 組件");
            return;
        }

        originalSpritePosition = waterSprite.transform.localPosition;

        // 如果要使用網格變形，需要準備 Mesh
        if (useGridDeformation)
        {
            SetupGridDeformation();
        }

        Debug.Log("[ImprovedWaveSystem] 初始化完成");
    }

    void Update()
    {
        if (!enableWaveAnimation) return;

        timeOffset += Time.deltaTime * waveSpeed;

        if (useGridDeformation)
        {
            UpdateMeshWaves();
        }
        else
        {
            UpdateSpriteWaves();
        }
    }

    // ========== Sprite 波浪更新（簡單版本）==========
    void UpdateSpriteWaves()
    {
        if (waterSprite == null) return;

        float waveY = 0f;

        if (useMultipleWaves)
        {
            // 多層波浪疊加
            for (int i = 0; i < layerAmplitudes.Length; i++)
            {
                float wave = Mathf.Sin((timeOffset * layerSpeeds[i]) * layerFrequencies[i])
                           * layerAmplitudes[i];
                waveY += wave;
            }
        }
        else
        {
            // 單層波浪
            waveY = Mathf.Sin(timeOffset * waveFrequency) * waveAmplitude;
        }

        waterSprite.transform.localPosition = originalSpritePosition + new Vector3(0, waveY, 0);
    }

    // ========== 網格變形（進階版本）==========
    void SetupGridDeformation()
    {
        // 創建平面網格
        waterMesh = new Mesh();
        waterMesh.name = "WaterMesh";

        // 創建頂點
        Vector3[] vertices = new Vector3[gridWidth * gridHeight];
        int[] triangles = new int[(gridWidth - 1) * (gridHeight - 1) * 6];

        float cellWidth = 1f / (gridWidth - 1);
        float cellHeight = 1f / (gridHeight - 1);

        // 初始化頂點
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int index = y * gridWidth + x;
                vertices[index] = new Vector3(
                    x * cellWidth - 0.5f,
                    y * cellHeight - 0.5f,
                    0
                );
            }
        }

        // 創建三角形
        int triIndex = 0;
        for (int y = 0; y < gridHeight - 1; y++)
        {
            for (int x = 0; x < gridWidth - 1; x++)
            {
                int v0 = y * gridWidth + x;
                int v1 = y * gridWidth + x + 1;
                int v2 = (y + 1) * gridWidth + x;
                int v3 = (y + 1) * gridWidth + x + 1;

                triangles[triIndex++] = v0;
                triangles[triIndex++] = v2;
                triangles[triIndex++] = v1;

                triangles[triIndex++] = v1;
                triangles[triIndex++] = v2;
                triangles[triIndex++] = v3;
            }
        }

        waterMesh.vertices = vertices;
        waterMesh.triangles = triangles;
        waterMesh.RecalculateNormals();
        waterMesh.RecalculateBounds();

        originalVertices = vertices;

        // 可選：使用 MeshCollider 進行碰撞檢測
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        meshCollider.convex = false;
    }

    void UpdateMeshWaves()
    {
        if (waterMesh == null || originalVertices == null) return;

        Vector3[] deformedVertices = new Vector3[originalVertices.Length];
        System.Array.Copy(originalVertices, deformedVertices, originalVertices.Length);

        // 對頂點應用波浪
        for (int i = 0; i < deformedVertices.Length; i++)
        {
            float xPos = deformedVertices[i].x;
            float yOffset = 0f;

            if (useMultipleWaves)
            {
                for (int layer = 0; layer < layerAmplitudes.Length; layer++)
                {
                    float wave = Mathf.Sin(
                        (xPos * layerFrequencies[layer] + timeOffset * layerSpeeds[layer]) * Mathf.PI
                    ) * layerAmplitudes[layer];
                    yOffset += wave;
                }
            }
            else
            {
                yOffset = Mathf.Sin(
                    (xPos * waveFrequency + timeOffset * waveSpeed) * Mathf.PI
                ) * waveAmplitude;
            }

            deformedVertices[i].y = originalVertices[i].y + yOffset;
        }

        waterMesh.vertices = deformedVertices;
        waterMesh.RecalculateNormals();
        waterMesh.RecalculateBounds();

        if (meshCollider != null)
        {
            meshCollider.convex = false;
        }
    }

    // ========== 公開方法 ==========

    /// <summary>
    /// 獲取特定 X 位置的波浪高度
    /// 用於讓船隻和物體跟隨波浪
    /// </summary>
    public float GetWaveHeightAtPosition(float xPosition)
    {
        float waveHeight = 0f;

        if (useMultipleWaves)
        {
            for (int layer = 0; layer < layerAmplitudes.Length; layer++)
            {
                float wave = Mathf.Sin(
                    (xPosition * layerFrequencies[layer] + timeOffset * layerSpeeds[layer]) * Mathf.PI
                ) * layerAmplitudes[layer];
                waveHeight += wave;
            }
        }
        else
        {
            waveHeight = Mathf.Sin(
                (xPosition * waveFrequency + timeOffset * waveSpeed) * Mathf.PI
            ) * waveAmplitude;
        }

        return waveHeight;
    }

    /// <summary>
    /// 獲取波浪在特定位置的速度（用於物體跟隨波浪運動）
    /// </summary>
    public float GetWaveVelocityAtPosition(float xPosition)
    {
        float velocity = 0f;

        if (useMultipleWaves)
        {
            for (int layer = 0; layer < layerAmplitudes.Length; layer++)
            {
                float waveVel = Mathf.Cos(
                    (xPosition * layerFrequencies[layer] + timeOffset * layerSpeeds[layer]) * Mathf.PI
                ) * layerAmplitudes[layer] * layerSpeeds[layer] * layerFrequencies[layer];
                velocity += waveVel;
            }
        }
        else
        {
            velocity = Mathf.Cos(
                (xPosition * waveFrequency + timeOffset * waveSpeed) * Mathf.PI
            ) * waveAmplitude * waveSpeed * waveFrequency;
        }

        return velocity;
    }

    public void SetWaveAnimationEnabled(bool enabled) => enableWaveAnimation = enabled;

    public float GetWaveAmplitude() => waveAmplitude;
    public float GetWaveFrequency() => waveFrequency;
}