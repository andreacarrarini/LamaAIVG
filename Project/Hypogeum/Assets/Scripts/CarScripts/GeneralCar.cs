﻿/*
MIT License
Copyright (c) 2019 Team Lama: Carrarini Andrea, Cerrato Loris, De Cosmo Andrea, Maione Michele
Author: Maione Michele
Contributors:
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using UnityEngine;
using UnityEngine.Networking;

public class GeneralCar : NetworkBehaviour
{

    //settata dal server
    [SyncVar]
    public string MyCannonName;
    //settata dal server


    //settata dal client
    [SyncVar]
    public uint Hype = 0;

    [SyncVar]
    public float Health = 1000;

    [SyncVar]
    public float actualSpeed = 0;
    //settata dal client


    //settate dal editor Unity
    public GB.EAnimal AnimalType;

    [Range(0, 250)]
    public int Speed;

    [Range(0, 10)]
    public int Defense;

    [Range(0, 10)]
    public int Agility;

    [Range(0, 10)]
    public int Attack;

    [Range(1f, 50f)]
    public float Accellerazione;

    [Range(5f, 55f)]
    public float maxSteeringAngle;

    [Range(100f, 200f)]
    public float maxTorque;
    //settate dal editor Unity


    //calcolate
    public float brakingTorque => maxTorque * 100;
    //calcolate

}