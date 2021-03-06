﻿/*
MIT License
Copyright (c) 2019 Team Lama: Carrarini Andrea, Cerrato Loris, De Cosmo Andrea, Maione Michele
Author: Maione Michele
Contributors: 
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using System.Data.Common;
using HypogeumDBW.DB.Tabelle;

namespace HypogeumDBW.DB
{
    public sealed class cPartecipanti : DataWrapper.Base.cBaseEntity<Partecipanti>
    {

        protected override Partecipanti Carica_Record(ref DbDataReader dr)
        {
            return new Partecipanti()
            {
                codice_unet = DrTo<string>(dr, "codice_unet"),
                id_utente = DrTo<int>(dr, "id_utente"),
                posizione = DrTo<int>(dr, "posizione"),
                punti = DrTo<int>(dr, "punti")
            };
        }

        protected override DbParameter[] Inserisci_Parametri(Partecipanti entita)
        {
            return new DbParameter[] {
                cDB.NewPar("codice_unet", entita.codice_unet),
                cDB.NewPar("id_utente", entita.id_utente),
                cDB.NewPar("posizione", entita.posizione),
                cDB.NewPar("punti", entita.punti),
            };
        }

        protected override DbParameter[] Modifica_Parametri(Partecipanti entita)
        {
            return new DbParameter[] {                
                cDB.NewPar("id_utente", entita.id_utente),
                cDB.NewPar("posizione", entita.posizione),
                cDB.NewPar("punti", entita.punti),
            };
        }

        protected override DbParameter[] Ricerca_Parametri(Partecipanti entita)
        {
            return new DbParameter[] {
                cDB.NewPar("codice_unet", entita.codice_unet),
            };
        }

    }
}