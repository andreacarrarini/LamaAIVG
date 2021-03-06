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
    public sealed class cUtente : DataWrapper.Base.cBaseEntity<Utente>
    {

        protected override Utente Carica_Record(ref DbDataReader dr)
        {
            return new Utente()
            {
                descrizione = DrTo<string>(dr, "descrizione"),
                email = DrTo<string>(dr, "email"),
                facebook_key = DrTo<string>(dr, "facebook_key"),
                id_utente = DrTo<int>(dr, "id_utente"),
            };
        }

        protected override DbParameter[] Inserisci_Parametri(Utente entita)
        {
            return new DbParameter[] {
                cDB.NewPar("id_utente", entita.id_utente),
                cDB.NewPar("facebook_key", entita.facebook_key),
                cDB.NewPar("email", entita.email),
                cDB.NewPar("descrizione", entita.descrizione),
            };
        }

        protected override DbParameter[] Modifica_Parametri(Utente entita)
        {
            return new DbParameter[] {
                cDB.NewPar("descrizione", entita.descrizione),
            };
        }

        protected override DbParameter[] Ricerca_Parametri(Utente entita)
        {
            return new DbParameter[] {
                cDB.NewPar("id_utente", entita.id_utente),
                cDB.NewPar("email", entita.email),
            };
        }

        public cRisultatoSQL<Utente> RicercaByEmail(string email)
        {
            Utente u = null;

            var dr = cDB.EseguiSQLDataReader(getQuery("RicercaByEmail"), new DbParameter[] {
                cDB.NewPar("email", email)
            });

            try
            {
                while (dr.HasRows && dr.Read())
                    u = Carica_RecordSenzaAudit(ref dr);
            }
            catch (System.Exception ex1)
            {
                dr.Close();

                return new cRisultatoSQL<Utente>(ex1);
            }

            dr.Close();

            return new cRisultatoSQL<Utente>(u);
        }


    }
}