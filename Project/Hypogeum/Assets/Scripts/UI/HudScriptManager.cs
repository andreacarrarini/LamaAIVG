﻿/*
MIT License
Copyright (c) 2019 Team Lama: Carrarini Andrea, Cerrato Loris, De Cosmo Andrea, Maione Michele
Author: Carrarini Andrea
Contributors: Maione Michele
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HudScriptManager : MonoBehaviour
{

    //To change the speed in the speedometer
    private Text speedText;

    //To manage the team Health bar
    private Slider healthBar, hypeBar;

    private GameObject controlsClosed, controlsOpen, win, loss;

    private bool GameControlsVisibile = false;
    private bool HoAvutoIlTempoDiLeggere = true;

    //private int NumeroMassimoTeamVistiInCampo = 0;

    internal GeneralCar generalCar;

    public GM gm;

    void Start()
    {
        //HUD            
        controlsClosed = GameObject.Find("ControlsClosed");
        controlsOpen = GameObject.Find("ControlsOpen");
        controlsOpen.SetActive(GameControlsVisibile);

        speedText = GameObject.FindGameObjectWithTag("SpeedText").GetComponent<Text>();
        healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        hypeBar = GameObject.Find("HypeBar").GetComponent<Slider>();

        win = GameObject.FindGameObjectWithTag("Win");
        loss = GameObject.FindGameObjectWithTag("Loss");

        win.SetActive(false);
        loss.SetActive(false);

        setSpeed(0);
        setHealth(0, 0, 0);
        setHype(0);
    }

    void Update()
    {
        //HUD
        if (Input.GetKey(KeyCode.F1) && HoAvutoIlTempoDiLeggere)
        {
            HoAvutoIlTempoDiLeggere = false;

            controlsOpen.SetActive(!GameControlsVisibile);
            controlsClosed.SetActive(GameControlsVisibile);
            GameControlsVisibile = !GameControlsVisibile;

            StartCoroutine(TempoDiLettura());
        }

        CheckAnimaliMorti();
        setValues();
    }

    private void CheckAnimaliMorti()
    {
        if (gm.NumeroAnimaliVistiVivi > 0)
        {
            var hoPerso = false;
            var possoVincere = false;

            if (gm.NumeroAnimaliVistiVivi > 1)
                possoVincere = true;

            foreach (var a in gm.AnimaliMorti)
                if ((int)GB.Animal == a)
                {
                    hoPerso = true;
                    loss.SetActive(true);
                    break;
                }

            if (possoVincere && !hoPerso)
                if (gm.NumeroAnimaliVistiVivi == gm.AnimaliMorti.Count + 1)
                    win.SetActive(true);
        }
    }

    private IEnumerator TempoDiLettura()
    {
        yield return new WaitForSeconds(0.999f);
        StopCoroutine(TempoDiLettura());
        HoAvutoIlTempoDiLeggere = true;
    }

    private void setValues()
    {
        var l = generalCar?.Health ?? 0;
        var h = generalCar?.Hype ?? 0;
        var s = generalCar?.actualSpeed ?? 0;

        setHype(h);
        setHealth(0, l, l);
        setSpeed(s);
    }

    private void setHype(float value)
    {
        hypeBar.minValue = 0;
        hypeBar.maxValue = 1000;
        hypeBar.value = value;
    }

    private void setHealth(int min, float max, float value)
    {
        healthBar.minValue = min;

        if (max > healthBar.maxValue)
            healthBar.maxValue = max;

        healthBar.value = (loss.active ? 0 : value);
    }

    private void setSpeed(float value)
    {
        //value: m/s
        var realspeed = System.Convert.ToInt16(GB.ms_to_mph(value));

        if (realspeed == 0 || realspeed % 2 == 0)
            speedText.text = realspeed.ToString("0");
    }


}