﻿/*
MIT License
Copyright (c) 2019 Team Lama: Carrarini Andrea, Cerrato Loris, De Cosmo Andrea, Maione Michele
Author: Michele Maione
Contributors: 
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using UnityEngine;
using UnityEngine.Networking;

public class BulletCollisionManager : NetworkBehaviour
{

    private GameObject explosion;
    private GameObject sand;
    private bool hasExploded = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            if (collision.gameObject.name.Equals("Arena"))
            {
                sand = Instantiate(sand, transform.position, Quaternion.Euler(-90, 0, 0));
            }
            else
            {
                explosion = Instantiate(explosion, transform.position, transform.rotation);

                var time = Time.time.ToString();

                explosion.name += time;
                hasExploded = true;
            }
        }
    }

    void Update()
    {
        if (hasExploded)
        {
            Destroy(explosion, 3.8f);
            Destroy(gameObject, 4.0f);
        }
    }


}