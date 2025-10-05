using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTeleporter : MonoBehaviour
{
    [Header("���a�]�w")]
    public Transform player;

    [Header("�ǰe�I�]�w")]
    public List<Transform> teleportPoints = new List<Transform>();

    [Header("��ܳ]�w")]
    public float selectionTime = 3f; // ��ܮɶ�

    private bool isSelecting = false;
    private Coroutine selectionCoroutine;

    void Awake()
    {
        // ��TeleportManager�b�������d�ɤ��Q�P��
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        RefreshTeleportSystem();
    }

    void OnLevelWasLoaded(int level)
    {
        // �C�����J�s���d�ɭ��s�]�m
        RefreshTeleportSystem();
    }

    void RefreshTeleportSystem()
    {
        // ���s�M�䪱�a
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // ���s������e���d���ǰe�I
        teleportPoints.Clear();
        GameObject[] points = GameObject.FindGameObjectsWithTag("TeleportPoint");
        foreach (GameObject point in points)
        {
            teleportPoints.Add(point.transform);
        }

        Debug.Log($"��� {teleportPoints.Count} �Ӷǰe�I");
    }

    void Update()
    {
        // �ˬd�O�_�P�ɫ���L�BO�BP
        if (Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.P))
        {
            // �����Ĳ�o�A�u�b�䤤�@�������U��Ĳ�o
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.P))
            {
                StartSelection();
            }
        }

        // �b��ܼҦ����ˬd�Ʀr��
        if (isSelecting)
        {
            CheckNumberKeys();
        }
    }

    void StartSelection()
    {
        if (teleportPoints.Count == 0 || player == null)
        {
            Debug.Log("�S���]�m�ǰe�I�Ϊ��a�I");
            return;
        }

        // ���ǰe��Ĥ@���I
        TeleportToPoint(0);
        Debug.Log("�ǰe���1���I�I�Цb3�����Ʀr���ܨ�L�ǰe�I");

        // �}�l��ܼҦ�
        isSelecting = true;
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
        }
        selectionCoroutine = StartCoroutine(SelectionTimer());
    }

    void CheckNumberKeys()
    {
        // �ˬd�Ʀr��1-9
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelectTeleportPoint(i);
                return;
            }
        }

        // �ˬd�Ʀr��0 (������10�Ӷǰe�I)
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectTeleportPoint(10);
        }
    }

    void SelectTeleportPoint(int number)
    {
        int index = number - 1; // �ഫ���}�C����

        if (index < 0 || index >= teleportPoints.Count)
        {
            Debug.Log($"�ǰe�I {number} ���s�b�I(�@�� {teleportPoints.Count} �Ӷǰe�I)");
            return;
        }

        TeleportToPoint(index);
        Debug.Log($"�ǰe��� {number} ���I�I");

        // ������ܼҦ�
        EndSelection();
    }

    void TeleportToPoint(int index)
    {
        if (index < 0 || index >= teleportPoints.Count) return;

        // ����ؼЦ�m
        Vector3 targetPosition = teleportPoints[index].position;

        // �p�G���a��CharacterController�ե�
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.position = targetPosition;
            cc.enabled = true;
        }
        else
        {
            // ��������
            player.position = targetPosition;
        }
    }

    IEnumerator SelectionTimer()
    {
        yield return new WaitForSeconds(selectionTime);

        if (isSelecting)
        {
            Debug.Log("��ܮɶ������I");
            EndSelection();
        }
    }

    void EndSelection()
    {
        isSelecting = false;
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
            selectionCoroutine = null;
        }
    }

    // ��ʭ��s��z�t�Ρ]�i��^
    [ContextMenu("���s��z�ǰe�t��")]
    public void ManualRefresh()
    {
        RefreshTeleportSystem();
    }
}