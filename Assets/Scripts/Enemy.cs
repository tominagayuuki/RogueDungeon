using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

class Enemy : MapObjectBase
{
    public virtual void MoveStart()
    {
        var player = Object.FindObjectOfType<Player>();
        if (!MoveToFollow(player))
        {
            MoveFree();
        }
    }

    protected void MoveFree()
    {
        //現在の左方向から順に右回りに進めるマスか確認していく。
        var startDir = Map.TurnLeftDirection(Forward);
        Forward = startDir;
        do
        {
            //- 壁だと移動できない
            //- 既に他のマップオブジェクトが存在している場合は移動できない
            var (movedMass, movedPos) = Map.GetMovePos(Pos, Forward);
            var massData = movedMass == null ? null : Map[movedMass.Type];
            if (movedMass == null || movedMass.ExistObject != null || !massData.IsRoad)
            {
                //移動できなければ向きを変え、移動確認を行う
                Forward = Map.TurnRightDirection(Forward);
            }
            else
            {
                break;
            }
        } while (startDir != Forward);

        //移動の前方向を決定したら移動する  
        Move(Forward);
    }
    [System.Serializable]
    public class Scope
    {
        //South方向
        [TextArea(3, 10)]
        public string Area = ""
            + "111\n"
            + "111\n"
            + "111";

        public bool IsInArea(Vector2Int target, Vector2Int startPos, Direction dir)
        {
            var relativePos = target - startPos;
            switch (dir)
            {
                case Direction.North: relativePos.x *= -1; relativePos.y *= -1; break;
                case Direction.South: break;
                case Direction.East: var tmp = relativePos.x; relativePos.x = -relativePos.y; relativePos.y = tmp; break;
                case Direction.West: tmp = relativePos.x; relativePos.x = relativePos.y; relativePos.y = -tmp; break;
            }

            var lines = Area.Split('\n');
            var width = lines.Select(_l => _l.Length).FirstOrDefault();
            if (!lines.All(_l => _l.Length == width))
            {
                throw new System.Exception("Areaの各行にサイズが異なるものが存在しています。");
            }

            var left = -width / 2;
            var right = left + width;
            if (left <= relativePos.x && relativePos.x < right)
            {
                if (1 <= relativePos.y && relativePos.y <= lines.Length)
                {
                    var offsetX = relativePos.x - left;
                    if ('1' == lines[relativePos.y - 1][offsetX])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public bool IsChasing = false;
    public Scope VisibleArea;

    protected bool MoveToFollow(MapObjectBase target)
    {
        if (VisibleArea.IsInArea(target.Pos, Pos, Forward))
        {
            Move(Forward);
            IsChasing = true;
            return true;
        }

        if (IsChasing)
        {
            var left = Map.TurnLeftDirection(Forward);
            if (VisibleArea.IsInArea(target.Pos, Pos, left))
            {
                Forward = left;
                Move(Forward);
                IsChasing = true;
                return true;
            }
            var right = Map.TurnRightDirection(Forward);
            if (VisibleArea.IsInArea(target.Pos, Pos, right))
            {
                Forward = right;
                Move(Forward);
                IsChasing = true;
                return true;
            }
        }

        IsChasing = false;
        return false;
    }
}
