﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Boss : MonoBehaviour
{
    private float maxHealth;
    public int health;
    public int phase = 0;
    public List<GameObject> phasePatterns = new List<GameObject>();

    [Space(10)]
    public GameObject outsideAttackerObj;
    private float outsiderRate = 0.1f;

    [Space(10)]
    public int attackType;
    public Transform bossTarget;
    [SerializeField]
    private Slider redHealth = null;
    private float redValue = 1;
    [SerializeField]
    private Slider yellowHealth = null;
    private float yellowValue = 1;
    public Object attackArea;
    public GameObject redBuff;
    public GameObject blueBuff;

    public Transform attackPoint;
    [HideInInspector]
    public Rigidbody rb;

    bool isClose = false;

    private int guardOn;
    private int pastPattern;
    public int turnLimit = 40;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        maxHealth = health;

        // first phase
        phase = 0;
        outsiderRate = 0.1f;
        guardOn = 0;
        pastPattern = -1;
        GameManager.inst.turnSystem.turnTimers[TurnType.MovePiece] = 8f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            var obj = other.GetComponent<AttackObj>();
            Damaged(obj.damage);

            obj.rb.isKinematic = true;
            obj.rb.velocity = Vector3.zero;
            obj.transform.position = transform.position;
        }
        else if (other.CompareTag("Piece"))
        {
            var obj = other.GetComponent<Piece>();
            Damaged(obj.damage);

            GameManager.inst.chessBoard.PermanantRemovePiece(obj);
        }
    }

    private void Damaged(int damage)
    {
        if (!GameManager.inst.isPlaying) return;
        health = Mathf.Max(0, health - damage);
        redValue = health / maxHealth;
        redHealth.value = redValue;
        if (health == 0) GameManager.inst.GameClear();
        else if (health < 1000)
        {
            phase = 2;
            outsiderRate = 0.35f;
            GameManager.inst.turnSystem.turnTimers[TurnType.MovePiece] = 6f;
            redBuff.SetActive(true);
        }
        else if (health < 2250)
        {
            phase = 1;
            outsiderRate = 0.25f;
            GameManager.inst.turnSystem.turnTimers[TurnType.MovePiece] = 7f;
            blueBuff.SetActive(true);
        }
        else
        {
            phase = 0;
            outsiderRate = 0.1f;
            GameManager.inst.turnSystem.turnTimers[TurnType.MovePiece] = 8f;
        }
    }

    public void ResetYellowHealth()
    {
        yellowValue = redValue;
        yellowHealth.value = yellowValue;
    }

    private BossPattern SelectPattern()
    {
        if (GameManager.inst.turnSystem.turnCount < turnLimit)
        {
            // randomly make outside attacker
            if (Random.Range(0f, 1f) < outsiderRate)
            {
                SpawnOutsideAttacker();
            }
            CheckClose();
            // select random pattern or biased pattern
            if (isClose)
            {
                if (guardOn == 0)
                {
                    guardOn = 2;
                    return Instantiate(phasePatterns[0]).GetComponent<BossPattern>();
                }
                else
                {
                    guardOn--;
                    int idx = pastPattern;
                    while (idx == pastPattern)
                    {
                        idx = Random.Range(1, phasePatterns.Count - 1);
                    }
                    pastPattern = idx;
                    return Instantiate(phasePatterns[idx]).GetComponent<BossPattern>();
                }
            }
            else
            {
                int idx = pastPattern;
                while (idx == pastPattern)
                {
                    idx = Random.Range(1, phasePatterns.Count - 1);
                }
                pastPattern = idx;
                return Instantiate(phasePatterns[idx]).GetComponent<BossPattern>();
            }
        }
        else
        {
            Debug.Log("End Limit!");
            for (int i = 0; i < 5; i++)
            {
                SpawnOutsideAttacker();
            }
            return Instantiate(phasePatterns[4]).GetComponent<BossPattern>(); //필중 패턴 구현 해야함
        }
    }

    public void StartPattern()
    {
        var pattern = SelectPattern();
        isClose = false;
        pattern.StartPattern();
    }

    void SpawnOutsideAttacker()
    {
        Vector3 randomPos = new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 3f), Random.Range(-3f, 3f));
        Instantiate(outsideAttackerObj, randomPos, Quaternion.identity);
    }
    
    public void CheckClose()
    {
        for (int b = 5; b < 8; b++)
        {
            for (int a = 0; a < 8; a++)
            {
                var p = GameManager.inst.chessBoard.GetPiece(a, b);
                if (p != null && p.isAlive)
                {
                    isClose = true;
                    break;
                }
            }
            if (isClose == true)
                break;
        }
    }

    public void AttackOnBoard(Vector2Int pos, float duration, bool isStrong = false)
    {
        StartCoroutine(AttackPiece(pos, duration - 0.01f, isStrong));
        GameManager.inst.chessBoard.ShowAttackArea(pos, duration - 0.01f, isStrong);
    }

    IEnumerator AttackPiece(Vector2Int pos, float time, bool isStrong)
    {
        yield return new WaitForSeconds(time);

        if (GameManager.inst.chessBoard.GetPiece(pos.x, pos.y) != null)
        {
            GameManager.inst.chessBoard.GetPiece(pos.x, pos.y).Damaged(isStrong);
        }
        yield return null;
    }
}
