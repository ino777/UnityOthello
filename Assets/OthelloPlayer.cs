using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Othello
{

    public abstract class OthelloPlayer
    {
        public int Color { get; }

        public OthelloPlayer(int color)
        {
            Color = color;
        }

        abstract public Pos? Action(int[,] board);

    }
    
    public class OthelloUser : OthelloPlayer
    {
        GameObject clickedGameObject;

        public OthelloUser(int color) : base(color) { }

        public override Pos? Action(int[,] _)
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickedGameObject = null;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(ray, out hit))
                {
                    clickedGameObject = hit.collider.gameObject;
                }

                if (clickedGameObject == null || this.clickedGameObject.tag != "Board Cell")
                {
                    Debug.Log("Invalid click!");
                    return null;
                }

                string[] arr = this.clickedGameObject.name.Split('_');
                int x = int.Parse(arr[1][1].ToString());
                int y = int.Parse(arr[1][0].ToString());
                return new Pos() { x = x, y = y };
            }
            return null;
        }
    }

    public class OthelloComputer : OthelloPlayer
    {
        OthelloAI ai;
        System.Diagnostics.Stopwatch stopWatch;

        public OthelloComputer(int color, int depth) : base(color)
        {
            ai = new OthelloAI(depth, color);
        }

        public override Pos? Action(int[,] board)
        {
            stopWatch = System.Diagnostics.Stopwatch.StartNew();
            Pos? action = ai.AcquireOptAction(board, Color);
            stopWatch.Stop();
            Debug.Log(string.Format("Time: {0}", stopWatch.Elapsed));
            return action;
        }
    }
}



