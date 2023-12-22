using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapSceneManager : MonoBehaviour
{
    [TextArea(3, 15)]
    public string mapData =
      "000P\n" +
      "0+++\n" +
      "0000\n" +
      "+++0\n" +
      "G000";

    public GameObject GameOver;
    public GameObject NextSelect;
    public GameObject MainMenu;
    public GameObject SubMenu;
    public GameObject MiniMap;

    public bool IsAutoGenerate = true;
    [SerializeField] Map.GenerateParam GenerateParam;
    public void GenerateMap()
    {
        var map = GetComponent<Map>();
        map.DestoryMap();
        map.GenerateMap(GenerateParam);
    }
    void Awake()
    {
        GameOver.SetActive(false);
        NextSelect.SetActive(false);
        MainMenu.SetActive(false);
        SubMenu.SetActive(false);
        MiniMap.SetActive(true);

        var map = GetComponent<Map>();
        var saveData = SaveData.Recover();
        if(saveData != null)
        {
            try
            {
                map.BuildMap(saveData.MapData);

                mapData = saveData.MapData.Aggregate("", (_s, _c) => _s + _c + '\n');
                var player = Object.FindObjectOfType<Player>();
                player.Recover(saveData);
                return;
            }
            catch (System.Exception e)
            {//�G���[�����B�����Ɏ��s�����畁�ʂ̃}�b�v�������s���l�ɂ��Ă���
                Debug.LogWarning($"Fail to Recover SaveData...");
            }
        }

        if (IsAutoGenerate)//�ǉ�
        {
            map.GenerateMap(GenerateParam); //�ǉ�
        }
        else
        {
            var lines = mapData.Split('\n').ToList();
            map.BuildMap(lines);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    GenerateMap();
        //}
    }
}