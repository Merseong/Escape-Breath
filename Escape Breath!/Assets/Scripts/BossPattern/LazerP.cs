﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerP : BossPattern
{
    public GameObject lazerPiece;
    Boss boss;
    ChessBoard board;
    int phase;
    int checkKing;

    public override void StartPattern()
    {
        //Debug.Log("lazerStart");
        board = GameManager.inst.chessBoard;
        boss = GameManager.inst.boss;
        phase = boss.phase;
        SelectTarget();
        AttackReady();
    }
    protected override void SelectTarget()
    {
        //Debug.Log("select start");
        Piece king = board.GetRandomPiece(PieceType.King);
        if (phase > 0)
        {
            if (!king.isActive)
            {
                Debug.Log("king is out!");
                checkKing = 0;
            }
            else
            {
                targets.Enqueue(king.boardIdx);
                checkKing = 1;
            }
        }
        Vector2Int check = new Vector2Int(-1, -1);
        for (int i = 0; i < (phase + 1);)
        {
            i++;
            Vector2Int pos = new Vector2Int();
            pos = board.GetRandomPiece().boardIdx;
            if(check == pos)
            {
                i--;
                continue;
            }
            targets.Enqueue(pos);
        }
        //Debug.Log("Target select done");
    }
    public void AttackReady()
    {
        //Debug.Log("atkkkk");
        for (int i = 0; i < (phase + 1) + checkKing; i++)
        {
            var atkPos = targets.Dequeue();
            float disappearTime = Random.Range(2, 2.5f);
            GameObject obj = Instantiate(lazerPiece, board.IndexToGlobalPos(atkPos.x, atkPos.y), Quaternion.Euler(90,0,0));
            boss.AttackOnBoard(atkPos, disappearTime, true);
            boss.AttackOnBoard(new Vector2Int((atkPos.x + 1), atkPos.y), disappearTime, false);
            boss.AttackOnBoard(new Vector2Int((atkPos.x - 1), atkPos.y), disappearTime, false);
            boss.AttackOnBoard(new Vector2Int(atkPos.x, (atkPos.y + 1)), disappearTime, false);
            boss.AttackOnBoard(new Vector2Int(atkPos.x, (atkPos.y - 1)), disappearTime, false);
            obj.GetComponent<LazerPiece>().Throw(disappearTime);
        }
        checkKing = 0;
    }
}
