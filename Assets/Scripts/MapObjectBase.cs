using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
 
class MapObjectBase : MonoBehaviour
{
    [Range(0, 100)] public float MoveSecond = 0.1f;
 
    public bool IsNowMoving { get; private set; } = false;
    public Vector2Int Pos;
    public Vector2Int PrevPos { get; protected set; }
    public Direction Forward;
    Map _map;
    public Map Map { get => _map != null ? _map : (_map = Object.FindObjectOfType<Map>()); }


    public int Life = 5;
    public int Attack = 2;
    public int Exp = 0;

    public enum Group
    {
        Player,
        Enemy,
        Other,
    }
    public Group CurrentGroup = Group.Other;

    [SerializeField] Weapon _weapon;
    [SerializeField] Weapon _weapon2;
    //装備する際に設定処理が必要なのでプロパティ経由で設定するようにする
    public Weapon CurrentWeapon
    {

        get => _weapon;
        set
        {
            if (_weapon != null)
            {
                _weapon.Detach(this);
            }
            _weapon = value;
            if (_weapon != null)
            {
                _weapon.Attach(this);
            }
        }
    }
    private void Awake()
    {
        if (CurrentWeapon != null)
        {
            CurrentWeapon.Attach(this);
        }
    }

    //位置と前方向を設定するメソッド
    public void SetPosAndForward(Vector2Int pos, Direction forward)
    {
        // PosとForwardの設定し、Transformに反映させる
        PrevPos = new Vector2Int(-1, -1);
        Pos = pos;
        Forward = forward;
 
        transform.position = Map.CalcMapPos(Pos);
    }
 
    //移動処理
    public virtual void Move(Direction dir)
    {
        IsNowMoving = false;
        var (movedMass, movedPos) = Map.GetMovePos(Pos, dir);
        if (movedMass == null) return;
 
        var massData = Map[movedMass.Type];
        if(movedMass.ExistObject)
        {
            MoveToExistObject(movedMass, movedPos);
        }
        else if (massData.IsRoad)
        {
            MoveToRoad(movedMass, movedPos);
        }
        else
        {
            MoveToNotMoving(movedMass, movedPos);
        }

        switch (dir)
        {
            case Direction.North:
                this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                break;
            case Direction.South:
                this.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                break;
            case Direction.East:
                this.transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                break;
            case Direction.West:
                this.transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
                break;
        }
    }
 
    protected virtual void MoveToExistObject(Map.Mass mass, Vector2Int movedPos)
    {
        var otherObject = mass.ExistObject.GetComponent<MapObjectBase>();
        if (IsAttackableObject(this, otherObject))
        {
            if (AttackTo(otherObject))
            {//攻撃の結果相手を倒したらそのマスに移動する
                StartCoroutine(MoveCoroutine(movedPos));
                return;
            }
        }

        StartCoroutine(NotMoveCoroutine(movedPos));
    }
 
    protected virtual void MoveToRoad(Map.Mass mass, Vector2Int movedPos)
    {
        StartCoroutine(MoveCoroutine(movedPos));
    }
 
    protected virtual void MoveToNotMoving(Map.Mass mass, Vector2Int movedPos)
    {
        StartCoroutine(NotMoveCoroutine(movedPos));
    }
 
    IEnumerator MoveCoroutine(Vector2Int target)
    {
        //マップのマス情報を更新する
        var startMass = Map[Pos.x, Pos.y];
        startMass.ExistObject = null;
        var movedPos = Map.CalcMapPos(target);
        PrevPos = Pos;
        Pos = target;
        var movedMass = Map[Pos.x, Pos.y];
        movedMass.ExistObject = gameObject;
 
        //モデルの移動処理
        IsNowMoving = true;
        var start = transform.position;
        var timer = 0f;
        while(timer < MoveSecond)
        {
            yield return null;
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(start, movedPos, timer/MoveSecond);
        }
        transform.position = movedPos;
        IsNowMoving = false;
    }

    protected IEnumerator NotMoveCoroutine(Vector2Int target)
    {
        var movedPos = Map.CalcMapPos(target);
 
        IsNowMoving = true;
        var start = transform.position;
        var timer = 0f;
        movedPos = Vector3.Lerp(start, movedPos, 0.5f);
        while (timer < MoveSecond)
        {
            yield return null;
            timer += Time.deltaTime;
            var t = 1f - Mathf.Abs(timer / MoveSecond * 2 - 1f);
            transform.position = Vector3.Lerp(start, movedPos, t);
        }
        transform.position = start;
        IsNowMoving = false;
    }

    public static bool IsAttackableObject(MapObjectBase self, MapObjectBase other)
    {
        return self.CurrentGroup != other.CurrentGroup
            && (self.CurrentGroup != Group.Other && other.CurrentGroup != Group.Other);
    }

    public virtual bool AttackTo(MapObjectBase other)
    {
        other.Life -= Attack;
        other.Damaged(Attack);
        
        if (other.Life <= 0)
        {
            other.Dead();
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void Damaged(int damage)
    { }

    public virtual void Dead()
    {
        Object.Destroy(gameObject);
    }
   
}