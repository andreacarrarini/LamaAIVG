﻿/*
MIT License
Copyright (c) 2019 Team Lama: Carrarini Andrea, Cerrato Loris, De Cosmo Andrea, Maione Michele
Author: Maione Michele
Contributors: 
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using HypogeumDBW.DB.DataWrapper.Tabelle;
using System.Collections.Generic;

namespace HypogeumDBW.DB.Tabelle
{
    public sealed class Partecipanti : TabellaBase
    {
        public string codice_unet { get; set; }

        public int id_utente { get; set; }
        public int punti { get; set; }
        public int posizione { get; set; }

        public Utente utente
        {
            get
            {
                var classeUtente = new cUtente();

                var R = classeUtente.Ricerca(new Utente()
                {
                    id_utente = id_utente
                });

                foreach (var u in R.Risultato)
                    return u;

                return null;
            }
        }


    }
}