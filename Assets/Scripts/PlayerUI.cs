using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class PlayerUI : MonoBehaviour
{
    [SerializeField] public Text LevelText;
    [SerializeField] public Text LifeText;
    [SerializeField] public Text MaxLifeText;
    //[SerializeField] public Text AttackText;
    //[SerializeField] public Text ExpText;
    [SerializeField] public Text FoodText;
    [SerializeField] public Text FloorText;
    [SerializeField] public Slider HpBar;
    [SerializeField] public Slider FoodBar;
    [SerializeField] public Slider ExpBar;


    public Player Player { get; private set; }

    public void Set(Player player)
    {
        Player = player;
    }

    private void Update()
    {
        if (Player == null) return;
        LevelText.text = Player.Level.ToString();
        LifeText.text = Player.Life.ToString();
        MaxLifeText.text = Player.MaxLife.ToString();
        //AttackText.text = Player.Attack.ToString();
        //ExpText.text = Player.Exp.ToString();
        FoodText.text = Player.Food.ToString();
        FloorText.text = Player.FloorNumber.ToString();
        HpBar.maxValue = Player.MaxLife;
        HpBar.value = Player.Life;
        FoodBar.value = Player.Food;
        ExpBar.maxValue = Player.MaxExp;
        ExpBar.value = Player.Exp;
    }
}