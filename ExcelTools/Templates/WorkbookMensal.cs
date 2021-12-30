﻿using Compass.CommomLibrary.Decomp;
using Compass.CommomLibrary.Prevs;
using Microsoft.Office.Interop.Excel;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compass.ExcelTools;

namespace Compass.ExcelTools.Templates
{
    public class WorkbookMensal : BaseWorkbook
    {
        public static bool TryCreate(Workbook xlWb, out WorkbookMensal w)
        {

            var ok = false;

            var names = new List<string>();
            foreach (Name name in xlWb.Names)
            {
                names.Add(name.Name);
            }

            ok =
                names.Contains("_DecompBase")
                ;

            if (ok)
            {
                w = new WorkbookMensal(xlWb);
                w.Wb.Worksheets["Geral"].Range["R11"] = GetPlanUEE();
            }
            else
                w = null;

            return ok;
        }
        public static string GetPlanUEE()
        {
            string cam = "";
            DateTime data = DateTime.Today;
            string buscPlan = @"C:\Files\Middle - Preço\06_APRESENTACOES_PMO";
            for (int i = 0; i < 3; i++)
            {
                cam = System.IO.Path.Combine(buscPlan, data.AddMonths(-i).ToString("yyyy"), data.AddMonths(-i).ToString("MM_yyyy"));
                if (Directory.Exists(cam))
                {
                    var nomePlan = Directory.GetFiles(cam).Where(x => x.Contains("Usinas_não_simuladas")).First().ToString();
                    return nomePlan;
                }
                i++;
            }
            string camPlan = System.IO.Path.Combine("C:\\Files\\Middle - Preço\\06_APRESENTACOES_PMO", data.ToString("yyyy"), data.ToString("MM_yyyy"));

            return cam;
        }
        public int Version
        {
            get
            {
                if (
                this.Names.ContainsKey("_NomeDoEstudo"))
                    return 4;
                else return 3;
            }
        }

        public string NomeDoEstudo
        {
            get
            {

                return this.Names["_NomeDoEstudo"].Text;
            }
        }

        public string DecompBase
        {
            get
            {
                return this.Names["_DecompBase"].Value;
            }
        }
        public string versao_Newave
        {
            get
            {
                return this.Wb.Worksheets["Geral"].Range("R8").Text;

            }
        }

        public string NewaveBase
        {
            get
            {
                return this.Names["_NewaveBase"].Value;
            }
        }

        public string CaminhoXml
        {
            get
            {
                return this.Names["_CaminhoXml"].Value;
            }
        }

        public Dictionary<int, int[]> Vazoes
        {
            get
            {

                var objarr = (object[,])Names["_vazoes"].Value2;

                var ret = new Dictionary<int, int[]>();
                var l1 = objarr.GetLength(0);
                var l2 = objarr.GetLength(1);

                for (int p = 1; p <= l1; p++)
                {


                    if (objarr[p, 1] != null)
                    {
                        var posto = Convert.ToInt32(objarr[p, 1]);
                        var vaz = new int[12];
                        for (int mes = 1; mes <= 12; mes++)
                        {
                            vaz[mes - 1] = Convert.ToInt32(objarr[p, mes + 1]);
                        }

                        ret.Add(posto, vaz);


                    }
                    else
                        break;
                }
                return ret;
            }
        }

        public Dictionary<int, double[]> Bloco_VE
        {
            get
            {

                var objarr = (object[,])Names["_ve"].Value2;

                var ret = new Dictionary<int, double[]>();
                var l1 = objarr.GetLength(0);
                var l2 = objarr.GetLength(1);

                for (int p = 1; p <= l1; p++)
                {


                    if (objarr[p, 1] != null)
                    {
                        var posto = Convert.ToInt32(objarr[p, 1]);
                        var vol = new double[12];
                        for (int mes = 1; mes <= 12; mes++)
                        {
                            vol[mes - 1] = Convert.ToDouble(objarr[p, mes + 1]);
                        }

                        ret.Add(posto, vol);


                    }
                    else
                        break;
                }
                return ret;
            }
        }

        public List<WorkSheetCen> Cenarios { get; private set; }

        public int[] ArvoreSegundoMes
        {
            get
            {

                var objarr = (object[,])Names["_arvore"].Value2;

                var ret = new int[12];

                for (int mes = 1; mes <= 12; mes++)
                {
                    ret[mes - 1] = Convert.ToInt32(objarr[1, mes]);
                }
                return ret;
            }
        }

        public int[] PostosArtificiais
        {
            get
            {

                if (!Names.ContainsKey("_partif")) return new int[0];

                var objarr = (object[,])Names["_partif"].Value2;

                var ret = new int[objarr.GetLength(0)];

                for (int p = 1; p <= ret.Length; p++)
                {
                    ret[p - 1] = Convert.ToInt32(objarr[p, 1]);
                }
                return ret;
            }
        }

        public string TipoMetaEarm
        {
            get
            {
                return Names["_TipoEarm"].Text;
            }
        }

        /// <summary>
        /// Value[0] = Inicial; Value[1-12] = final de Janeiro....Dezembro
        /// </summary>
        public Dictionary<int, double[]> Earm
        {
            get
            {

                if (!Names.ContainsKey("_earm"))
                {
                    return Cenarios.First().Earm;
                }

                var objarr = (object[,])Names["_earm"].Value2;

                var ret = new Dictionary<int, double[]>();
                var l1 = objarr.GetLength(0);
                var l2 = objarr.GetLength(1);

                for (int p = 1; p <= l1; p++)
                {


                    if (objarr[p, 1] != null)
                    {
                        var cod = Convert.ToInt32(objarr[p, 1]);
                        var earm = new double[13];
                        for (int mes = 1; mes <= 13; mes++)
                        {
                            earm[mes - 1] = Convert.ToDouble(objarr[p, mes + 1]);
                        }

                        ret.Add(cod, earm);


                    }
                    else
                        break;
                }
                return ret;
            }
        }

        public bool RodarVazoes
        {
            get
            {
                return Names["_rodarVazoes"].Value;
            }
        }

        List<RHE> rhes = null;
        public List<RHE> Rhes
        {
            get
            {
                if (rhes == null)
                {
                    rhes = new List<RHE>();

                    var ws = Names["_rhe"].Worksheet;
                    var row = Names["_rhe"].Row;
                    var col = Names["_rhe"].Column;

                    for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                    {
                        rhes.Add(new RHE(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 8]]));
                    }
                }

                return rhes;
            }
        }

        List<RHV> rhvs = null;
        public List<RHV> Rhvs
        {
            get
            {
                if (rhvs == null)
                {
                    rhvs = new List<RHV>();

                    var ws = Names["_rhv"].Worksheet;
                    var row = Names["_rhv"].Row;
                    var col = Names["_rhv"].Column;

                    for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                    {
                        rhvs.Add(new RHV(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 9]]));
                    }
                }

                return rhvs;
            }
        }

        List<RHQ> rhqs = null;
        public List<RHQ> Rhqs
        {
            get
            {
                if (rhqs == null)
                {
                    rhqs = new List<RHQ>();

                    var ws = Names["_rhq"].Worksheet;
                    var row = Names["_rhq"].Row;
                    var col = Names["_rhq"].Column;

                    for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                    {
                        rhqs.Add(new RHQ(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 11]]));
                    }
                }

                return rhqs;
            }
        }

        List<AC> acs = null;
        public List<AC> Acs
        {
            get
            {
                if (acs == null)
                {
                    acs = new List<AC>();

                    var ws = Names["_alteracao"].Worksheet;
                    var row = Names["_alteracao"].Row;
                    var col = Names["_alteracao"].Column;

                    for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                    {
                        acs.Add(new AC(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 4]]));
                    }
                }

                return acs;
            }
        }


        List<Compass.CommomLibrary.IRE> redat = null;
        public List<Compass.CommomLibrary.IRE> ReDats
        {
            get
            {
                if (Names.ContainsKey("_redat"))
                {
                    if (redat == null)
                    {
                        redat = new List<Compass.CommomLibrary.IRE>();




                        var ws = Names["_redat"].Worksheet;
                        var row = Names["_redat"].Row;
                        var col = Names["_redat"].Column;

                        for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                        {
                            redat.Add(new RE(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 7]]));
                        }
                    }


                    return redat;
                }
                else return null;
            }
        }

        List<Compass.CommomLibrary.IAGRIGNT> agrint = null;
        public List<Compass.CommomLibrary.IAGRIGNT> AgrintDats
        {
            get
            {
                if (Names.ContainsKey("_agrint"))
                {
                    if (agrint == null)
                    {
                        agrint = new List<Compass.CommomLibrary.IAGRIGNT>();

                        var ws = Names["_agrint"].Worksheet;
                        var row = Names["_agrint"].Row;
                        var col = Names["_agrint"].Column;

                        for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                        {
                            agrint.Add(new AGRINT(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 8]]));
                        }
                    }

                    return agrint;
                }
                else return null;
            }
        }






        List<Compass.CommomLibrary.IINTERCAMBIO> intercambios = null;
        public List<Compass.CommomLibrary.IINTERCAMBIO> Intercambios
        {
            get
            {
                if (Names.ContainsKey("_intercambio"))
                {
                    if (intercambios == null)
                    {
                        intercambios = new List<Compass.CommomLibrary.IINTERCAMBIO>();

                        var ws = Names["_intercambio"].Worksheet;
                        var row = Names["_intercambio"].Row;
                        var col = Names["_intercambio"].Column;

                        for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                        {
                            intercambios.Add(new INTERCAMBIO(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 8]]));
                        }
                    }

                    return intercambios;
                }
                else return null;
            }
        }

        List<Compass.CommomLibrary.IMERCADO> MercadosSis = null;
        public List<Compass.CommomLibrary.IMERCADO> MercadosSisdat
        {
            get
            {
                if (Names.ContainsKey("_MercadoEner"))
                {
                    if (MercadosSis == null)
                    {
                        MercadosSis = new List<Compass.CommomLibrary.IMERCADO>();

                        var ws = Names["_MercadoEner"].Worksheet;
                        var row = Names["_MercadoEner"].Row;
                        var col = Names["_MercadoEner"].Column;

                        for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                        {
                            MercadosSis.Add(new MERCADOSIS(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 4]]));
                        }
                    }

                    return MercadosSis;
                }
                else return null;
            }
        }

        List<Compass.CommomLibrary.IADTERM> adterm = null;
        public List<Compass.CommomLibrary.IADTERM> adtermdat
        {
            get
            {
                if (Names.ContainsKey("_adterm"))
                {
                    if (adterm == null)
                    {
                        adterm = new List<Compass.CommomLibrary.IADTERM>();

                        var ws = Names["_adterm"].Worksheet;
                        var row = Names["_adterm"].Row;
                        var col = Names["_adterm"].Column;

                        for (var r = row; !string.IsNullOrWhiteSpace(ws.Cells[r, col].Text); r++)
                        {
                            adterm.Add(new ADTERM(ws.Range[ws.Cells[r, col], ws.Cells[r, col + 4]]));
                        }
                    }

                    return adterm;
                }
                else return null;
            }
        }


        public WorkbookMensal(Workbook xlWb)
            : base(xlWb)
        {

            Cenarios = new List<WorkSheetCen>();

            foreach (Worksheet ws in Wb.Worksheets)
            {
                if (ws.Name.StartsWith("Hidrol"))
                {
                    Cenarios.Add(new WorkSheetCen(ws, this));
                }
            }


            Cenarios = Cenarios.OrderBy(x => x.NomeCenario).ToList();
        }


        public void AddResult(object label, object[,] result)
        {

            Worksheet ws = null;
            int row;
            int col;

            foreach (Worksheet _ws in this.Wb.Worksheets)
            {
                if (_ws.Name == "Resultado " + label)
                {
                    ws = _ws;
                    ws.UsedRange.Clear();
                }
            }
            if (ws == null)
            {
                ws = this.Wb.Worksheets.Add();
                ws.Name = "Resultado " + label;
            }

            row = 1;
            col = 1;

            ws.Range[ws.Cells[row + 1, col + 1], ws.Cells[row + result.GetLength(0), col + result.GetLength(1)]].Value2 = result;

            ((Microsoft.Office.Interop.Excel._Worksheet)ws).Activate();
        }

        public class RHE
        {
            public List<int> Usinas { get; set; }
            public List<Tuple<string, string>> Sistemas { get; set; }

            public int Freq_itaipu { get; set; }
            public int Restricao { get; set; }

            public int Mes { get; set; }
            public int? Estagio { get; set; }
            public double? LimInf1 { get; set; }
            public double? LimInf2 { get; set; }
            public double? LimInf3 { get; set; }
            public double? LimSup1 { get; set; }
            public double? LimSup2 { get; set; }
            public double? LimSup3 { get; set; }

            public RHE(Range rng)
            {


                Usinas = new List<int>();
                Sistemas = new List<Tuple<string, string>>();

                ((string)rng[1, 1].Text).Split(new string[] { ";", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(x =>
                {

                    int u;
                    if (int.TryParse(x.Trim(), out u))
                    {
                        Usinas.Add(u);
                    }
                    else if (x.StartsWith("#", StringComparison.OrdinalIgnoreCase)) Restricao = int.Parse(x.Substring(1));
                    else if (x.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                    {
                        var y = x.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        int us = int.Parse(y[0].Substring(1).Trim());
                        Usinas.Add(us);
                        Freq_itaipu = int.Parse(y[1].Trim());

                    }
                    else
                    {
                        var y = x.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        Sistemas.Add(new Tuple<string, string>(y[0].Trim(), y[1].Trim()));
                    }
                });

                if (rng[1, 2].Value is double) Mes = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) Estagio = (int)rng[1, 3].Value;

                if (rng[1, 4].Value is double) LimInf1 = rng[1, 4].Value;
                if (rng[1, 5].Value is double) LimSup1 = rng[1, 5].Value;
                if (rng[1, 6].Value is double) LimInf2 = rng[1, 6].Value;
                if (rng[1, 7].Value is double) LimSup2 = rng[1, 7].Value;
                if (rng[1, 8].Value is double) LimInf3 = rng[1, 8].Value;
                if (rng[1, 9].Value is double) LimSup3 = rng[1, 9].Value;
            }
        }

        public class RHV
        {
            public int Usina { get; set; }
            public int Restricao { get; set; }

            public int Mes { get; set; }
            public int Ano { get; set; }
            public int? Estagio { get; set; }
            public double? LimInf { get; set; }
            public double? LimSup { get; set; }
            public bool exclui { get; set; }



            public RHV(Range rng)
            {
                if (rng[1, 1].Value is double) Usina = (int)rng[1, 1].Value;
                else if (((string)rng[1, 1].Text).StartsWith("#", StringComparison.OrdinalIgnoreCase)) Restricao = int.Parse(((string)rng[1, 1].Text).Substring(1));

                if (rng[1, 2].Value is double) Mes = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) Ano = (int)rng[1, 3].Value;
                if (rng[1, 4].Value is double) Estagio = (int)rng[1, 4].Value;
                if (rng[1, 5].Value is double) LimInf = rng[1, 5].Value;
                if (rng[1, 6].Value is double) LimSup = rng[1, 6].Value;

                if (((string)rng[1, 7].Text).ToUpper() == "E")
                {
                    exclui = true;
                }
                else
                {
                    exclui = false;
                }
            }
        }

        public class RHQ
        {
            public int Usina { get; set; }
            public int Restricao { get; set; }
            public int Mes { get; set; }
            public int Ano { get; set; }
            public int? Estagio { get; set; }
            public double? LimInf1 { get; set; }
            public double? LimInf2 { get; set; }
            public double? LimInf3 { get; set; }
            public double? LimSup1 { get; set; }
            public double? LimSup2 { get; set; }
            public double? LimSup3 { get; set; }
            public string minemonico { get; set; }
            public bool exclui { get; set; }

            public RHQ(Range rng)
            {

                if (rng[1, 1].Value is double) Usina = (int)rng[1, 1].Value;
                else if (((string)rng[1, 1].Text).StartsWith("#", StringComparison.OrdinalIgnoreCase)) Restricao = int.Parse(((string)rng[1, 1].Text).Substring(1));


                if (rng[1, 2].Value is double) Mes = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) Ano = (int)rng[1, 3].Value;
                if (rng[1, 4].Value is double) Estagio = (int)rng[1, 4].Value;

                if (rng[1, 5].Value is double) LimInf1 = rng[1, 5].Value;
                if (rng[1, 7].Value is double) LimInf2 = rng[1, 7].Value;
                if (rng[1, 9].Value is double) LimInf3 = rng[1, 9].Value;
                if (rng[1, 6].Value is double) LimSup1 = rng[1, 6].Value;
                if (rng[1, 8].Value is double) LimSup2 = rng[1, 8].Value;
                if (rng[1, 10].Value is double) LimSup3 = rng[1, 10].Value;
                string tipo = rng[1, 11].Value;
                if (tipo == "")
                {
                    minemonico = "QDEF";
                }
                else
                {
                    minemonico = ((string)rng[1, 11].Text).ToUpper();

                }

                if (((string)rng[1, 12].Text).ToUpper() == "E")
                {
                    exclui = true;
                }
                else
                {
                    exclui = false;
                }

            }
        }

        public class AC
        {
            public int Usina { get; set; }
            public string Mnemonico { get; set; }
            public int Mes { get; set; }
            public object Valor1 { get; set; }
            public object Valor2 { get; set; }
            public object Valor3 { get; set; }

            public AC(Range rng)
            {

                if (rng[1, 1].Value is double) Usina = (int)rng[1, 1].Value;

                Mnemonico = rng[1, 2].Text;

                if (rng[1, 3].Value is double) Mes = (int)rng[1, 3].Value;
                else Mes = 0;

                Valor1 = rng[1, 4].Value;
                Valor2 = rng[1, 5].Value;
                Valor3 = rng[1, 6].Value;
            }
        }

        public class RE : Compass.CommomLibrary.IRE
        {
            public List<int> Usinas { get; set; }
            public int MesEstudo { get; set; }

            public int MesIni { get; set; }
            public int AnoIni { get; set; }

            public int MesFim { get; set; }
            public int AnoFim { get; set; }


            public int Patamar { get; set; }
            public double Restricao { get; set; }


            public RE(Range rng)
            {
                Usinas = new List<int>();
                ((string)rng[1, 1].Text).Split(new string[] { ";", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(x =>
                {

                    int u;
                    if (int.TryParse(x.Trim(), out u))
                    {
                        Usinas.Add(u);
                    }
                });

                if (rng[1, 2].Value is double) MesEstudo = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) MesIni = (int)rng[1, 3].Value;
                if (rng[1, 4].Value is double) AnoIni = (int)rng[1, 4].Value;
                if (rng[1, 5].Value is double) MesFim = (int)rng[1, 5].Value;
                if (rng[1, 6].Value is double) AnoFim = (int)rng[1, 6].Value;
                if (rng[1, 7].Value is double) Patamar = (int)rng[1, 7].Value;
                if (rng[1, 8].Value is double) Restricao = rng[1, 8].Value;

            }
        }

        public class AGRINT : Compass.CommomLibrary.IAGRIGNT
        {

            public int MesEstudo { get; set; }

            public int MesIni { get; set; }
            public int AnoIni { get; set; }

            public int MesFim { get; set; }
            public int AnoFim { get; set; }

            public double RestricaoP1 { get; set; }
            public double RestricaoP2 { get; set; }
            public double RestricaoP3 { get; set; }
            public List<Tuple<int, int>> Intercambios { get; set; }

            public AGRINT(Range rng)
            {
                Intercambios = new List<Tuple<int, int>>();
                ((string)rng[1, 1].Text).Split(new string[] { ";", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(x =>
                {
                    if (x.Split('-').Length == 2 &&
                    int.TryParse(x.Split('-')[0], out int A) && int.TryParse(x.Split('-')[1], out int B))
                    {
                        Intercambios.Add(new Tuple<int, int>(A, B));
                    }

                });

                if (rng[1, 2].Value is double) MesEstudo = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) MesIni = (int)rng[1, 3].Value;
                if (rng[1, 4].Value is double) AnoIni = (int)rng[1, 4].Value;
                if (rng[1, 5].Value is double) MesFim = (int)rng[1, 5].Value;
                if (rng[1, 6].Value is double) AnoFim = (int)rng[1, 6].Value;
                if (rng[1, 7].Value is double) RestricaoP1 = rng[1, 7].Value;
                if (rng[1, 8].Value is double) RestricaoP2 = rng[1, 8].Value;
                if (rng[1, 8].Value is double) RestricaoP3 = rng[1, 9].Value;
            }
        }

        public class ADTERM : Compass.CommomLibrary.IADTERM
        {
            public int Mes { get; set; }
            public double RestricaoP1 { get; set; }
            public double Usina { get; set; }
            public double RestricaoP2 { get; set; }
            public double RestricaoP3 { get; set; }

            public List<int> Usinas { get; set; }

            public ADTERM(Range rng)
            {
                if (rng[1, 1].Value is double) Usina = (int)rng[1, 1].Value;
                if (rng[1, 2].Value is double) Mes = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) RestricaoP1 = rng[1, 3].Value;
                if (rng[1, 4].Value is double) RestricaoP2 = rng[1, 4].Value;
                if (rng[1, 5].Value is double) RestricaoP3 = rng[1, 5].Value;
            }

        }

        public class MERCADOSIS : Compass.CommomLibrary.IMERCADO
        {
            public double SubMercado { get; set; }
            public double AnoIni { get; set; }
            public double MesEstudo { get; set; }

            public double Mes { get; set; }

            public double Carga { get; set; }

            public MERCADOSIS(Range rng)
            {
                if (rng[1, 1].Value is double) SubMercado = (int)rng[1, 1].Value;
                if (rng[1, 2].Value is double) MesEstudo = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) Mes = rng[1, 3].Value;
                if (rng[1, 4].Value is double) AnoIni = rng[1, 4].Value;
                if (rng[1, 5].Value is double) Carga = rng[1, 5].Value;
            }

        }


        public class INTERCAMBIO : Compass.CommomLibrary.IINTERCAMBIO
        {

            public int MesEstudo { get; set; }

            public int MesIni { get; set; }
            public int AnoIni { get; set; }

            public int MesFim { get; set; }
            public int AnoFim { get; set; }

            public double RestricaoP1 { get; set; }
            public double RestricaoP2 { get; set; }
            public double RestricaoP3 { get; set; }
            public Tuple<int, int> Intercambios { get; set; }

            public INTERCAMBIO(Range rng)
            {

                var x = ((string)rng[1, 1].Text);

                if (x.Split('-').Length == 2 &&
                int.TryParse(x.Split('-')[0], out int A) && int.TryParse(x.Split('-')[1], out int B))
                {
                    Intercambios = new Tuple<int, int>(A, B);
                }

                if (rng[1, 2].Value is double) MesEstudo = (int)rng[1, 2].Value;
                if (rng[1, 3].Value is double) MesIni = (int)rng[1, 3].Value;
                if (rng[1, 4].Value is double) AnoIni = (int)rng[1, 4].Value;
                if (rng[1, 5].Value is double) MesFim = (int)rng[1, 5].Value;
                if (rng[1, 6].Value is double) AnoFim = (int)rng[1, 6].Value;
                if (rng[1, 7].Value is double) RestricaoP1 = rng[1, 7].Value;
                if (rng[1, 8].Value is double) RestricaoP2 = rng[1, 8].Value;
                if (rng[1, 8].Value is double) RestricaoP3 = rng[1, 9].Value;
            }
        }

        public int MesesAvancar
        {
            get
            {
                return (int)this.Wb.Worksheets["Geral"].Range("R5").Value;
            }
        }

        public string NewaveOrigem
        {
            get
            {
                return this.Wb.Worksheets["Geral"].Range("C4").Value;
            }
        }

        public string ExecutavelNewave
        {
            get             //home/compass/sacompass/previsaopld
            {
                return "/home/compass/sacompass/previsaopld/cpas_ctl_common/scripts/newave" +
                //return "/home/compass/sacompass/previsaopld/cpas_ctl_common/scripts/newave" +
                    this.Wb.Worksheets["Geral"].Range("R8").Text
                    + "Cons.sh";
            }
        }
    }

    public class WorkSheetCen : BaseWorksheet
    {


        WorkbookMensal wb;
        public WorkSheetCen(Worksheet ws, WorkbookMensal wb) : base(ws) { this.wb = wb; }

        public string NomeCenario { get { return base.SheetName; } }

        public string NomeDoEstudo
        {
            get
            {
                if (this.Names.ContainsKey("_nome"))
                    return this.Names["_nome"].Text;
                else
                    return wb.Names["_NomeDoEstudo"].Text;
            }
        }

        public Dictionary<int, double[]> Earm
        {
            get
            {


                dynamic objarr;
                if (this.Names.ContainsKey("_earm"))
                    objarr = (object[,])Names["_earm"].Value2;
                else
                    objarr = (object[,])wb.Names["_earm"].Value2;

                var ret = new Dictionary<int, double[]>();
                var l1 = objarr.GetLength(0);
                var l2 = objarr.GetLength(1);

                for (int p = 1; p <= l1; p++)
                {


                    if (objarr[p, 1] != null)
                    {
                        var cod = Convert.ToInt32(objarr[p, 1]);
                        var earm = new double[13];
                        for (int mes = 1; mes <= 13; mes++)
                        {
                            earm[mes - 1] = Convert.ToDouble(objarr[p, mes + 1]);
                        }

                        ret.Add(cod, earm);


                    }
                    else
                        break;
                }
                return ret;
            }
        }

        public Dictionary<int, int[]> Vazoes
        {
            get
            {

                dynamic objarr;
                if (this.Names.ContainsKey("_vazoes"))
                    objarr = (object[,])Names["_vazoes"].Value2;
                else
                    objarr = (object[,])wb.Names["_vazoes"].Value2;

                var ret = new Dictionary<int, int[]>();
                var l1 = objarr.GetLength(0);
                var l2 = objarr.GetLength(1);

                for (int p = 1; p <= l1; p++)
                {


                    if (objarr[p, 1] != null)
                    {
                        var posto = Convert.ToInt32(objarr[p, 1]);
                        var vaz = new int[12];
                        for (int mes = 1; mes <= 12; mes++)
                        {
                            vaz[mes - 1] = Convert.ToInt32(objarr[p, mes + l2 - 12]);
                        }

                        ret.Add(posto, vaz);


                    }
                    else
                        break;
                }
                return ret;
            }
        }

    }
}



