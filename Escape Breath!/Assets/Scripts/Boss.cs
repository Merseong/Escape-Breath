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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        maxHealth = health;

        // first phase
        phase = 0;
        outsiderRate = 0.1f;
        GameManager.inst.turnSystem.turnTimers[TurnType.MovePiece] = 8f;
    }

    ////test
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space)) //meteor test
    //    {
    //        Debug.Log("test Space");
    //        var A = Instantiate(phasePatterns[1]).GetComponent<BossPattern>();
    //        A.StartPattern();
    //    }
    //}
    ////test
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
        else if (health < 500)
        {
            phase = 2;
            outsiderRate = 0.3f;
            GameManager.inst.turnSystem.turnTimers[TurnType.MovePiece] = 6f;
            redBuff.SetActive(true);
        }
        else if (health < 2000)
        {
            phase = 1;
            outsiderRate = 0.15f;
            GameManager.inst.turnSystem.turnTimers[TurnType.MovePiece] = 7f;
            blueBuff.SetActive(true);
        }
        else
        {
            phase = 0;
            outsiderRate = 0f;
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
        // randomly make outside attacker
        if (Random.Range(0f, 1f) < outsiderRate)
        {
            SpawnOutsideAttacker();
        }

        // select random pattern or biased pattern
        if (isClose)
            return Instantiate(phasePatterns[0]).GetComponent<BossPattern>(); //근접 공격 phasepatterns 임시 숫자임
        else
        {
            int idx = Random.Range(0, phasePatterns.Count);
            return Instantiate(phasePatterns[idx]).GetComponent<BossPattern>();
        }
    }

    public void StartPattern()
    {
        var pattern = SelectPattern();
        pattern.StartPattern();
    }

    void SpawnOutsideAttacker()
    {
        Vector3 randomPos = new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 3f), Random.Range(-3f, 3f));
        var outside = Instantiate(outsideAttackerObj, randomPos, Quaternion.identity).GetComponent<OutsideEnemy>();
        PieceType randomTarget;
        float rand = Random.Range(0f, 1f);
        if (rand < 0.4f) randomTarget = PieceType.King;
        else if (rand < 0.6f) randomTarget = PieceType.Queen;
        else if (rand < 0.8f) randomTarget = PieceType.Rook;
        else if (rand < 0.9f) randomTarget = PieceType.Bishop;
        else randomTarget = PieceType.Knight;
        outside.target = GameManager.inst.chessBoard.GetRandomPiece(randomTarget).transform;
    }
    
    public void CheckClose()
    {
        for (int b = 5; b < 8; b++)
        {
            for (int a = 0; a < 8; a++)
            {
                if (GameManager.inst.chessBoard.GetPiece(a, b) != null)
                    isClose = true;
            }
        }
    }

    public void AttackOnBoard(Vector2Int pos, float duration, bool isStrong = false)
    {
        StartCoroutine(AttackPiece(pos, duration, isStrong));
        GameManager.inst.chessBoard.ShowAttackArea(pos, duration - 0.05f, isStrong);
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
