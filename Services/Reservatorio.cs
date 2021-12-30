﻿using Compass.CommomLibrary.Dadger;
using Compass.CommomLibrary.Decomp;
using Compass.CommomLibrary.HidrDat;
using Compass.CommomLibrary.Prevs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compass.ExcelTools;
using Compass.ExcelTools.Templates;
using System.Windows.Forms;

namespace Compass.Services {
    public class Reservatorio {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configH"></param>
        /// <param name="earmTargetLevel"></param>
        /// <param name="earmMax">Desconsiderado caso a meta seja em valor absoluto</param>
        /// <returns></returns>
        public static void SetUHBlock(ConfigH configH, double[] earmTargetLevel, double[] earmMax, List<Infosheet.Dados_Fixa> Fixa_UH) {
            var earmTarget = new double[earmTargetLevel.Length];

            if (earmTargetLevel.All(x => x <= 1)) {
                for (int x = 0; x < configH.index_sistemas.Count; x++) {
                    earmTarget[x] = earmTargetLevel[x] * earmMax[x];
                }
            } else if (earmTargetLevel.All(x => x <= 100)) {
                for (int x = 0; x < configH.index_sistemas.Count; x++) {
                    earmTarget[x] = (earmTargetLevel[x] / 100f) * earmMax[x];
                }

            } else {
                earmTarget = earmTargetLevel;
            }

            if (configH.baseDoc is Dadger) {
                //UhBlock uhResult = 
                buildReserv(configH, earmTarget,earmMax,Fixa_UH);
                //((Dadger)configH.baseDoc).BlocoUh = uhResult;
            } else if (configH.baseDoc is Compass.CommomLibrary.ConfhdDat.ConfhdDat) {
                buildReservNW(configH, earmTarget);
            }

            //return uhResult;
        }

        public static void SetUHBlock(ConfigH configH, double[] earmTargetLevel, double[] earmMax)
        {
            var earmTarget = new double[earmTargetLevel.Length];

            if (earmTargetLevel.All(x => x <= 1))
            {
                for (int x = 0; x < configH.index_sistemas.Count; x++)
                {
                    earmTarget[x] = earmTargetLevel[x] * earmMax[x];
                }
            }
            else if (earmTargetLevel.All(x => x <= 100))
            {
                for (int x = 0; x < configH.index_sistemas.Count; x++)
                {
                    earmTarget[x] = (earmTargetLevel[x] / 100f) * earmMax[x];
                }

            }
            else
            {
                earmTarget = earmTargetLevel;
            }

            if (configH.baseDoc is Dadger)
            {
                //UhBlock uhResult = 
                buildReserv(configH, earmTarget, earmMax);
                //((Dadger)configH.baseDoc).BlocoUh = uhResult;
            }
            else if (configH.baseDoc is Compass.CommomLibrary.ConfhdDat.ConfhdDat)
            {
                buildReservNW(configH, earmTarget);
            }

            //return uhResult;
        }


        static void buildReserv(ConfigH configH, double[] earmTarget , double[] earmMax, List<Infosheet.Dados_Fixa> Fixa_UH = null) {

            //UhBlock uhResult = new UhBlock();

         
            goalSeek(configH, earmTarget,earmMax, Fixa_UH);

            //ordenação deve obedecer a proveniente do deckBase.uh
            foreach (var uhBase in ((Dadger)configH.baseDoc).BlocoUh) {

                //var newUh = (UhLine)uhBase.Clone();
                var uhe = configH.usinas[uhBase.Usina];
                   
               uhBase.VolIniPerc = uhe.VolIni > 0 && uhe.VolUtil > 0 ? (float)Math.Round((uhe.VolIni / uhe.VolUtil) * 100f, 2) : 0f;
                   
                 
                //uhResult.Add(newUh);
            }
            
            

            //return uhResult;
        }
        static void buildReservNW(ConfigH configH, double[] earmTarget)
        {


            goalSeek(configH, earmTarget);


            //ordenação deve obedecer a proveniente do deckBase.uh
            foreach (var uh in (Compass.CommomLibrary.ConfhdDat.ConfhdDat)configH.baseDoc)
            {

                var uhe = configH.usinas[uh.Cod];
                uh.VolUtil = uhe.VolIni > 0 && uhe.VolUtil > 0 ? (float)Math.Round((uhe.VolIni / uhe.VolUtil) * 100f, 2) : 0f;
            }
        }

        public static Boolean Meta_Fixa_Uh(ConfigH configH, double[] earmTarget, double[] earm_Max, List<Infosheet.Dados_Fixa> Fixa_UH = null)
        {
            foreach(var uhBase in ((Dadger)configH.baseDoc).BlocoUh) {


                foreach(var item in Fixa_UH)
                {
                    if (uhBase.Usina == item.Posto)
                    {
                        uhBase.VolIniPerc = item.Volini;

                    }

                }                              

            }
            foreach(var usina in configH.Usinas)
            {
                foreach (var item in Fixa_UH)
                {
                    if (usina.Cod == item.Posto)
                    {
                        usina.VolIni = item.Volini * usina.VolUtil / 100f;

                    }

                }              
            }

            var earm_UH = configH.GetEarms();
         //   var earm_Max = earmMax;
            
            var desvio_max = Math.Max(Math.Abs((earmTarget[0]/earm_Max[0]) - (earm_UH[0] / earm_Max[0])), Math.Max(Math.Abs((earmTarget[1] / earm_Max[1]) - (earm_UH[1] / earm_Max[1])), Math.Max(Math.Abs((earmTarget[2] / earm_Max[2]) - (earm_UH[2] / earm_Max[2])), Math.Abs((earmTarget[3] / earm_Max[3]) - (earm_UH[3] / earm_Max[3])))));
            
            if (desvio_max > 0.0001)
            {
                return false;

            }
            else
            {
                return true;
            }
           
        }

        

        static void goalSeek(ConfigH configH, double[] earmTarget, double[] earmMax = null, List<Infosheet.Dados_Fixa> Fixa_UH = null) {


            var fatores = new double[configH.index_sistemas.Max(t => t.Item2) + 1];
            for (int i = 0; i < fatores.Length; i++) fatores[i] = 1;

            double erro = 100;
            double erroAnterior = 0;
            int itNumber = 0;
            int itMax = 100;
            Boolean desvio = true;

            do {

                //Travar Usinas Norte
                if(Fixa_UH != null)
                {
                    if (Fixa_UH.Count > 0)
                    {
                        desvio = false;
                        desvio = Meta_Fixa_Uh(configH, earmTarget, earmMax, Fixa_UH);
                        itMax = 1000;
                    }
                }
                

                //Fim da trava

                var earmCurrent = configH.GetEarms();

                erroAnterior = erro;
                erro = 0;

                for (int x = 0; x < configH.index_sistemas.Count; x++) {

                    var sis = configH.index_sistemas[x].Item2;

                    erro = erro + Math.Abs(earmCurrent[x] - earmTarget[x]);
                    var f = (earmTarget[x] / earmCurrent[x]);
                    fatores[sis] = f;
                }

                //se erro pequeno ou não houver grande variação parar iteração
                if ((erro < 2 || Math.Abs(erroAnterior - erro) < 1)&& desvio ==true)
                    break;


                //atualiza volumes e queda
                foreach (var uhe in configH.Usinas.Where(u => !u.IsFict && u.VolIni > 0)) {

                    if (!uhe.CodFicticia.HasValue) {
                        uhe.VolIni *= fatores[uhe.Mercado];
                    } else {
                        // se influenciar em outro sistema, levar em conta o fator do sistema afetado
                        // f = ( fs^3 * ff ) ^ (1/4)
                        var f = (float)Math.Pow(fatores[uhe.Mercado] *
                            fatores[uhe.Mercado] *
                            fatores[uhe.Mercado] *
                            fatores[configH.usinas[uhe.CodFicticia.Value].Mercado],
                            1d / 4d);
                        uhe.VolIni *= f;
                        //configH.usinas[uhe.CodFicticia.Value].atualizaQueda();
                    }
                }

            } while (++itNumber < itMax);

            if(itNumber >= itMax)
            {
                MessageBox.Show("Número Máximo de Iterações atingido");
            }
        }

    }
}

