using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class characterScript : MonoBehaviour
{
    public TMP_InputField nameinput;
    public Slider levelSlider;
    public Slider healthSlider;

    public character ReturnCharacter()
    {
        return new character(nameinput.text, healthSlider.value, levelSlider.value);
    }
    public void setUi(character character)
    {
        nameinput.text = character.cname;
        healthSlider.value = character.health;
        levelSlider.value = character.level;
    }
}

public class character
{
    public string cname;
    public float health;
    public float level;
   
    public character(string cname , float health , float level)
    {
        this.cname = cname;
        this.health = health;
        this.level = level;
    }
}
