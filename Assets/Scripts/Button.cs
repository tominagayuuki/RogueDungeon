using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



public class Button : MonoBehaviour
{
    public void NextMap()
    {
        var player = Object.FindObjectOfType<Player>();
        player.NextMap();
    }

    public void StayMap()
    {
        var player = Object.FindObjectOfType<Player>();
        player.ChoseOff();
    }

    public void MenuClose()
    {
        var player = Object.FindObjectOfType<Player>();
        player.MenuClose();
    }

    public void SubMenuOpen()
    {
        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        mapManager.SubMenu.SetActive(true);
    }

    public void SubMenuClose()
    {
        var mapManager = Object.FindObjectOfType<MapSceneManager>();
        mapManager.SubMenu.SetActive(false);
    }


    public void ChangeSceneNewMap()
    {
        SceneManager.LoadScene("MapScene");
        SaveData.Destoy();
    }
    public void ChangeSceneLoadMap()
    {
        SceneManager.LoadScene("MapScene");
    }
    public void ChangeSceneTaitle()
    {
        SceneManager.LoadScene("TitleScene");
    }



    //public Fade fade;

    //public void outs()
    //{
    //    fade.CallCoroutine();
    //}
}
