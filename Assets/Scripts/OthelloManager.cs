using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Othello;



public class OthelloManager : MonoBehaviour {

    // Othello Board
    Board board;

    // Othello Player
    OthelloPlayer player1;
    OthelloPlayer player2;

    // Othello Evaluator
    OthelloEvaluator evaluator;

    // Othello AI
    OthelloAI ai;
    private int aiDepth = 4;

    // Stone Prefabs
    public GameObject blackStonePrefab;
    public GameObject whiteStonePrefab;

    // Audio
    AudioSource audioSource;


    // UI Text
    public Text blackCountText;
    public Text whiteCountText;
    public Text blackEventText;
    public Text whiteEventText;

    // Game parameter
    int turn = 1;
    bool endFlag = false;
    
	// Use this for initialization
	void Start ()
    {
        board = new Board();
        board.Init();

        player1 = new OthelloUser(StoneColor.black);
        // player2 = new OthelloUser(StoneColor.white);
        // player1 = new OthelloComputer(StoneColor.black, aiDepth);
        player2 = new OthelloComputer(StoneColor.white, aiDepth);

        PutStone(new Pos() { x = 3, y = 3 }, StoneColor.black);
        PutStone(new Pos() { x = 3, y = 4 }, StoneColor.white);
        PutStone(new Pos() { x = 4, y = 3 }, StoneColor.white);
        PutStone(new Pos() { x = 4, y = 4 }, StoneColor.black);

        evaluator = new OthelloEvaluator();

        audioSource = GetComponent<AudioSource>();

        blackCountText.text = "2";
        whiteCountText.text = "2";
    }



    // Update is called once per frame
    void Update()
    {
        if (endFlag) { return; }

        // End
        if (IsEnd())
        {
            endFlag = true;
            Debug.Log("END!!");
            int winner = GetWinner();
            if (winner == StoneColor.black)
            {
                blackEventText.text = "Win";
                whiteEventText.text = "Lose";
            }
            else if (winner == StoneColor.white)
            {
                blackEventText.text = "Lose";
                whiteEventText.text = "Win";
            }
            Debug.Log(string.Format("Winner: {0}", winner));

            return;
        }


        int color = TurnColor(turn);

        // Pass
        if (board.Availables(color).Count == 0)
        {
            Debug.Log("skipped!");
            turn += 1;
            return;
        }

        OthelloPlayer player;
        if (color == player1.Color)
        {
            player = player1;
        }
        else if (color == player2.Color)
        {
            player = player2;
        }
        else
        {
            return;
        }

        Pos action = player.Action(board.GetBoard(), turn) ?? new Pos() { x = -1, y = -1 };
        if (!ValidPos(action, color)) { return; }

        PutStone(action, color);

        // Make a sound
        audioSource.PlayOneShot(audioSource.clip);

        ReverseStones(board.GetReversibles(action, color), color);

        board.UpdateBoard(action, color);
        blackCountText.text = board.CountStones(StoneColor.black).ToString();
        whiteCountText.text = board.CountStones(StoneColor.white).ToString();
        turn += 1;

    }

    // Get this turn color
    // 1st: Black, 2nd:White
    int TurnColor(int turn)
    {
        if (turn % 2 == 1)
        {
            return StoneColor.black;   // Black
        }
        else
        {
            return StoneColor.white;  // White
        }
    }

    // Whether the given position is possible to put stone
    bool ValidPos(Pos pos, int color)
    {
        return 0 <= pos.x && pos.x < 8 && 0 <= pos.y && pos.y < 8 && board.IsAvailable(pos, color);
    }

    // Put stone
    void PutStone(Pos pos, int color)
    {
        GameObject stone;

        // Select stone color
        if (color == 1)
        {
            stone = Instantiate(blackStonePrefab) as GameObject;
        }
        else if (color == -1)
        {
            stone = Instantiate(whiteStonePrefab) as GameObject;
        }
        else
        {
            return;
        }

        stone.name = string.Format("Stone_{0}{1}", pos.y, pos.x);

        // Force option
        // This option is mainly used when putting a stone by a system, not player
        GameObject cell = GameObject.Find(string.Format("Board Cell_{0}{1}", pos.y, pos.x));
        stone.transform.position = new Vector3(cell.transform.position.x, 0.15f, cell.transform.position.z);

        // Put the stone where player clicked
        // stone.transform.position = new Vector3(clickedGameObject.transform.position.x, 0.18f, clickedGameObject.transform.position.z);
        
    }

    // Reverse stones
    void ReverseStones(List<List<Pos>> reversibles, int color)
    {
        foreach(List<Pos> list in reversibles)
        {
            foreach(Pos pos in list)
            {
                GameObject stone = GameObject.Find(string.Format("Stone_{0}{1}", pos.y, pos.x));
                Destroy(stone);

                PutStone(pos, color);
            }
        }
    }


    // Game end flag
    bool IsEnd()
    {
        return board.Availables(StoneColor.black).Count == 0 && board.Availables(StoneColor.white).Count == 0;
    }

    // Get the winner
    int GetWinner()
    {
        int black = board.CountStones(StoneColor.black);
        int white = board.CountStones(StoneColor.white);

        if (black > white)
        {
            return StoneColor.black;
        }
        else if (white > black)
        {
            return StoneColor.white;
        }
        else
        {
            return 0;   // Get even
        }
    }

}
