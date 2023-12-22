using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

class Player : MapObjectBase
{
    public int Level = 1;//レベル
    public int MaxExp = 20;//レベルアップに必要な経験値の最大値
    public int MaxLife = 30;//Hpの上限設定
    public int FloorNumber = 1;//進んだ階層
    private int count;
    string MyName = "ユウキ";

    MessageWindow _messageWindow;
    MessageWindow MessageWindow
    {
        get =>_messageWindow != null ? _messageWindow : (_messageWindow = MessageWindow.Find());
    }
    void Start()
    {
        
        var playerUI = Object.FindObjectOfType<PlayerUI>();
        playerUI.Set(this);

        StartCoroutine(CameraMove());
        StartCoroutine(ActionCoroutine());
    }

    public enum Action
    {
        None,
        MoveUp,
        MoveDown,
        MoveRight,
        MoveLeft,
        MoveAttck,
    }
    public Action NowAction { get; private set; } = Action.None;
    public bool DoWaitEvent { get; set; } = false;

    public bool IsNextMap = false;
    IEnumerator ActionCoroutine()
    {
        while (true)
        {
            //入力待ち
            StartCoroutine(WaitInput());
            yield return new WaitWhile(() => NowAction == Action.None);
            //アクションの実行
            switch (NowAction)
            {
                case Action.MoveUp:
                case Action.MoveDown:
                case Action.MoveRight:
                case Action.MoveLeft:
                case Action.MoveAttck:
                    Move(ToDicection(NowAction));
                    yield return new WaitWhile(() => IsNowMoving);     //アクションが終わるまで待つ
                    break;
            }
            if (Exp >= MaxExp)
            {
                LevelUp();
            }
            UpdateFood();//空腹度の処理
            NowAction = Action.None;

            //イベントを確認
            CheckEvent();
            yield return new WaitWhile(() => DoWaitEvent);
        }
    }
    Direction ToDicection(Action action)
    {
        switch (action)
        {
            case Action.MoveUp: return Direction.North;
            case Action.MoveDown: return Direction.South;
            case Action.MoveRight: return Direction.East;
            case Action.MoveLeft: return Direction.West;
            case Action.MoveAttck: return Direction.stay;
            default: throw new System.NotImplementedException();
        }
    }

    IEnumerator WaitInput()
    {
        NowAction = Action.None;
        //キー入力の確認
        while (NowAction == Action.None)
        {
            yield return null;
            //入力されたキーの確認
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveUp; transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveDown; transform.rotation = Quaternion.Euler(0.0f, -180.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveRight; transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveLeft; transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.Return)) NowAction = Action.MoveAttck;
            if (Input.GetKeyDown(KeyCode.G)) { Dead(); }//セーブ初期化のデバック用
            if (Input.GetKeyDown(KeyCode.M)) { MiniMapOff(); }//ミニマップの非表示
            if (Input.GetKeyDown(KeyCode.E)) { MenuOpen(); }//メニュー画面表示
            if (Input.GetKeyDown(KeyCode.Escape)) { MenuClose(); }//メニュー画面非表示
        }
    }

    void CheckEvent()
    {
        //ゴール判定
        var mass = Map[Pos.x, Pos.y];
        if (mass.Type == MassType.Goal)
        {
            ChoseOn();
        }
        else
        {
            //StartCoroutine(Goal());
            ChoseOff();
            //終了処理
            DoWaitEvent = true;
        }

        MenuClose();
        DoWaitEvent = false;
        StartCoroutine(RunEvents());
    }

    [Range(0, 100)] public float CameraDistance;
    public Vector3 CameraDirection = new Vector3(0, 10, -3);
    IEnumerator CameraMove()
    {
        var camera = Camera.main;

        while (true)
        {
            //カメラの位置をプレイヤーからの相対位置に設定する
            camera.transform.position = transform.position + CameraDirection.normalized * CameraDistance;
            camera.transform.LookAt(transform.position);
            yield return null;
        }
    }

    IEnumerator RunEvents()
    {
        //敵の移動処理
        foreach (var enemy in Object.FindObjectsOfType<Enemy>())
        {
            enemy.MoveStart();
        }
        //全ての敵が移動完了するまで待つ
        yield return new WaitWhile(() => Object.FindObjectsOfType<Enemy>().All(_e => !_e.IsNowMoving));
    }

    IEnumerator Goal()
    {
        FloorNumber += 1;
        yield return new WaitForSeconds(0.0f);//ゴール時にウェイト入れたければ使う

        var mapSceneManager = Object.FindObjectOfType<MapSceneManager>();
        mapSceneManager.GenerateMap();

        var player = Object.FindObjectOfType<Player>();
        player.MaxLife = MaxLife;
        player.Life = Life;
        player.Food = Food;
        player.MaxExp = MaxExp;
        player.Exp = Exp;
        player.Attack = Attack;
        player.Level = Level;
        player.FloorNumber = FloorNumber;
        player.CurrentWeapon = CurrentWeapon;
        if(CurrentWeapon != null)
        {
            player.Attack -= CurrentWeapon.Attack;
        }

        var saveData = new SaveData();
        saveData.Level = Level;
        saveData.MaxLife = MaxLife;
        saveData.Life = Life;
        saveData.Food = Food;
        saveData.MaxExp = MaxExp;
        saveData.Exp= Exp;
        saveData.Attack = Attack;
        saveData.FloorNumber = FloorNumber;
        if (CurrentWeapon != null)
        {
            saveData.Attack-=CurrentWeapon.Attack;
            saveData.WeaponName = CurrentWeapon.Name;
            saveData.WeaponAttack = CurrentWeapon.Attack;
        }
        else
        {
            saveData.WeaponName = "";
            saveData.WeaponAttack = 0;
        }
        saveData.MapData = Map.MapData;
        saveData.Save();

    }

    public override void Dead()
    {
        base.Dead();
        MessageWindow.AppendMessage($"{MyName}は力尽きた");
        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        mapManager.GameOver.SetActive(true);

        SaveData.Destoy();
    }

    public void ChoseOn()
    {
        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        mapManager.NextSelect.SetActive(true);
    }
    public void ChoseOff()
    {

        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        mapManager.NextSelect.SetActive(false);
    }
    public void MenuOpen()
    {
        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        mapManager.MainMenu.SetActive(true);
    }

    public void MenuClose()
    {
        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        mapManager.MainMenu.SetActive(false);
        mapManager.SubMenu.SetActive(false);
    }

    public void MiniMapOff()
    {
        count += 1;
        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        if (count == 1)
        {
            mapManager.MiniMap.SetActive(false);
        }
        if (count == 2)
        {
            mapManager.MiniMap.SetActive(true);
            count = 0;
        }
    }

    public void NextMap()
    {
        ChoseOff();
        StartCoroutine(Goal());
    }

    public override bool AttackTo(MapObjectBase other)
    {
        MessageWindow.AppendMessage($"{MyName}の攻撃!{Attack}ダメージ与えた");
        other.Life -= Attack;
        other.Damaged(Attack);
        if (other.Life <= 0)
        {
            MessageWindow.AppendMessage($"モンスターは倒れた!{other.Exp}経験値得た");
            other.Dead();
            //攻撃の結果敵を倒したらその敵のExp分自身のExpを上げる
            Exp += other.Exp;
           
            return true;
        }
        else
        {
            return false;
        }
    }


    public void LevelUp()
    {
        Level += 1;
        Exp -= MaxExp;
        MaxExp = 20 * Level;
        MaxLife += 5;
        Life += 5;
        Attack += 1;

        if (MaxLife <= Life)
        {
            Life = MaxLife;
        }

        if (Level == 2)
        { 
        }
        MessageWindow.AppendMessage($"レベルが上がった! {Level - 1}から{Level}になった");
        MessageWindow.AppendMessage($"体力が5増えた! 攻撃力が1上がった!");
    }

    public override void Damaged(int damage)
    {
        base.Damaged(damage);
        MessageWindow.AppendMessage($"攻撃された!{damage}ダメージ受けた");
    }

    public int Food = 100;
    public int foodCount = 10;
    void UpdateFood()
    {
        foodCount--;

        if(foodCount <= 0) { 
            Food--;
            foodCount = 10;
        }
        if (Food <= 0)
        {
            Food = 0;
            Life--;
            MessageWindow.AppendMessage($"空腹ダメージ!!体力が{Life}が1減った");
        }
    }

    protected override void MoveToExistObject(Map.Mass mass, Vector2Int movedPos)
    {
        var otherObject = mass.ExistObject.GetComponent<MapObjectBase>();
        if (otherObject is Treasure)
        {
            var treasure = (otherObject as Treasure);
            OpenTreasure(treasure, mass, movedPos);
            StartCoroutine(NotMoveCoroutine(movedPos));
            return;
        }
        else if (otherObject is Trap) //追加
        {
            var trap = (otherObject as Trap);
            StampTrap(trap, mass, movedPos);
            StartCoroutine(NotMoveCoroutine(movedPos));
            return;
        }

        base.MoveToExistObject(mass, movedPos);
    }

    protected void StampTrap(Trap trap, Map.Mass mass, Vector2Int movedPos)
    {

        MessageWindow.AppendMessage($"罠だ!!");
        switch (trap.CurrentType)
        {
            case Trap.Type.LifeDown:
                Life -= trap.Value;
                MessageWindow.AppendMessage($"体力が{trap.Value}減った");

                if(Life <= 0)
                {
                    Dead();
                }
                break;
            case Trap.Type.FoodDown:
                Food -= trap.Value;
                MessageWindow.AppendMessage($"空腹度が{trap.Value}減った");
                break;
            default: throw new System.NotImplementedException();
        }

        //罠はマップから削除する
        mass.ExistObject = null;
        mass.Type = MassType.Road;
        Object.Destroy(trap.gameObject);
    }

    protected void OpenTreasure(Treasure treasure, Map.Mass mass, Vector2Int movedPos)
    {
        
        switch (treasure.CurrentType)
        {
            case Treasure.Type.LifeUp:
                if (MaxLife > Life)
                {
                    Life += treasure.Value;
                    if (MaxLife <= Life)
                    {
                        Life = MaxLife;
                    }
                }
               
                MessageWindow.AppendMessage($"回復薬だ！体力が{treasure.Value}回復した");
                break;
            case Treasure.Type.FoodUp:
                Food += treasure.Value;
                if (Food >= 100)
                {
                    Food = 100;
                    foodCount = 10;
                }
                MessageWindow.AppendMessage($"おにぎりだ！空腹度が{treasure.Value}回復した");
                break;
            case Treasure.Type.Weapon:
                //装備中の武器の攻撃力に足し合わせる
                MessageWindow.AppendMessage($"{treasure.CurrentWeapon.Name}を見つけた！");
                MessageWindow.AppendMessage($"この武器の攻撃力は{treasure.CurrentWeapon}だ!");
               
                var newWeapon = treasure.CurrentWeapon.Merge(CurrentWeapon);
                CurrentWeapon = newWeapon;
                break;
            default: throw new System.NotImplementedException();
        }

        //宝箱を開けたらマップから削除する
        mass.ExistObject = null;
        mass.Type = MassType.Road;
        Object.Destroy(treasure.gameObject);
    }

    public void Recover(SaveData saveData)
    {
        CurrentWeapon = null;
        Level = saveData.Level;
        MaxLife = saveData.MaxLife;
        Life = saveData.Life;
        Food = saveData.Food;
        Attack = saveData.Attack;
        MaxExp = saveData.MaxExp;
        Exp = saveData.Exp;
        FloorNumber = saveData.FloorNumber;
        if (saveData.WeaponName != "")
        {
            var weapon = ScriptableObject.CreateInstance<Weapon>();
            weapon.Name = saveData.WeaponName;
            weapon.Attack = saveData.WeaponAttack;
            CurrentWeapon = weapon;
        }
    }
}
