﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compass.CommomLibrary.EntdadosDat
{
    public class ItBlock : BaseBlock<ItLine>
    {




    }

    public class ItLine : BaseLine
    {
        public string IdBloco { get { return this[0].ToString(); } set { this[0] = value; } }
        public int NumReserv { get { return (int)this[1]; } set { this[1] = value; } }

        public override BaseField[] Campos { get { return ItCampos; } }

        static readonly BaseField[] ItCampos = new BaseField[] {
                new BaseField(1  , 2 ,"A2"    , "IdBloco"),//
                new BaseField(5  , 6 ,"I2"    , "ReservatorioNum"),//
                new BaseField(10  , 24 ,"E15.0"    , "Poli1"),//
                new BaseField(25  , 39 ,"E15.0"    , "Poli2"),//
                new BaseField(40  , 54 ,"E15.0"    , "Poli3"),//
                new BaseField(55  , 69 ,"E15.0"    , "Poli4"),//
                new BaseField(70  , 84 ,"E15.0"    , "Poli5"),//


            };
    }
}
