﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir;
            //if (args.Length > 0 && Directory.Exists(args[0]))
            //{
            //    dir = args[0];
            //}
            //else if (args.Length > 0 && File.Exists(args[0]))
            //{
            //    dir = Path.GetDirectoryName(args[0]);
            //}
            //else if (args.Length == 0)
            //{
            dir = System.Environment.CurrentDirectory;
            //}
            //else
            //{
            //    Console.WriteLine("Não encontrado");
            //    return;
            //}




            if (args[0] == "RV0")
            {
                Altera_Dadgnl_Sem(dir);
                //Altera_Dadgnl_Men(dir);

            }

            else if (args[0] == "cotvol")
            {

                CalcJirauStoAnto(dir);
            }

            else if (args[0] == "dadgnl")
            {
                Altera_Dadgnl_Men(dir);
            }
            else if (args[0] == "adterm")
            {
                Altera_Adterm(dir);
            }
            else if (args[0] == "flexTucurui")
            {
                flexibilizaTucurui(dir);
            }
            else
            {
                int nivel = 1; //1 = hqs, res, 2 = 1 + TI , 3 = 2 + excessoes

                if (args.Length > 0 && int.TryParse(args[0].Trim(), out int n) && (n >= 1 && n <= 3))
                {
                    nivel = n;
                }



                if (DeckFactory.CreateDeck(dir) is ConsoleApp1.Decomp.Deck deck)
                {

                    var fi = Directory.GetFiles(dir, "inviab_unic.*", SearchOption.TopDirectoryOnly).FirstOrDefault();

                    if (fi != null)
                    {
                        var inviab = (Inviab.Inviab)DocumentFactory.Create(fi);

                        var fname = System.IO.Path.GetFileNameWithoutExtension(fi);
                        System.IO.File.Copy(fi, System.IO.Path.Combine(dir, fname + DateTime.Now.ToString("_yyyyMMddHHmmss.bak")));

                        DesfazerInviabilidades(deck, inviab, nivel);
                        deck[ConsoleApp1.Decomp.DeckDocument.dadger].Document.SaveToFile(createBackup: true);
                    }
                    else
                        Console.WriteLine("Arquivo inviab_unic.xxx não encontrado.");
                }
                else
                    Console.WriteLine("Não foi possível ler o deck");
            }
        }


        /// <param name="deck"></param>
        /// <param name="inviabilidades"></param>
        /// <param name="nivel">1 = hqs, res, 2 = 1 + TI , 3 = 2 + excessoes</param>
        public static void DesfazerInviabilidades(ConsoleApp1.Decomp.Deck deck, Inviab.Inviab inviabilidades, int nivel)
        {
            var dadger = deck[ConsoleApp1.Decomp.DeckDocument.dadger].Document as Dadger.Dadger;
            var hidr = deck[ConsoleApp1.Decomp.DeckDocument.hidr].Document as HidrDat.HidrDat;

            var q =
                        from inv in inviabilidades.Iteracao
                        group inv by new { inv.Estagio, inv.RestricaoViolada } into invG
                        select invG.OrderByDescending(x => x.Violacao).First();

            foreach (var inviab in q.OrderByDescending(x => x.Estagio))
            {
              

                if (inviab.TipoRestricao == "RHE" || inviab.TipoRestricao == "RHQ" || inviab.TipoRestricao == "RHV")
                {
                    IEnumerable<BaseLine> rs;
                    if (inviab.TipoRestricao == "RHE")
                        rs = dadger.BlocoRhe.Where(x => x.Restricao == inviab.CodRestricao);
                    else if (inviab.TipoRestricao == "RHQ")
                        rs = dadger.BlocoRhq.Where(x => x.Restricao == inviab.CodRestricao);
                    else if (inviab.TipoRestricao == "RHV")
                        rs = dadger.BlocoRhv.Where(x => x.Restricao == inviab.CodRestricao);
                    else
                        continue;

                    if (rs.Count() > 0)
                    {
                        dynamic le;
                        dynamic le256;
                        if (inviab.TipoRestricao == "RHE")
                        {
                            var ls = rs.Where(x => x is Dadger.LuLine).Select(x => (Dadger.LuLine)x);
                            le = ls.Where(x => x.Estagio <= inviab.Estagio).OrderByDescending(x => x.Estagio).FirstOrDefault();
                        }
                        else if (inviab.TipoRestricao == "RHQ")
                        {

                            if (nivel < 3)//excecoes   pula essas condicoes caso nivel < 2
                            {
                                if (rs.Where(x => x is Dadger.CqLine).Select(x => x as Dadger.CqLine)
                                    .All(x =>
                                       x.Usina == 251 // SERRA DA MESA
                                    || x.Usina == 156 // TRES MARIAS
                                    || x.Usina == 169 // SOBRADINHO
                                    || x.Usina == 178 // XINGO

                                    ))
                                    continue;
                            }

                            var ls = rs.Where(x => x is Dadger.LqLine).Select(x => (Dadger.LqLine)x);
                            le = ls.Where(x => x.Estagio <= inviab.Estagio).OrderByDescending(x => x.Estagio).FirstOrDefault();
                        }
                        else if (inviab.TipoRestricao == "RHV")
                        {
                            if (inviab.CodRestricao == 43)//essa restrição não pode ser alterada
                            {
                                continue;
                            }
                            var ls = rs.Where(x => x is Dadger.LvLine).Select(x => (Dadger.LvLine)x);
                            le = ls.Where(x => x.Estagio <= inviab.Estagio).OrderByDescending(x => x.Estagio).FirstOrDefault();
                        }
                        else continue;

                        if (inviab.TipoRestricao == "RHQ" && (inviab.CodRestricao == 87 || inviab.CodRestricao == 149 || inviab.CodRestricao == 159 || inviab.CodRestricao == 102 || inviab.CodRestricao == 125))
                        {
                            dynamic le125;
                            dynamic le159;
                            IEnumerable<BaseLine> rs125;
                            IEnumerable<BaseLine> rs159;

                            rs125 = dadger.BlocoRhq.Where(x => x.Restricao == 125);
                            rs159 = dadger.BlocoRhq.Where(x => x.Restricao == 159);

                            if (rs125.Count() > 0)
                            {
                                var ls125 = rs125.Where(x => x is Dadger.LqLine).Select(x => (Dadger.LqLine)x);
                                le125 = ls125.Where(x => x.Estagio <= inviab.Estagio).OrderByDescending(x => x.Estagio).FirstOrDefault();

                                if (le125.Estagio < inviab.Estagio)
                                {

                                    var nle125 = le125.Clone();
                                    nle125.Estagio = inviab.Estagio;
                                    dadger.BlocoRhq.Add(nle125);
                                    le125 = nle125;
                                }
                                //
                                le125[3] = 90;
                                le125[5] = 90;
                                le125[7] = 90;
                                le125[4] = 99999;
                                le125[6] = 99999;
                                le125[8] = 99999;

                                //continue;
                            }

                            if (rs159.Count() > 0)
                            {
                                var ls159 = rs159.Where(x => x is Dadger.LqLine).Select(x => (Dadger.LqLine)x);
                                le159 = ls159.Where(x => x.Estagio <= inviab.Estagio).OrderByDescending(x => x.Estagio).FirstOrDefault();

                                if (le159.Estagio < inviab.Estagio)
                                {

                                    var nle159 = le159.Clone();
                                    nle159.Estagio = inviab.Estagio;
                                    dadger.BlocoRhq.Add(nle159);
                                    le159 = nle159;
                                }
                                //
                                if (nivel < 3)
                                {
                                    le159[4] = 160;
                                    le159[6] = 160;
                                    le159[8] = 160;
                                }
                                else
                                {
                                    le159[4] = 99999;
                                    le159[6] = 99999;
                                    le159[8] = 99999;
                                }
                                

                            }
                            continue;

                        }

                        if (inviab.TipoRestricao == "RHQ" && inviab.CodRestricao == 161)
                        {
                            IEnumerable<BaseLine> rs162;
                            dynamic le162;
                            rs162 = dadger.BlocoRhq.Where(x => x.Restricao == 162);

                            if (rs162.Count() > 0)
                            {
                                var ls162 = rs162.Where(x => x is Dadger.LqLine).Select(x => (Dadger.LqLine)x);
                                le162 = ls162.Where(x => x.Estagio <= inviab.Estagio).OrderByDescending(x => x.Estagio).FirstOrDefault();

                                if (le162.Estagio < inviab.Estagio)
                                {

                                    var nle162 = le162.Clone();
                                    nle162.Estagio = inviab.Estagio;
                                    dadger.BlocoRhq.Add(nle162);
                                    le162 = nle162;
                                }
                                //
                                var p = 2 * (inviab.Patamar ?? 1) + (inviab.SupInf == "INF" ? 1 : 2);

                                double valor = Math.Ceiling(inviab.Violacao);

                                le162[p] = inviab.SupInf == "INF" ? le162[p] - valor : le162[p] + valor;

                                if (le162[p] < 0) le162[p] = 0;
                                continue;
                            }

                        }
                        ///////////

                        if (le.Estagio < inviab.Estagio)
                        {

                            var nle = le.Clone();
                            nle.Estagio = inviab.Estagio;
                            if (inviab.TipoRestricao == "RHE") dadger.BlocoRhe.Add(nle);
                            else if (inviab.TipoRestricao == "RHQ") dadger.BlocoRhq.Add(nle);
                            else if (inviab.TipoRestricao == "RHV") dadger.BlocoRhv.Add(nle);
                            le = nle;
                        }

                        var i = 2 * (inviab.Patamar ?? 1) + (inviab.SupInf == "INF" ? 1 : 2);

                        double valorInviab;
                        if (inviab.TipoRestricao == "RHV")
                        {
                            valorInviab = Math.Ceiling(inviab.Violacao * 100d) / 100d;
                        }
                        else
                        {
                            valorInviab = Math.Ceiling(inviab.Violacao);
                        }


                        if (inviab.TipoRestricao == "RHQ" && inviab.CodRestricao == 258) //tratamento para as restrições 256 e 258 referentes a belomonte e pimental que se comportam comose fossem uma
                        {                                                                //  caso  a violação seja  no limite inferior da restrição 258 deve-se retirar primeiro da restrição 256 e o restante da restrição 258 
                            IEnumerable<BaseLine> rs256;
                            rs256 = dadger.BlocoRhq.Where(x => x.Restricao == 256);

                            if (rs256.Count() > 0)
                            {
                                var ls256 = rs256.Where(x => x is Dadger.LqLine).Select(x => (Dadger.LqLine)x);
                                le256 = ls256.Where(x => x.Estagio <= inviab.Estagio).OrderByDescending(x => x.Estagio).FirstOrDefault();

                                if (le256.Estagio < inviab.Estagio)
                                {

                                    var nle256 = le256.Clone();
                                    nle256.Estagio = inviab.Estagio;
                                    dadger.BlocoRhq.Add(nle256);
                                    le256 = nle256;
                                }
                                //
                                var valorAntigo = le256[i];

                                le256[i] = inviab.SupInf == "INF" ? le256[i] - valorInviab : le[i] + valorInviab;

                                if (le256[i] < 0)
                                {
                                    le256[i] = 0;
                                    var inviabRestante = valorInviab - valorAntigo;
                                    le[i] = le[i] - inviabRestante;

                                    if (le[i] < 0) le[i] = 0;
                                }
                            }

                        }
                        else
                        {
                            le[i] =
                           inviab.SupInf == "INF"
                           ? le[i] - valorInviab
                           : le[i] + valorInviab;

                            if (le[i] < 0) le[i] = 0;
                        }

                    }
                }
                else if (inviab.TipoRestricao == "EVAP")
                {
                    var usina = hidr[inviab.Usina];
                    if (usina != null)
                    {
                        dadger.BlocoUh.Where(x => x.Usina == usina.Cod).First().Evaporacao = false;
                    }
                }
                else if (inviab.TipoRestricao == "IRRI" && nivel >= 2) // somente trata no nivel 2
                {
                    var usina = hidr[inviab.Usina];
                    if (usina != null)
                    {
                        //dadger.BlocoUh.Where(x => x.Usina == usina.Cod).First().Evaporacao = false;

                        var ti = dadger.BlocoTi.Where(x => x.Usina == usina.Cod).First();

                        ti[inviab.Estagio + 1] -= Math.Ceiling(inviab.Violacao);
                        if (ti[inviab.Estagio + 1] < 0) ti[inviab.Estagio + 1] = 0;
                    }
                }

            }
        }

        public static void flexibilizaTucurui(string dir)
        {
            var deckDecomp = DeckFactory.CreateDeck(dir) as ConsoleApp1.Decomp.Deck;
            var dadger = deckDecomp[ConsoleApp1.Decomp.DeckDocument.dadger].Document as Dadger.Dadger;

            IEnumerable<BaseLine> rs;
            dynamic le;

            rs = dadger.BlocoRhv.Where(x => x.Restricao == 101);

            var relato = Directory.GetFiles(dir, "relato.bkp", SearchOption.TopDirectoryOnly).FirstOrDefault();

            var blocos = File.ReadAllText(relato).Split("RELATORIO  DO  BALANCO  HIDRAULICO").Skip(1).ToList();

            double valorAcumulado = 0;
            if (rs.Count() > 0)
            {
                foreach (var parte in blocos)
                {
                     var bloco = parte.Split("Relatorio das Restricoes Hidraulicas").First();
                    var linhas = bloco.Split('\n').Skip(2).ToList();
                    var estagio = Convert.ToInt32(linhas[0].Split('/').Skip(1).First().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last());
                    foreach (var item in linhas)
                    {
                        if (item.Contains("TUCURUI"))
                        {
                            var tucurui = item.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (tucurui.First() == "TUCURUI")
                            {

                                var qVer = Convert.ToDouble(tucurui[4]);
                                var hm = Math.Round((qVer / Math.Pow(10, 6)) * 168 * 3600, 1);
                                valorAcumulado = Math.Round((valorAcumulado + hm), 1);
                                var ls = rs.Where(x => x is Dadger.LvLine).Select(x => (Dadger.LvLine)x);
                                le = ls.Where(x => x.Estagio == estagio).FirstOrDefault();
                                if (le != null)
                                {
                                    le[4] += valorAcumulado;
                                }
                            }
                        }

                    }

                }
            }

            deckDecomp[ConsoleApp1.Decomp.DeckDocument.dadger].Document.SaveToFile();

        }
        public static void CalcJirauStoAnto(string dir)
        {
            Console.WriteLine("Curvas cmont x cfuga atualizadas em 11-09-2020");

            List<double> cmont287 = new List<double> { 70.50, 71.28, 71.11, 70.83, 70.92, 71.22, 71.30, 71.30, 71.30, 71.30, 71.30, 71.30, 71.30 };
            List<double> cmont285 = new List<double> { 90.00, 89.45, 90.00, 90.00, 90.00, 89.75, 87.96, 85.26, 83.29, 82.78, 83.04, 84.55, 87.18 };
            List<double> cfuga287 = new List<double> { 54.59, 55.26, 57.23, 58.41, 58.12, 56.08, 53.55, 50.85, 48.55, 47.43, 48.03, 49.98, 52.62 };
            List<double> cfuga285 = new List<double> { 72.71, 72.92, 73.72, 74.24, 74.02, 73.21, 72.39, 71.80, 71.49, 71.40, 71.44, 71.66, 72.15 };
            List<double> vazaflu = new List<double> { 3805, 3836, 4012, 4909, 5600, 6165, 6800, 7010, 7488, 8695, 9626, 10400, 10600, 10913, 12311, 13334, 13733, 14685, 14858, 15855, 15900, 16600, 17566, 21314, 22700, 23900, 27059, 28685, 33650, 35496, 35800, 37600, 38372, 39050, 39085, 39902, 40300, 40516, 41400, 42250, 43000, 43600, 44050, 44400, 44555, 47521 };
            List<double> nabarra = new List<double> { 81.71, 81.73, 81.81, 82.50, 82.50, 82.78, 83.10, 83.17, 83.38, 83.93, 84.35, 84.70, 84.80, 84.93, 85.57, 86.03, 86.21, 86.64, 86.72, 87.17, 87.20, 87.50, 87.95, 89.65, 90.00, 90.00, 90.00, 90.00, 90.00, 89.77, 89.50, 89.00, 88.70, 88.50, 88.46, 88.17, 88.00, 87.93, 87.50, 87.00, 86.50, 86.00, 85.50, 85.00, 84.74, 85.43 };


            var dirs = System.IO.Directory.GetFiles(dir);
            var caso = dirs.Where(x => x.ToUpper().Contains("CASO.DAT")).First();
            var rv = File.ReadAllText(caso).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).First();

            var dadgerFile = dirs.Where(x => System.IO.Path.GetFileName(x).ToUpper().Equals("DADGER." + rv.ToUpper())).First();
            var prevsFile = dirs.Where(x => System.IO.Path.GetFileName(x).ToUpper().Equals("PREVS." + rv.ToUpper())).First();
            var quantilFile = dirs.Where(x => System.IO.Path.GetFileName(x).ToUpper().Equals("QUANTIL.CSV")).First();

            //var dadger = ConsoleApp1.DocumentFactory.Create(dadgerFile) as Dadger.Dadger;

            //var dadgerAc = dadger.BlocoAc.GroupBy(x => x.Mnemonico);
            int numSemPassada = TrataInviab.Functions.NumSemanasPasssadas(dadgerFile);
            int numSemDecomp = TrataInviab.Functions.NumSemanasDecomp(dadgerFile);
            int numEstDecomp = TrataInviab.Functions.NumEstagiosDecomp(dadgerFile);
            string tipoDecomp = TrataInviab.Functions.TipoDecomp(dadgerFile);
            DateTime dataEstudo = TrataInviab.Functions.DataEstudoDecomp(dadgerFile);

            //Mês em português por extenso
            // string mesEstudoExtenso = dataEstudo.ToString("MMMM", new CultureInfo("pt-BR")).ToUpper();
            //Mês abreviado em português também.
            string mesEstudo = new CultureInfo("pt-BR").DateTimeFormat.GetAbbreviatedMonthName(dataEstudo.Month).ToUpper();
            string mesSeguinte = new CultureInfo("pt-BR").DateTimeFormat.GetAbbreviatedMonthName(dataEstudo.AddMonths(1).Month).ToUpper();

            int anoEstudo = dataEstudo.Year;
            int anoSeguinte = dataEstudo.AddMonths(1).Year;

            int mesEstudoNum = dataEstudo.Month;
            int mesSeguinteNum = dataEstudo.AddMonths(1).Month;

            Console.WriteLine("numEstagios = " + numEstDecomp.ToString());
            Console.WriteLine("numSemanasPassadas = " + numSemPassada.ToString());
            Console.WriteLine($"numEstagios={numEstDecomp}     tipo={tipoDecomp}");


            //jirau
            string texto = "&adicionado automaticamente COTVOL e JUSMED Jirau/Sto Antonio\n" +
                $"&{mesEstudo} - CMONT  285={cmont285[mesEstudoNum]:0.00} CFUGA 285={cfuga285[mesEstudoNum]:0.00}\n" +
                $"&{mesSeguinte} - CMONT 285={cmont285[mesSeguinteNum]:0.00} CFUGA 285={cfuga285[mesSeguinteNum]:0.00}\n";

            var prevsLinhas = File.ReadAllLines(prevsFile).ToList();
            List<string[]> vaz285 = new List<string[]>();
            foreach (var linha in prevsLinhas)
            {
                var vazoes = linha.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (vazoes[1] == "285")
                {
                    vaz285.Add(vazoes);
                }
            }
            int indice = Convert.ToInt32(rv.ToUpper().Split('V').Last());

            for (int i = 1; i <= numEstDecomp; i++)
            {
                var v285 = Convert.ToDouble(vaz285[0][(indice + 2 + (i - 1))]);
                var cota = Calcula_cota285(v285, vazaflu, nabarra);
                var jusmed = Calcula_Jusmed285(cota, mesEstudoNum, cmont285, cfuga285);

                string posfix = "       ";
                if (tipoDecomp == "SEMANAL")
                {
                    posfix = mesEstudo + "  " + i.ToString();
                }
                texto += $"AC  285  JUSMED         {jusmed:0.0000}                                      {posfix}\n" +
                    $"AC  285  COTVOL        1         {cota:0.0000}                             {posfix}\n" +
                    $"AC  285  COTVOL        2         0.00000                             {posfix}\n" +
                    $"AC  285  COTVOL        3         0.00000                             {posfix}\n" +
                    $"AC  285  COTVOL        4         0.00000                             {posfix}\n" +
                    $"AC  285  COTVOL        5         0.00000                             {posfix}\n";
            }

            var quantilLinhas = File.ReadAllLines(quantilFile).ToList();
            double q285 = 0;
            foreach (var linha in quantilLinhas)
            {
                var dados = linha.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (dados.Count() > 4)
                {
                    if (dados[3].Contains("285"))
                    {
                        q285 = Convert.ToDouble(dados[4]);
                    }
                }

            }
            var qCota = Calcula_cota285(q285, vazaflu, nabarra);
            var qJusmed = Calcula_Jusmed285(qCota, mesSeguinteNum, cmont285, cfuga285);

            Console.WriteLine($"qcota285 = {qCota}");
            Console.WriteLine($"qJusmed285 = {qJusmed}");

            texto += $"AC  285  JUSMED         {qJusmed:0.0000}                                      {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  285  COTVOL        1         {qCota:0.0000}                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  285  COTVOL        2         0.00000                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  285  COTVOL        3         0.00000                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  285  COTVOL        4         0.00000                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  285  COTVOL        5         0.00000                             {mesSeguinte}    {anoSeguinte}\n";

            texto += $"&{mesEstudo} - CMONT  287={cmont287[mesEstudoNum]:0.00} CFUGA 287={cfuga287[mesEstudoNum]:0.00}\n" +
                $"&{mesSeguinte} - CMONT 287={cmont287[mesSeguinteNum]:0.00} CFUGA 287={cfuga287[mesSeguinteNum]:0.00}\n";

            //sto antonio
            //copia do codigo
            List<string[]> vaz287 = new List<string[]>();
            foreach (var linha in prevsLinhas)
            {
                var vazoes = linha.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (vazoes[1] == "287")
                {
                    vaz287.Add(vazoes);
                }
            }

            for (int i = 1; i <= numEstDecomp; i++)
            {
                var v287 = Convert.ToDouble(vaz287[0][(indice + 2 + (i - 1))]);
                var cota = Calcula_cota287(v287);
                var jusmed = Calcula_Jusmed287(cota, mesEstudoNum, cmont287, cfuga287);

                string posfix = "       ";
                if (tipoDecomp == "SEMANAL")
                {
                    posfix = mesEstudo + "  " + i.ToString();
                }
                texto += $"AC  287  JUSMED         {jusmed:0.0000}                                      {posfix}\n" +
                    $"AC  287  COTVOL        1         {cota:0.0000}                             {posfix}\n" +
                    $"AC  287  COTVOL        2         0.00000                             {posfix}\n" +
                    $"AC  287  COTVOL        3         0.00000                             {posfix}\n" +
                    $"AC  287  COTVOL        4         0.00000                             {posfix}\n" +
                    $"AC  287  COTVOL        5         0.00000                             {posfix}\n";
            }

            double q287 = 0;
            foreach (var linha in quantilLinhas)
            {
                var dados = linha.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (dados.Count() > 4)
                {
                    if (dados[3].Contains("287"))
                    {
                        q287 = Convert.ToDouble(dados[4]);
                    }
                }

            }
            var qCota287 = Calcula_cota287(q287);
            var qJusmed287 = Calcula_Jusmed287(qCota287, mesSeguinteNum, cmont287, cfuga287);

            Console.WriteLine($"qcota287 = {qCota287}");
            Console.WriteLine($"qJusmed287 = {qJusmed287}");

            texto += $"AC  287  JUSMED         {qJusmed287:0.0000}                                      {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  287  COTVOL        1         {qCota287:0.0000}                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  287  COTVOL        2         0.00000                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  287  COTVOL        3         0.00000                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  287  COTVOL        4         0.00000                             {mesSeguinte}    {anoSeguinte}      \n" +
                $"AC  287  COTVOL        5         0.00000                             {mesSeguinte}    {anoSeguinte}";
            //termino da copia
            texto = texto.Replace(',', '.');
            File.WriteAllText(Path.Combine(dir, "dadger." + rv + ".temp.modif"), texto);//mudar o nome para dadger.rvx.temp.modif//calculado
            //parte final
            var dadgerLinhas = File.ReadAllLines(dadgerFile, Encoding.GetEncoding("iso-8859-1")).ToList();
            List<string> aux = new List<string>();
            foreach (var linha in dadgerLinhas)
            {
                var regex = System.Text.RegularExpressions.Regex.Match(linha, @"^AC\s*28[57]\s*(JUSMED|COTVOL)");
                if (!regex.Success)
                {
                    aux.Add(linha.ToString());
                }
            }
            File.WriteAllLines(Path.Combine(dir, "dadger." + rv + ".temp"), aux, Encoding.GetEncoding("iso-8859-1"));//mudar para dadger.rvx.temp//sem ACs
            var index = aux.IndexOf(aux.Where(x => x.StartsWith("AC")).Last());

            if (!File.Exists(Path.Combine(dir, "dadger." + rv + ".origjirstoant"))) //renomear aqui o dadger original para dadger.rvx.origjirstoant
            {
                File.Move(dadgerFile, Path.Combine(dir, "dadger." + rv + ".origjirstoant"));
            }

            aux.Insert(index + 1, texto); // inserindo o texto com os novos dados no final do bloco AC do dadger
            File.WriteAllLines(dadgerFile, aux, Encoding.GetEncoding("iso-8859-1"));//mudar para dadger.rvx//com ACs

        }
        public static double Calcula_cota285(double v285, List<double> vazaflu, List<double> nabarra)
        {
            int i = 0;
            int j = 0;
            if (v285 < 47521)
            {
                if (v285 > 3805)
                {
                    foreach (var vflu in vazaflu)
                    {
                        if (v285 > vflu)
                        {
                            i = (i + 1);
                            j = (i - 1);
                        }
                    }

                    var vazSup = vazaflu[i];
                    var vazInf = vazaflu[j];
                    var naSup = nabarra[i];
                    var naInf = nabarra[j];
                    //( $vazsup - $vazinf ) * ( $nainf ) + ( $q - $vazinf ) * ( $nasup - $nainf )) / ( $vazsup - $vazinf )"
                    double resultado = Math.Round(((vazSup - vazInf) * (naInf) + (v285 - vazInf) * (naSup - naInf)) / (vazSup - vazInf), 4);
                    return resultado;
                }
                else
                {
                    return 81.7100;
                }
            }
            else
            {
                return 85.4000;
            }
        }

        public static double Calcula_cota287(double v287)
        {
            if (v287 < 34000)
            {
                return 71.3000;
            }
            else
            {
                return 70.5000;
            }
        }

        public static double Calcula_Jusmed285(double v285, int mesEstudoNum, List<double> cmont285, List<double> cfuga285)
        {
            var cotaBase = cmont285[mesEstudoNum];
            var jusmedBase = cfuga285[mesEstudoNum];

            var resultado = Math.Round((v285 - (cotaBase - jusmedBase)), 4);
            return resultado;
        }

        public static double Calcula_Jusmed287(double v287, int mesEstudoNum, List<double> cmont287, List<double> cfuga287)
        {
            var cotaBase = cmont287[mesEstudoNum];
            var jusmedBase = cfuga287[mesEstudoNum];

            var resultado = Math.Round((v287 - (cotaBase - jusmedBase)), 4);
            return resultado;
        }
        public static void Altera_Dadgnl_Sem(string dir)
        {

            var dataCaso = dir.Split('/').Last();
            var dtEstudoAno = Convert.ToInt32(dataCaso.Substring(0, 4));
            var dtEstudoMes = dataCaso.Substring(4, 2);
            DateTime dtEstudo = new DateTime(dtEstudoAno, int.Parse(dtEstudoMes), 01);

            var dir_Partes = dir.Split('/');
            string NewaveBase = "";

            for (int i = 0; i <= dir_Partes.Count() - 3; i++)
            {
                NewaveBase = NewaveBase + dir_Partes[i] + "/";

            }
            // Directory.SetCurrentDirectory(NewaveBase);
            var name_DCGNL = Directory.GetDirectories(NewaveBase, "DCGNL*");

            // Directory.SetCurrentDirectory(dir);

            var path_DCGNL = Path.Combine(name_DCGNL);




            Console.WriteLine(dataCaso);

            Console.WriteLine(NewaveBase);
            var deckNWEstudo = DeckFactory.CreateDeck(Path.Combine(NewaveBase, dataCaso)) as ConsoleApp1.Newave.Deck;
            var deckDCEstudo = DeckFactory.CreateDeck(dir) as ConsoleApp1.Decomp.Deck;

            ConsoleApp1.Dadgnl.Dadgnl dadgnl;

            dadgnl = deckDCEstudo[ConsoleApp1.Decomp.DeckDocument.dadgnl].Document as ConsoleApp1.Dadgnl.Dadgnl;


            ConsoleApp1.AdtermDat.AdtermDat adterm;

            adterm = deckNWEstudo[ConsoleApp1.Newave.Deck.DeckDocument.adterm].Document as ConsoleApp1.AdtermDat.AdtermDat;
            var uts = dadgnl.BlocoTG.Where(x => x.Estagio == 1).ToArray();



            var glOriginal = dadgnl.BlocoGL.ToList();
            // dadgnl.BlocoGL.Clear();

            var patamares = deckNWEstudo[ConsoleApp1.Newave.Deck.DeckDocument.patamar].Document as ConsoleApp1.PatamarDat.PatamarDat;
            var durPat1 = patamares.Blocos["Duracao"].Where(x => x[1] == dtEstudoAno).OrderBy(x => x[0]).Select(x => x[int.Parse(dtEstudoMes.ToString())]).ToArray();
            bool patamares2019 = durPat1[0] > 0.15;


            var mesOperativo = MesOperativo.CreateSemanal(dtEstudoAno, int.Parse(dtEstudoMes), patamares2019);

            foreach (var ut in uts)
            {
                var tgLine = ut.Clone();



                //  var glLine = new ConsoleApp1.Dadgnl.GlLine();
                //  glLine.NumeroUsina = ut.Usina;
                //  glLine.Subsistema = ut[2];



                for (int _e = 0; _e < mesOperativo.EstagiosReaisDoMesAtual; _e++)
                {

                    var Linha_GL1 = dadgnl.BlocoGL.Where(x => x.MesInicio == mesOperativo.SemanasOperativas[_e].Inicio.Month && x.AnoInicio == mesOperativo.SemanasOperativas[_e].Inicio.Year && x.DiaInicio == mesOperativo.SemanasOperativas[_e].Inicio.Day && x.NumeroUsina == ut.Usina).FirstOrDefault();
                    double[] dadosAdt = new double[3];

                    DateTime dataBen = new DateTime(Linha_GL1.AnoInicio, Linha_GL1.MesInicio, Linha_GL1.DiaInicio);

                    dataBen = dataBen.AddDays(-63);

                    var dir_Ben = Path.Combine(path_DCGNL, dataBen.ToString("yyyyMM"), "bengnl.csv");

                    if (File.Exists(dir_Ben))
                    {

                        StreamReader rd = new StreamReader(dir_Ben);

                        string linha = null;

                        string[] dado = null;
                        try
                        {
                            while ((linha = rd.ReadLine()) != null)
                            {
                                dado = linha.Split(';');


                                if (dado[0].Trim() == "1")
                                {

                                    var usina_ben = int.Parse(dado[4].Trim());

                                    if (usina_ben == ut.Usina)
                                    {
                                        var beneficio = Convert.ToDouble(dado[7].Trim());
                                        var custo = Convert.ToDouble(dado[8].Trim());
                                        var pata_Ben = int.Parse(dado[6].Trim());


                                        switch (pata_Ben)
                                        {
                                            case 1:
                                                dadosAdt[0] = beneficio > custo ? (float)tgLine[6] : 0;
                                                Linha_GL1.GeracaoPat1 = Math.Min((float)dadosAdt[0], (float)tgLine[6]);
                                                break;
                                            case 2:
                                                dadosAdt[1] = beneficio > custo ? (float)tgLine[9] : 0;
                                                Linha_GL1.GeracaoPat2 = Math.Min((float)dadosAdt[1], (float)tgLine[9]);
                                                break;
                                            case 3:
                                                dadosAdt[2] = beneficio > custo ? (float)tgLine[12] : 0;
                                                Linha_GL1.GeracaoPat3 = Math.Min((float)dadosAdt[2], (float)tgLine[12]);
                                                break;
                                        }

                                    }
                                }

                            }
                            rd.Close();



                        }
                        catch
                        {
                            rd.Close();
                        }

                    }
                    else
                    {



                        int indice;


                        foreach (var adt in adterm.Despachos.Where(x => x.String != "            "))
                        {
                            if (adt.Numero == ut.Usina)
                            {
                                indice = adterm.Despachos.IndexOf(adt);
                                indice = indice + 1;

                                dadosAdt[0] = adterm.Despachos[indice].Lim_P1;
                                dadosAdt[1] = adterm.Despachos[indice].Lim_P2;
                                dadosAdt[2] = adterm.Despachos[indice].Lim_P3;
                            }
                        }
                    }
                    Tuple<double, double, double> despacho;
                    despacho = new Tuple<double, double, double>(dadosAdt[0], dadosAdt[1], dadosAdt[2]);

                    Linha_GL1.GeracaoPat1 = Math.Min((float)despacho.Item1, (float)tgLine[6]);
                    Linha_GL1.GeracaoPat2 = Math.Min((float)despacho.Item2, (float)tgLine[9]);
                    Linha_GL1.GeracaoPat3 = Math.Min((float)despacho.Item3, (float)tgLine[12]);

                }

                var dtTemp = mesOperativo.Fim.AddDays(1);






                var linhas_Seg = dadgnl.BlocoTG.Where(x => x.Estagio != 1).ToArray();

                double[] dadosTG = new double[3];

                foreach (var lin in linhas_Seg)
                {
                    var tg2line = lin.Clone();

                    if (tg2line[1] == ut.Usina)
                    {

                        dadosTG[0] = tg2line[6];
                        dadosTG[1] = tg2line[9];
                        dadosTG[2] = tg2line[12];

                    }
                }




                for (int _e = mesOperativo.EstagiosReaisDoMesAtual; _e < 9; _e++)
                {
                    double[] dadosAdt = new double[3];
                    var endSemanaTemp = dtTemp.AddDays(6);
                    if (_e > mesOperativo.EstagiosReaisDoMesAtual && endSemanaTemp.Day < 7) endSemanaTemp = endSemanaTemp.AddDays(-endSemanaTemp.Day);


                    var semanaOperativaTemp = new SemanaOperativa(dtTemp, endSemanaTemp, patamares2019);

                    var Linha_GL1 = dadgnl.BlocoGL.Where(x => x.MesInicio == semanaOperativaTemp.Inicio.Month && x.AnoInicio == semanaOperativaTemp.Inicio.Year && x.DiaInicio == semanaOperativaTemp.Inicio.Day && x.NumeroUsina == ut.Usina).FirstOrDefault();

                    DateTime dataBen = new DateTime(Linha_GL1.AnoInicio, Linha_GL1.MesInicio, Linha_GL1.DiaInicio);

                    dataBen = dataBen.AddDays(-63);

                    var dir_Ben = Path.Combine(path_DCGNL, dataBen.ToString("yyyyMM"), "bengnl.csv");

                    if (File.Exists(dir_Ben))
                    {

                        StreamReader rd = new StreamReader(dir_Ben);

                        string linha = null;

                        string[] dado = null;
                        try
                        {
                            while ((linha = rd.ReadLine()) != null)
                            {
                                dado = linha.Split(';');


                                if (dado[0].Trim() == "1")
                                {

                                    var usina_ben = int.Parse(dado[4].Trim());

                                    if (usina_ben == ut.Usina)
                                    {
                                        var beneficio = Convert.ToDouble(dado[7].Trim());
                                        var custo = Convert.ToDouble(dado[8].Trim());
                                        var pata_Ben = int.Parse(dado[6].Trim());


                                        switch (pata_Ben)
                                        {
                                            case 1:
                                                dadosAdt[0] = beneficio > custo ? (float)dadosTG[0] : 0;
                                                Linha_GL1.GeracaoPat1 = Math.Min((float)dadosAdt[0], (float)dadosTG[0]);
                                                break;
                                            case 2:
                                                dadosAdt[1] = beneficio > custo ? (float)dadosTG[1] : 0;
                                                Linha_GL1.GeracaoPat2 = Math.Min((float)dadosAdt[1], (float)dadosTG[1]);
                                                break;
                                            case 3:
                                                dadosAdt[2] = beneficio > custo ? (float)dadosTG[2] : 0;
                                                Linha_GL1.GeracaoPat3 = Math.Min((float)dadosAdt[2], (float)dadosTG[2]);
                                                break;
                                        }

                                    }
                                }

                            }
                            rd.Close();



                        }
                        catch
                        {
                            rd.Close();
                        }

                    }

                    else
                    {

                        Tuple<double, double, double> despacho;
                        int indice;



                        foreach (var adt in adterm.Despachos.Where(x => x.String != "            "))
                        {
                            if (adt.Numero == ut.Usina)
                            {
                                indice = adterm.Despachos.IndexOf(adt);

                                indice = indice + 2;

                                dadosAdt[0] = adterm.Despachos[indice].Lim_P1;
                                dadosAdt[1] = adterm.Despachos[indice].Lim_P2;
                                dadosAdt[2] = adterm.Despachos[indice].Lim_P3;
                            }

                        }
                        //var tg2line = new ConsoleApp1.Dadgnl.TgLine();

                        despacho = new Tuple<double, double, double>(dadosAdt[0], dadosAdt[1], dadosAdt[2]);

                        Linha_GL1.GeracaoPat1 = Math.Min((float)despacho.Item1, (float)dadosTG[0]);
                        Linha_GL1.GeracaoPat2 = Math.Min((float)despacho.Item2, (float)dadosTG[1]);
                        Linha_GL1.GeracaoPat3 = Math.Min((float)despacho.Item3, (float)dadosTG[2]);



                    }
                    dtTemp = dtTemp.AddDays(7);
                }
            }

            dadgnl.SaveToFile(createBackup: true);

        }


        public static void Altera_Dadgnl_Men(string dir)
        {

            var dataCaso = dir.Split('/').Last();
            var dtEstudoAno = Convert.ToInt32(dataCaso.Substring(0, 4));
            var dtEstudoMes = dataCaso.Substring(4, 2);
            DateTime dtEstudo = new DateTime(dtEstudoAno, int.Parse(dtEstudoMes), 01);

            var dir_Partes = dir.Split('/');
            string NewaveBase = "";

            for (int i = 0; i <= dir_Partes.Count() - 3; i++)
            {
                NewaveBase = NewaveBase + dir_Partes[i] + "/";

            }



            var deckNWEstudo = DeckFactory.CreateDeck(Path.Combine(NewaveBase, dataCaso)) as ConsoleApp1.Newave.Deck;


            ConsoleApp1.Dadgnl.Dadgnl dadgnl = null;
            ConsoleApp1.Dadgnl.Dadgnl dadgnl_ori = null;

            Decomp.Deck deckDCEstudo = null;
            try
            {
                deckDCEstudo = DeckFactory.CreateDeck(dir) as ConsoleApp1.Decomp.Deck;
            }
            catch (Exception e)
            {
                Console.WriteLine("Deck decomp não encontrado");

            }

            try
            {
                dadgnl = deckDCEstudo[ConsoleApp1.Decomp.DeckDocument.dadgnl].Document as ConsoleApp1.Dadgnl.Dadgnl;
            }
            catch
            {
                Console.WriteLine("Dadgnl não encontrado");
            }

            try
            {
                dadgnl_ori = deckDCEstudo[ConsoleApp1.Decomp.DeckDocument.oficial_dadgnl].Document as ConsoleApp1.Dadgnl.Dadgnl;
            }
            catch
            {
                Console.WriteLine("Dadgnl oficial não encontrado");
            }


            //   dadgnl.BlocoGL.Clear();


            var patamares = deckNWEstudo[ConsoleApp1.Newave.Deck.DeckDocument.patamar].Document as ConsoleApp1.PatamarDat.PatamarDat;
            var durPat1 = patamares.Blocos["Duracao"].Where(x => x[1] == dtEstudoAno).OrderBy(x => x[0]).Select(x => x[int.Parse(dtEstudoMes.ToString())]).ToArray();
            bool patamares2019 = durPat1[0] > 0.15;

            var mesOperativo = MesOperativo.CreateMensal(dtEstudoAno, int.Parse(dtEstudoMes), patamares2019);


            var horasMesEstudoP1 = mesOperativo.SemanasOperativas[0].HorasPat1;
            var horasMesEstudoP2 = mesOperativo.SemanasOperativas[0].HorasPat2;
            var horasMesEstudoP3 = mesOperativo.SemanasOperativas[0].HorasPat3;

            var horasMesSeguinteP1 = mesOperativo.SemanasOperativas[1].HorasPat1;
            var horasMesSeguinteP2 = mesOperativo.SemanasOperativas[1].HorasPat2;
            var horasMesSeguinteP3 = mesOperativo.SemanasOperativas[1].HorasPat3;

            var uts = dadgnl.BlocoTG.Where(x => x.Estagio == 1).ToArray();


            double[] dadosAdt = new double[3];

            foreach (var ut in uts)
            {

                var tgLine = ut.Clone();


                Dadgnl.GlLine Line_Ofi = null;


                var dt_Ben = dtEstudo.AddMonths(-2);
                var pasta_ben = dt_Ben.ToString("yyyyMM");


                var dir_Partes_Ben = dir.Split('/');
                string BaseDC = "";

                for (int i = 0; i <= dir_Partes_Ben.Count() - 2; i++)
                {
                    BaseDC = BaseDC + dir_Partes_Ben[i] + "/";

                }


                var dir_Ben = Path.Combine(BaseDC, pasta_ben, "bengnl.csv");

                try
                {
                    try
                    {
                        Line_Ofi = dadgnl_ori.BlocoGL.Where(x => x.MesInicio == int.Parse(dtEstudoMes) && x.AnoInicio == dtEstudoAno && x.NumeroUsina == ut.Usina).FirstOrDefault();
                    }
                    catch
                    {
                        Console.WriteLine("Dadgnl oficial não encontrado");
                    }

                    var Linha_GL1 = dadgnl.BlocoGL.Where(x => x.MesInicio == dtEstudo.Month && x.AnoInicio == dtEstudo.Year && x.NumeroUsina == ut.Usina).FirstOrDefault();

                    if (Line_Ofi != null)
                    {

                        dadosAdt[0] = Line_Ofi.GeracaoPat1;
                        dadosAdt[1] = Line_Ofi.GeracaoPat2;
                        dadosAdt[2] = Line_Ofi.GeracaoPat3;

                        Linha_GL1.GeracaoPat1 = Math.Min((float)dadosAdt[0], (float)tgLine[6]);
                        Linha_GL1.GeracaoPat2 = Math.Min((float)dadosAdt[1], (float)tgLine[9]);
                        Linha_GL1.GeracaoPat3 = Math.Min((float)dadosAdt[2], (float)tgLine[12]);

                    }
                    else if (File.Exists(dir_Ben))
                    {

                        StreamReader rd = new StreamReader(dir_Ben);

                        string linha = null;

                        string[] dado = null;
                        try
                        {
                            while ((linha = rd.ReadLine()) != null)
                            {
                                dado = linha.Split(';');


                                if (dado[0].Trim() == "1")
                                {

                                    var usina_ben = int.Parse(dado[4].Trim());

                                    if (usina_ben == ut.Usina)
                                    {
                                        var beneficio = Convert.ToDouble(dado[7].Trim());
                                        var custo = Convert.ToDouble(dado[8].Trim());
                                        var pata_Ben = int.Parse(dado[6].Trim());


                                        switch (pata_Ben)
                                        {
                                            case 1:
                                                dadosAdt[0] = beneficio > custo ? (float)tgLine[6] : 0;
                                                Linha_GL1.GeracaoPat1 = Math.Min((float)dadosAdt[0], (float)tgLine[6]);
                                                break;
                                            case 2:
                                                dadosAdt[1] = beneficio > custo ? (float)tgLine[9] : 0;
                                                Linha_GL1.GeracaoPat2 = Math.Min((float)dadosAdt[1], (float)tgLine[9]);
                                                break;
                                            case 3:
                                                dadosAdt[2] = beneficio > custo ? (float)tgLine[12] : 0;
                                                Linha_GL1.GeracaoPat3 = Math.Min((float)dadosAdt[2], (float)tgLine[12]);
                                                break;
                                        }

                                    }
                                }

                            }
                            rd.Close();



                        }
                        catch
                        {
                            rd.Close();
                        }

                    }
                    else
                    {

                        // var Linha_GL1_Origem = dadgnl.BlocoGL.Where(x => x.MesInicio == dtEstudo.Month && x.AnoInicio == dtEstudo.Year && x.NumeroUsina == ut.Usina).FirstOrDefault();
                        //  dadosAdt[0] = Linha_GL1_Origem.GeracaoPat1;
                        //  dadosAdt[1] = Linha_GL1_Origem.GeracaoPat2;
                        //  dadosAdt[2] = Linha_GL1_Origem.GeracaoPat3;

                    }
                }
                catch
                {
                    Console.WriteLine("Erros no 1 mês");
                }

                //Segundo Mês

                try
                {
                    var dt_seguinte = dtEstudo.AddMonths(1);

                    Dadgnl.GlLine Line_Ofi_Seguinte = null;

                    try
                    {
                        Line_Ofi_Seguinte = dadgnl_ori.BlocoGL.Where(x => x.MesInicio == dt_seguinte.Month && x.AnoInicio == dt_seguinte.Year && x.NumeroUsina == ut.Usina).FirstOrDefault();
                    }
                    catch
                    {
                        Console.WriteLine("Dadgnl Oficial não encontrado");
                    }


                    dt_Ben = dt_seguinte.AddMonths(-2);
                    pasta_ben = dt_Ben.ToString("yyyyMM");



                    dir_Ben = Path.Combine(BaseDC, pasta_ben, "bengnl.csv");



                    var linhas_Seg = dadgnl.BlocoTG.Where(x => x.Estagio != 1).ToArray();

                    double[] dadosTG = new double[3];

                    foreach (var lin in linhas_Seg)
                    {
                        var tg2line = lin.Clone();

                        if (tg2line[1] == ut.Usina)
                        {
                            var Linha_GL = dadgnl.BlocoGL.Where(x => x.MesInicio == dt_seguinte.Month && x.AnoInicio == dt_seguinte.Year && x.NumeroUsina == ut.Usina).FirstOrDefault();


                            dadosTG[0] = tg2line[6];
                            dadosTG[1] = tg2line[9];
                            dadosTG[2] = tg2line[12];

                        }
                    }

                    var Linha_GL2 = dadgnl.BlocoGL.Where(x => x.MesInicio == dt_seguinte.Month && x.AnoInicio == dt_seguinte.Year && x.NumeroUsina == ut.Usina).FirstOrDefault();

                    if (Line_Ofi_Seguinte != null)
                    {

                        dadosAdt[0] = Line_Ofi_Seguinte.GeracaoPat1;
                        dadosAdt[1] = Line_Ofi_Seguinte.GeracaoPat2;
                        dadosAdt[2] = Line_Ofi_Seguinte.GeracaoPat3;

                        Linha_GL2.GeracaoPat1 = Math.Min((float)dadosAdt[0], (float)dadosTG[0]);
                        Linha_GL2.GeracaoPat2 = Math.Min((float)dadosAdt[1], (float)dadosTG[1]);
                        Linha_GL2.GeracaoPat3 = Math.Min((float)dadosAdt[2], (float)dadosTG[2]);

                    }
                    else if (File.Exists(dir_Ben))
                    {

                        StreamReader rd = new StreamReader(dir_Ben);

                        string linha = null;

                        string[] dado = null;
                        try
                        {
                            while ((linha = rd.ReadLine()) != null)
                            {
                                dado = linha.Split(';');


                                if (dado[0].Trim() == "1")
                                {

                                    var usina_ben = int.Parse(dado[4].Trim());
                                    var per = int.Parse(dado[0].Trim());
                                    if (usina_ben == ut.Usina)
                                    {
                                        var beneficio = Convert.ToDouble(dado[7].Trim());
                                        var custo = Convert.ToDouble(dado[8].Trim());
                                        var pata_Ben = int.Parse(dado[6].Trim());


                                        switch (pata_Ben)
                                        {
                                            case 1:
                                                dadosAdt[0] = beneficio > custo ? dadosTG[0] : 0;
                                                Linha_GL2.GeracaoPat1 = Math.Min((float)dadosAdt[0], (float)dadosTG[0]);
                                                break;
                                            case 2:
                                                dadosAdt[1] = beneficio > custo ? dadosTG[1] : 0;
                                                Linha_GL2.GeracaoPat2 = Math.Min((float)dadosAdt[1], (float)dadosTG[1]);
                                                break;
                                            case 3:
                                                dadosAdt[2] = beneficio > custo ? dadosTG[2] : 0;
                                                Linha_GL2.GeracaoPat3 = Math.Min((float)dadosAdt[2], (float)dadosTG[2]);
                                                break;
                                        }

                                    }
                                }

                            }

                            rd.Close();
                        }
                        catch
                        {
                            rd.Close();
                        }

                    }
                    else
                    {
                        //   var Linha_GL_Origem = dadgnl.BlocoGL.Where(x => x.MesInicio == dt_seguinte.Month && x.AnoInicio == dt_seguinte.Year && x.NumeroUsina == ut.Usina).FirstOrDefault();
                        //   dadosAdt[0] = Linha_GL_Origem.GeracaoPat1;
                        //  dadosAdt[1] = Linha_GL_Origem.GeracaoPat2;
                        //  dadosAdt[2] = Linha_GL_Origem.GeracaoPat3;

                    }
                }
                catch
                {
                    Console.WriteLine("Erros no 2 mês");
                }


            }

            dadgnl.SaveToFile(createBackup: true);
        }


        public static void Altera_Adterm(string dir)
        {
            var dataCaso = dir.Split('/').Last();

            var dtEstudoAno = Convert.ToInt32(dataCaso.Substring(0, 4));
            var dtEstudoMes = dataCaso.Substring(4, 2);
            DateTime dtEstudo = new DateTime(dtEstudoAno, int.Parse(dtEstudoMes), 01);

            var deckNWEstudo = DeckFactory.CreateDeck(dir) as ConsoleApp1.Newave.Deck;

            ConsoleApp1.AdtermDat.AdtermDat adterm;

            adterm = deckNWEstudo[ConsoleApp1.Newave.Deck.DeckDocument.adterm].Document as ConsoleApp1.AdtermDat.AdtermDat;

            double[] dadosAdt = new double[3];

            foreach (var adt in adterm.Despachos.Where(x => x.String != "            "))
            {
                var dt_Ben = dtEstudo.AddMonths(-2);
                var pasta_ben = dt_Ben.ToString("yyyyMM");

                var dir_Partes = dir.Split('/');
                string dir_Base = "";

                for (int i = 0; i <= dir_Partes.Count() - 2; i++)
                {
                    dir_Base = dir_Base + dir_Partes[i] + "/";

                }


                var pasta_DC = Directory.GetDirectories(dir_Base, "DCGNL*");




                var dir_Ben = Path.Combine(pasta_DC[0], pasta_ben, "bengnl.csv");

                if (File.Exists(dir_Ben))
                {

                    int indice;
                    indice = adterm.Despachos.IndexOf(adt);
                    indice = indice + 1;

                    StreamReader rd = new StreamReader(dir_Ben);

                    string linha = null;

                    string[] dado = null;
                    try
                    {
                        while ((linha = rd.ReadLine()) != null)
                        {
                            dado = linha.Split(';');


                            if (dado[0].Trim() == "1")
                            {

                                var usina_ben = int.Parse(dado[4].Trim());

                                if (usina_ben == adt.Numero)
                                {
                                    var beneficio = Convert.ToDouble(dado[7].Trim());
                                    var custo = Convert.ToDouble(dado[8].Trim());
                                    var pata_Ben = int.Parse(dado[6].Trim());


                                    switch (pata_Ben)
                                    {
                                        case 1:
                                            dadosAdt[0] = beneficio > custo ? adterm.Despachos[indice].Lim_P1 : 0;
                                            adterm.Despachos[indice].Lim_P1 = dadosAdt[0];
                                            break;
                                        case 2:
                                            dadosAdt[1] = beneficio > custo ? adterm.Despachos[indice].Lim_P2 : 0;
                                            adterm.Despachos[indice].Lim_P2 = dadosAdt[1];
                                            break;
                                        case 3:
                                            dadosAdt[2] = beneficio > custo ? adterm.Despachos[indice].Lim_P3 : 0;
                                            adterm.Despachos[indice].Lim_P3 = dadosAdt[2];
                                            break;
                                    }


                                    //   adterm.Despachos[indice].Lim_P1 = dadosAdt[0];
                                    //  adterm.Despachos[indice].Lim_P2 = dadosAdt[1];
                                    //      adterm.Despachos[indice].Lim_P3 = dadosAdt[2];
                                }
                            }

                        }
                        rd.Close();



                    }
                    catch
                    {
                        rd.Close();
                    }





                }




                var dt_Ben_Seg = dtEstudo.AddMonths(-1);
                pasta_ben = dt_Ben_Seg.ToString("yyyyMM");

                dir_Partes = dir.Split('/');
                dir_Base = "";

                for (int i = 0; i <= dir_Partes.Count() - 2; i++)
                {
                    dir_Base = dir_Base + dir_Partes[i] + "/";

                }


                pasta_DC = Directory.GetDirectories(dir_Base, "DCGNL*");

                dir_Ben = Path.Combine(pasta_DC[0], pasta_ben, "bengnl.csv");

                if (File.Exists(dir_Ben))
                {
                    int indice;
                    indice = adterm.Despachos.IndexOf(adt);
                    indice = indice + 2;

                    StreamReader rd = new StreamReader(dir_Ben);

                    string linha = null;

                    string[] dado = null;
                    try
                    {
                        while ((linha = rd.ReadLine()) != null)
                        {
                            dado = linha.Split(';');


                            if (dado[0].Trim() == "1")
                            {

                                var usina_ben = int.Parse(dado[4].Trim());

                                if (usina_ben == adt.Numero)
                                {
                                    var beneficio = Convert.ToDouble(dado[7].Trim());
                                    var custo = Convert.ToDouble(dado[8].Trim());
                                    var pata_Ben = int.Parse(dado[6].Trim());


                                    switch (pata_Ben)
                                    {
                                        case 1:
                                            dadosAdt[0] = beneficio > custo ? adterm.Despachos[indice].Lim_P1 : 0;
                                            adterm.Despachos[indice].Lim_P1 = dadosAdt[0];
                                            break;
                                        case 2:
                                            dadosAdt[1] = beneficio > custo ? adterm.Despachos[indice].Lim_P2 : 0;
                                            adterm.Despachos[indice].Lim_P2 = dadosAdt[1];
                                            break;
                                        case 3:
                                            dadosAdt[2] = beneficio > custo ? adterm.Despachos[indice].Lim_P3 : 0;
                                            adterm.Despachos[indice].Lim_P3 = dadosAdt[2];
                                            break;
                                    }

                                }
                            }

                        }
                        rd.Close();



                    }
                    catch
                    {
                        rd.Close();
                    }


                    //    adterm.Despachos[indice].Lim_P1 = dadosAdt[0];
                    ///  adterm.Despachos[indice].Lim_P2 = dadosAdt[1];
                    //  adterm.Despachos[indice].Lim_P3 = dadosAdt[2];


                }



            }

            adterm.SaveToFile(createBackup: true);

        }

    }
}

