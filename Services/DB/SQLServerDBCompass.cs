﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compass.Services.DB
{
    public class SQLServerDBCompass : SQLServerDB
    {
        public SQLServerDBCompass(string banco = "IPDO")
        {

            this.SetServidor("bdcompass.database.windows.net");
            //this.SetPorta(1433);
            this.SetUsuario("compass");
            this.SetSenha(this.GetPassword(this.GetUsuario()));
            this.SetDatabase(this.GetDatabase(banco));
        }

        private string GetPassword(string p_strUsuario)
        {
            switch (p_strUsuario)
            {
                case "compass":
                    return "cpas#9876";
                case "captura":
                    return "c@ptura9876";

                case "captura_read":
                    return "captur@leitur@";

                default:
                    return "";
            }
        }

        private string GetDatabase(string p_banco)
        {
            switch (p_banco)
            {
                case "IPDO":
                    return "IPDO";
                case "ESTUDO_PV":
                    return "ESTUDO_PV";



                default:
                    return "";
            }
        }
    }
}
