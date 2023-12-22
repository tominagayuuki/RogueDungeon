using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

class Player : MapObjectBase
{
    public int Level = 1;//���x��
    public int MaxExp = 20;//���x���A�b�v�ɕK�v�Ȍo���l�̍ő�l
    public int MaxLife = 30;//Hp�̏���ݒ�
    public int FloorNumber = 1;//�i�񂾊K�w
    private int count;
    string MyName = "���E�L";

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
            //���͑҂�
            StartCoroutine(WaitInput());
            yield return new WaitWhile(() => NowAction == Action.None);
            //�A�N�V�����̎��s
            switch (NowAction)
            {
                case Action.MoveUp:
                case Action.MoveDown:
                case Action.MoveRight:
                case Action.MoveLeft:
                case Action.MoveAttck:
                    Move(ToDicection(NowAction));
                    yield return new WaitWhile(() => IsNowMoving);     //�A�N�V�������I���܂ő҂�
                    break;
            }
            if (Exp >= MaxExp)
            {
                LevelUp();
            }
            UpdateFood();//�󕠓x�̏���
            NowAction = Action.None;

            //�C�x���g���m�F
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
        //�L�[���͂̊m�F
        while (NowAction == Action.None)
        {
            yield return null;
            //���͂��ꂽ�L�[�̊m�F
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveUp; transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveDown; transform.rotation = Quaternion.Euler(0.0f, -180.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveRight; transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift)) { NowAction = Action.MoveLeft; transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f); }
            if (Input.GetKeyDown(KeyCode.Return)) NowAction = Action.MoveAttck;
            if (Input.GetKeyDown(KeyCode.G)) { Dead(); }//�Z�[�u�������̃f�o�b�N�p
            if (Input.GetKeyDown(KeyCode.M)) { MiniMapOff(); }//�~�j�}�b�v�̔�\��
            if (Input.GetKeyDown(KeyCode.E)) { MenuOpen(); }//���j���[��ʕ\��
            if (Input.GetKeyDown(KeyCode.Escape)) { MenuClose(); }//���j���[��ʔ�\��
        }
    }

    void CheckEvent()
    {
        //�S�[������
        var mass = Map[Pos.x, Pos.y];
        if (mass.Type == MassType.Goal)
        {
            ChoseOn();
        }
        else
        {
            //StartCoroutine(Goal());
            ChoseOff();
            //�I������
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
            //�J�����̈ʒu���v���C���[����̑��Έʒu�ɐݒ肷��
            camera.transform.position = transform.position + CameraDirection.normalized * CameraDistance;
            camera.transform.LookAt(transform.position);
            yield return null;
        }
    }

    IEnumerator RunEvents()
    {
        //�G�̈ړ�����
        foreach (var enemy in Object.FindObjectsOfType<Enemy>())
        {
            enemy.MoveStart();
        }
        //�S�Ă̓G���ړ���������܂ő҂�
        yield return new WaitWhile(() => Object.FindObjectsOfType<Enemy>().All(_e => !_e.IsNowMoving));
    }

    IEnumerator Goal()
    {
        FloorNumber += 1;
        yield return new WaitForSeconds(0.0f);//�S�[�����ɃE�F�C�g���ꂽ����Ύg��

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
        MessageWindow.AppendMessage($"{MyName}�͗͐s����");
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
        MessageWindow.AppendMessage($"{MyName}�̍U��!{Attack}�_���[�W�^����");
        other.Life -= Attack;
        other.Damaged(Attack);
        if (other.Life <= 0)
        {
            MessageWindow.AppendMessage($"�����X�^�[�͓|�ꂽ!{other.Exp}�o���l����");
            other.Dead();
            //�U���̌��ʓG��|�����炻�̓G��Exp�����g��Exp���グ��
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
        MessageWindow.AppendMessage($"���x�����オ����! {Level - 1}����{Level}�ɂȂ���");
        MessageWindow.AppendMessage($"�̗͂�5������! �U���͂�1�オ����!");
    }

    public override void Damaged(int damage)
    {
        base.Damaged(damage);
        MessageWindow.AppendMessage($"�U�����ꂽ!{damage}�_���[�W�󂯂�");
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
            MessageWindow.AppendMessage($"�󕠃_���[�W!!�̗͂�{Life}��1������");
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
        else if (otherObject is Trap) //�ǉ�
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

        MessageWindow.AppendMessage($"㩂�!!");
        switch (trap.CurrentType)
        {
            case Trap.Type.LifeDown:
                Life -= trap.Value;
                MessageWindow.AppendMessage($"�̗͂�{trap.Value}������");

                if(Life <= 0)
                {
                    Dead();
                }
                break;
            case Trap.Type.FoodDown:
                Food -= trap.Value;
                MessageWindow.AppendMessage($"�󕠓x��{trap.Value}������");
                break;
            default: throw new System.NotImplementedException();
        }

        //㩂̓}�b�v����폜����
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
               
                MessageWindow.AppendMessage($"�񕜖򂾁I�̗͂�{treasure.Value}�񕜂���");
                break;
            case Treasure.Type.FoodUp:
                Food += treasure.Value;
                if (Food >= 100)
                {
                    Food = 100;
                    foodCount = 10;
                }
                MessageWindow.AppendMessage($"���ɂ��肾�I�󕠓x��{treasure.Value}�񕜂���");
                break;
            case Treasure.Type.Weapon:
                //�������̕���̍U���͂ɑ������킹��
                MessageWindow.AppendMessage($"{treasure.CurrentWeapon.Name}���������I");
                MessageWindow.AppendMessage($"���̕���̍U���͂�{treasure.CurrentWeapon}��!");
               
                var newWeapon = treasure.CurrentWeapon.Merge(CurrentWeapon);
                CurrentWeapon = newWeapon;
                break;
            default: throw new System.NotImplementedException();
        }

        //�󔠂��J������}�b�v����폜����
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
