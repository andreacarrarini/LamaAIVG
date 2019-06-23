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

public class GM : NetworkBehaviour
{
    
    void FixedUpdate()
    {
        if (isServer)
        {
            var cars = GameObject.FindGameObjectsWithTag("car");
            var weapons = GameObject.FindGameObjectsWithTag("Cannon");

            if (cars != null && weapons != null)
                foreach (var car in cars)
                {
                    var gc = car.GetComponent<GeneralCar>();

                    if (gc.MyCannon == null)
                        foreach (var weapon in weapons)
                        {
                            var sh = weapon.GetComponent<Shooting>();

                            if (sh.TipoDiArma == gc.AnimalType && sh.Car == null)
                            {
                                //matched!
                                gc.MyCannon = weapon;
                                sh.Car = car;
                            }
                        }
                }
        }
    }


}