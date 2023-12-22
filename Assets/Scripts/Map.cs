using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

enum MassType
{
    Goal,
    Road,
    Wall,
    Player,
    Enemy,
    LifeTreasure,
    FoodTreasure,
    WeaponTreasure,
    Trap,
    FoodTrap,
}
[System.Serializable]
class MassData
{
    public GameObject Prefab;
    public MassType Type;
    public char MapChar;
    public bool IsRoad;
    public bool IsCharacter;
}
enum Direction
{
    North, //北
    South, //南
    East, //東
    West, //西
    stay,//待機
}

class Map : MonoBehaviour
{
    public class Mass
    {
        public MassType Type;
        public GameObject MassGameObject;
        public GameObject ExistObject;
    }

    public MassData[] massDataList;
    public Vector2 MassOffset = new Vector2(1, 1);

    public bool IsNowBuilding { get; private set; }
    public Vector2Int StartPos { get; set; } //マップ上の開始位置
    public Direction StartForward { get; set; } //プレイヤーの開始時の向き

    Dictionary<MassType, MassData> MassDataDict { get; set; }
    Dictionary<char, MassData> MapCharDict { get; set; }

    public Vector2Int MapSize { get; private set; }
    List<List<Mass>> Data { get; set; }
    public Mass this[int x, int y]
    {
        get => Data[y][x];
        set => Data[y][x] = value;
    }

    public MassData this[MassType type]
    {
        get => MassDataDict[type];
    }

    public List<string> MapData {  get; private set; }

    public void BuildMap(List<string> map)
    {
        InitMassData();

        var mapSize = Vector2Int.zero;
        Data = new List<List<Mass>>();
        foreach (var line in map)
        {
            var lineData = new List<Mass>();
            for (var i = 0; i < line.Length; ++i)
            {
                var ch = line[i];
                if (!MapCharDict.ContainsKey(ch))
                {
                    Debug.LogWarning("どのマスかわからない文字がマップデータに存在しています。 ch=" + ch);
                    ch = MapCharDict.First().Key; //始めのデータで代用
                }

                var massData = MapCharDict[ch];
                var mass = new Mass();
                var pos = CalcMapPos(i, Data.Count);
                
                if (massData.IsCharacter)
                {
                    mass.ExistObject = Object.Instantiate(massData.Prefab, transform);
                    var mapObject = mass.ExistObject.GetComponent<MapObjectBase>();
                    mapObject.SetPosAndForward(new Vector2Int(i, Data.Count), Direction.South);

                    //キャラクターの時は道も一緒に作成する
                    massData = this[MassType.Road];
                }
                mass.Type = massData.Type;
                mass.MassGameObject = Object.Instantiate(massData.Prefab, transform);
                mass.MassGameObject.transform.position = pos;
                lineData.Add(mass);
            }
            Data.Add(lineData);

            //マップサイズの設定
            mapSize.x = Mathf.Max(mapSize.x, line.Length);
            mapSize.y++;
        }
        MapSize = mapSize;
        MapData = map;
    }

    void InitMassData()
    {
        MassDataDict = new Dictionary<MassType, MassData>();
        MapCharDict = new Dictionary<char, MassData>();
        foreach (var massData in massDataList)
        {
            MassDataDict.Add(massData.Type, massData);
            MapCharDict.Add(massData.MapChar, massData);
        }
    }

    public Vector3 CalcMapPos(int x, int y)
    {
        var pos = Vector3.zero;
        pos.x = x * MassOffset.x;
        pos.z = y * MassOffset.y * -1;
        return pos;
    }

    public Vector3 CalcMapPos(Vector2Int pos) => CalcMapPos(pos.x, pos.y);

    public Vector2Int CalcDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return new Vector2Int(0, -1);
            case Direction.South: return new Vector2Int(0, 1);
            case Direction.East: return new Vector2Int(1, 0);
            case Direction.West: return new Vector2Int(-1, 0);
            case Direction.stay: return new Vector2Int(0, 0);
            default: throw new System.NotImplementedException();
        }
    }

    public (Mass mass, Vector2Int movedPos) GetMovePos(Vector2Int currentPos, Direction moveDir)
    {
        var offset = CalcDirection(moveDir);
        var movedPos = currentPos + offset;
        if (movedPos.x < 0 || movedPos.y < 0) return (null, currentPos);
        if (movedPos.y >= MapSize.y) return (null, currentPos);
        var line = Data[movedPos.y];
        if (movedPos.x >= line.Count) return (null, currentPos);

        var mass = line[movedPos.x];
        return (mass, movedPos);
    }

    public static Direction TurnRightDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.East;
            case Direction.South: return Direction.West;
            case Direction.East: return Direction.South;
            case Direction.West: return Direction.North;
            default: throw new System.NotImplementedException();
        }
    }
    public static Direction TurnLeftDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.West;
            case Direction.South: return Direction.East;
            case Direction.East: return Direction.North;
            case Direction.West: return Direction.South;
            default: throw new System.NotImplementedException();
        }
    }

    [System.Serializable]
    public class GenerateParam
    {
        public Vector2Int Size = new Vector2Int(20, 20);
        public int GoalMinDistance = 10;
        [Range(0, 1)] public float LimitMassPercent = 0.5f;
        [Range(0, 1)] public float RoadMassPercent = 0.7f;
        [Range(0, 1)] public float EnemyPercent = 0.2f;
        [Range(0, 1)] public float OtherPercent = 0;
    }

    public void DestoryMap()
    {
        for (var i = transform.childCount - 1; 0 <= i; --i)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void GenerateMap(GenerateParam generateParam)
    {
        InitMassData();

        var mapData = new List<List<char>>();
        var wallData = this[MassType.Wall];
        var line = new List<char>();
        for (var x = 0; x < generateParam.Size.x; ++x) { line.Add(wallData.MapChar); }
        for (var y = 0; y < generateParam.Size.y; ++y) { mapData.Add(new List<char>(line)); }

        PlacePlayerAndGoal(mapData, generateParam);
        PlaceMass(mapData, generateParam);

        BuildMap(mapData.Select(_l => _l.Aggregate("", (_s, _c) => _s + _c)).ToList());
    }

    void PlacePlayerAndGoal(List<List<char>> mapData, GenerateParam generateParam) 
    {
        var rnd = new System.Random();
        var playerPos = new Vector2Int(rnd.Next(generateParam.Size.x), rnd.Next(generateParam.Size.y));

        var goalPos = playerPos;
        do
        {
            goalPos = new Vector2Int(rnd.Next(generateParam.Size.x), rnd.Next(generateParam.Size.y));
        } while ((int)(playerPos - goalPos).magnitude < generateParam.GoalMinDistance);

        //プレイヤーとゴールを結ぶ
        //その際、中間点を通るようにしている。
        var centerPos = playerPos;
        do
        {
            centerPos = new Vector2Int(rnd.Next(generateParam.Size.x), rnd.Next(generateParam.Size.y));
        } while ((playerPos == centerPos) || goalPos == centerPos);

        var roadData = this[MassType.Road];
        DrawLine(mapData, playerPos, centerPos, roadData.MapChar);
        DrawLine(mapData, centerPos, goalPos, roadData.MapChar);

        var playerData = this[MassType.Player];
        var goalData = this[MassType.Goal];
        mapData[playerPos.y][playerPos.x] = playerData.MapChar;
        mapData[goalPos.y][goalPos.x] = goalData.MapChar;
    }
    void DrawLine(List<List<char>> mapData, Vector2Int start, Vector2Int end, char ch)
    {
        var pos = start;
        var vec = (Vector2)(end - start);
        vec.Normalize();
        var diff = Vector2.zero;
        while (pos != end)
        {
            diff += vec;
            if (Mathf.Abs(diff.x) >= 1)
            {
                var offset = diff.x > 0 ? 1 : -1;
                pos.x += offset;
                diff.x -= offset;
                mapData[pos.y][pos.x] = ch;
            }
            if (Mathf.Abs(diff.y) >= 1)
            {
                var offset = diff.y > 0 ? 1 : -1;
                pos.y += offset;
                diff.y -= offset;
                mapData[pos.y][pos.x] = ch;
            }
        }
    }
    void PlaceMass(List<List<char>> mapData, GenerateParam generateParam) 
    {
        var rnd = new System.Random();
        var massSum = generateParam.Size.x * generateParam.Size.y;
        var placeMassCount = massSum * generateParam.LimitMassPercent;
        var wallData = this[MassType.Wall];
        var roadData = this[MassType.Road];
        var EnemyData = this[MassType.Enemy];
        var placeMassKeys = MassDataDict.Keys.Where(_k => _k != MassType.Wall && _k != MassType.Player && _k != MassType.Goal && _k != MassType.Road && _k != MassType.Enemy).ToList();

        while (0 < placeMassCount)
        {
            var pos = Vector2Int.zero;
            var loopCount = placeMassCount * 10;
            do
            {
                if (loopCount-- < 0) break; //mapData[pos.y][pos.x] != wallData.MapChar条件の無限ループ回避用
                pos = new Vector2Int(rnd.Next(generateParam.Size.x), rnd.Next(generateParam.Size.y));
            } while (mapData[pos.y][pos.x] != wallData.MapChar);

            var t = rnd.Next(1000) / 1000f;
            var t2 = rnd.Next(1000) / 1000f;
            var t3 = rnd.Next(1000) / 1000f;
            if (t < generateParam.RoadMassPercent)
            {
                mapData[pos.y][pos.x] = roadData.MapChar;
            }
            else
            {
                if (t2 < generateParam.EnemyPercent)
                {
                    mapData[pos.y][pos.x] = EnemyData.MapChar;
                }
                else
                {
                    if (t3 < generateParam.OtherPercent)
                    {
                        var placeMassKey = placeMassKeys[rnd.Next(placeMassKeys.Count)];
                        var placeMass = this[placeMassKey];
                        mapData[pos.y][pos.x] = placeMass.MapChar;
                    }
                    else
                    {
                        mapData[pos.y][pos.x] = roadData.MapChar;
                    }
                }
            }

            
            placeMassCount--;
        }
    }
}