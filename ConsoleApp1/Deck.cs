﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    public enum DeckDocument
    {
        caso,
        dadger,
        dadgnl,
        hidr,
        loss,
        mlt,
        modif,
        postos,
        prevs,
        vazoes,
        vazoesc
    }

    class DeckDecomp : BaseDeck
    {

        Dictionary<string, DeckFile> documents = new Dictionary<string, DeckFile> {
            {"CASO.DAT"   , null},
            {"DADGER."  , null},
            {"DADGNL."  , null},
            {"HIDR.DAT"   , null},
            {"LOSS.DAT"   , null},
            {"PERDAS.DAT"   , null},
            {"MLT.DAT"    , null},
            {"MODIF.DAT"  , null},
            {"POSTOS.DAT" , null},
            {"PREVS."   , null},
            {"VAZOES."  , null},
            {"VAZOES.DAT"  , null},
            {"VAZOESC.DAT", null},
            {"REGRAS.DAT", null},
            {"GEVAZP.DAT", null},
            {"BACIAS.DAT", null},
            {"ARQUIVOS.DAT", null},
            {"GEVAZP.CFG", null},

        };

        public override Dictionary<string, DeckFile> Documents { get { return documents; } }

        public DeckFile this[DeckDocument doc]
        {

            get
            {
                switch (doc)
                {
                    case DeckDocument.caso:
                        return Documents["CASO.DAT"];
                    case DeckDocument.dadger:
                        return Documents["DADGER."];
                    case DeckDocument.dadgnl:
                        return Documents["DADGNL."];
                    case DeckDocument.hidr:
                        return Documents["HIDR.DAT"];
                    case DeckDocument.loss:
                        if (Documents["PERDAS.DAT"] != null) return Documents["PERDAS.DAT"];
                        else return Documents["LOSS.DAT"];
                    case DeckDocument.mlt:
                        return Documents["MLT.DAT"];
                    case DeckDocument.modif:
                        return Documents["MODIF.DAT"];
                    case DeckDocument.postos:
                        return Documents["POSTOS.DAT"];
                    case DeckDocument.prevs:
                        return Documents["PREVS."];
                    case DeckDocument.vazoes:
                        return Documents["VAZOES."];
                    case DeckDocument.vazoesc:
                        return Documents["VAZOES.DAT"] ?? Documents["VAZOESC.DAT"];
                    default:
                        return null;
                }
            }
            set
            {
                switch (doc)
                {
                    case DeckDocument.caso:
                        documents["CASO.DAT"] = value;
                        break;
                    case DeckDocument.dadger:
                        documents["DADGER."] = value;
                        break;
                    case DeckDocument.dadgnl:
                        documents["DADGNL."] = value;
                        break;
                    case DeckDocument.hidr:
                        documents["HIDR.DAT"] = value;
                        break;
                    case DeckDocument.loss:
                        documents["LOSS.DAT"] = value;
                        break;
                    case DeckDocument.mlt:
                        documents["MLT.DAT"] = value;
                        break;
                    case DeckDocument.modif:
                        documents["MODIF.DAT"] = value;
                        break;
                    case DeckDocument.postos:
                        documents["POSTOS.DAT"] = value;
                        break;
                    case DeckDocument.prevs:
                        documents["PREVS."] = value;
                        break;
                    case DeckDocument.vazoes:
                        documents["VAZOES."] = value;
                        break;
                    case DeckDocument.vazoesc:
                        documents["VAZOESC.DAT"] = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public string Folder { get; set; }
        public int Rev { get; set; }

        public string caso;
        public string Caso { get { return caso; } set { caso = value; } }

        public override void GetFiles(string baseFolder)
        {

            BaseFolder = baseFolder;

            var folderFiles = System.IO.Directory.GetFiles(baseFolder).Where(x => !x.EndsWith(".bak", StringComparison.OrdinalIgnoreCase));


            var q = from doc in documents
                    from file in folderFiles
                    let filename = System.IO.Path.GetFileName(file)
                    where (doc.Key.EndsWith(".") && filename.StartsWith(doc.Key, StringComparison.OrdinalIgnoreCase))
                    || (filename.Equals(doc.Key, StringComparison.OrdinalIgnoreCase))
                    select new { doc.Key, file };

            if (q.Any(x => x.Key == "CASO.DAT"))
            {
                var f = q.Where(x => x.Key == "CASO.DAT").First().file;
                documents["CASO.DAT"] = new DeckFile(f);
                GetCaso(f);
            }
            else
                throw new FileNotFoundException("CASO.DAT não encontrado");


            q.Where(x => x.Key != "CASO.DAT").ToList().ForEach(x =>
            {

                if (!string.IsNullOrWhiteSpace(Caso) && x.Key.EndsWith("."))
                {
                    if (x.file.EndsWith(Caso, StringComparison.OrdinalIgnoreCase))
                        documents[x.Key] = new DeckFile(x.file);
                }
                else
                    documents[x.Key] = new DeckFile(x.file);

            });

            var casoFile = folderFiles.Where(x => System.IO.Path.GetFileName(x)
                .Equals(Caso, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (casoFile == null) throw new FileNotFoundException(Caso + " não encontrado");

            if (!documents.ContainsKey(Caso))
            {
                documents.Add(Caso, new DeckFile(casoFile));
            }

        }

        private void GetCaso(string file)
        {
            Caso = File.ReadAllLines(file)[0].Trim();
        }

        public override void CopyFilesToFolder(string folder)
        {
            Folder = folder;

            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            foreach (var doc in documents.Where(x => x.Value != null))
            {
                doc.Value.Folder = folder;
                System.IO.File.Copy(doc.Value.BasePath, doc.Value.Path, true);
            }

        }

        //Result result = null;

        //public override Result GetResults()
        //{

        //    if (result != null) return result;

        //    try
        //    {

        //        var dadger = this[DeckDocument.dadger].Document as Dadger.Dadger;
        //        var numEstagios = dadger.VAZOES_NumeroDeSemanas;
        //        var dias2mes = dadger.VAZOES_NumeroDiasDoMes2;

        //        var dec_oper = Path.Combine(this.BaseFolder, "dec_oper_sist.csv");

        //        if (!File.Exists(dec_oper)) return null;

        //        result = new Result(this.BaseFolder) { Tipo = "DC" };

        //        //List<string> datalines = new List<string>();
        //        List<string[]> datalines = new List<string[]>();
        //        using (var sr = File.OpenText(dec_oper))
        //        {

        //            while (sr.ReadLine() != "@Tabela") { }
        //            sr.ReadLine();
        //            sr.ReadLine();
        //            sr.ReadLine();
        //            sr.ReadLine();
        //            string l;
        //            do
        //            {
        //                l = sr.ReadLine();
        //                var ls = l.Split(';').Select(x => x.Trim()).ToArray();
        //                if (ls.Length > 10 && ls[0] == ls[1]) datalines.Add(ls);
        //                else break;
        //            } while (!sr.EndOfStream);
        //        }

        //        //Dictionary<string, string[,]> resumos = new Dictionary<string, string[,]>();
        //        var resumofile = Path.Combine(this.BaseFolder, "resumo.log");
        //        var casodatfile = Path.Combine(this.BaseFolder, "caso.dat");


        //        var files = Directory.GetFiles(BaseFolder).ToList();

        //        var relatoPath = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals("relato"));
        //        int mesEstudo = -1;
        //        if (relatoPath != null)
        //        {
        //            var relato = (Relato.Relato)DocumentFactory.Create(relatoPath);
        //            foreach (var th in relato.THMensal) result[(int)th[1]].EnaTH = th[3];
        //            relato.EnergiaAcoplamentoSistema.Where(ena => ena[1] == 1).Select(ena =>
        //            {

        //                result[Enum.Parse(typeof(SistemaEnum), ena[0])].EnaSemCV = ena[2];

        //                if (dadger.VAZOES_NumeroDeSemanas == 0)
        //                {
        //                    result[Enum.Parse(typeof(SistemaEnum), ena[0])].Ena = ena[2];
        //                }
        //                else
        //                { // calcular ena fechamento do mês.
        //                    result[Enum.Parse(typeof(SistemaEnum), ena[0])].Ena = ena[2];
        //                }

        //                return true;

        //            }).ToList();

        //            mesEstudo = relato.PeriodoInicio.Month - 1;
        //            //foreach (var ena in )) result[(int)ena[1]].Ena = ena[3];
        //        }

        //        if (File.Exists(resumofile) && File.Exists(casodatfile))
        //        {
        //            var caso = File.ReadAllLines(casodatfile);
        //            int cvIdx;
        //            if (int.TryParse(caso[0].Substring(2, 1), out cvIdx))
        //            {

        //                var r = File.ReadAllLines(resumofile);

        //                if (r.Length == 5)
        //                {
        //                    var se = r[1].Split(' ');
        //                    var s = r[2].Split(' ');
        //                    var ne = r[3].Split(' ');
        //                    var n = r[4].Split(' ');

        //                    result[1].Ena = float.Parse(se[6]);
        //                    result[2].Ena = float.Parse(s[6]);
        //                    result[3].Ena = float.Parse(ne[6]);
        //                    result[4].Ena = float.Parse(n[6]);

        //                    result[1].EnaMLT = float.Parse(se[7].Replace("%", "")) / 100;
        //                    result[2].EnaMLT = float.Parse(s[7].Replace("%", "")) / 100;
        //                    result[3].EnaMLT = float.Parse(ne[7].Replace("%", "")) / 100;
        //                    result[4].EnaMLT = float.Parse(n[7].Replace("%", "")) / 100;

        //                    result[1].EnaSemCV = float.Parse(se[cvIdx]);
        //                    result[2].EnaSemCV = float.Parse(s[cvIdx]);
        //                    result[3].EnaSemCV = float.Parse(ne[cvIdx]);
        //                    result[4].EnaSemCV = float.Parse(n[cvIdx]);
        //                }
        //            }
        //        }

        //        var cortesPath = (this[DeckDocument.dadger].Document as Dadger.Dadger).CortesPath;
        //        result.Cortes = System.IO.Path.GetDirectoryName(cortesPath);

        //        if (BaseDeck.EnaMLT == null || BaseDeck.EnaMLT[SistemaEnum.SE][mesEstudo] == 0)
        //        {
        //            var deck = DeckFactory.CreateDeck(result.Cortes);
        //            if (deck is Newave.Deck) DeckFactory.CreateDeck(result.Cortes).GetResults();
        //        }


        //        datalines
        //            .Where(x => x[0] == "1")
        //            .GroupBy(x => int.Parse(x[4]))
        //            .Where(x => x.Key < 5).ToList()
        //            .ForEach(x =>
        //            {

        //                double cmo1, cmo2, cmo3, cmo, earmI, earmf;

        //                double.TryParse(x.First(y => y[2].Trim() == "1")[23].Trim(),
        //                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo,
        //                    out cmo1);
        //                double.TryParse(x.First(y => y[2].Trim() == "2")[23],
        //                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo,
        //                    out cmo2);
        //                double.TryParse(x.First(y => y[2].Trim() == "3")[23],
        //                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo,
        //                    out cmo3);
        //                double.TryParse(x.First(y => y[2].Trim() == "-")[23],
        //                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo,
        //                    out cmo);
        //                double.TryParse(x.First(y => y[2].Trim() == "-")[20],
        //                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo,
        //                    out earmI);
        //                double.TryParse(x.First(y => y[2].Trim() == "-")[22],
        //                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo,
        //                    out earmf);
        //                //double.TryParse(x.First(y => y[2].Trim() == "-")[18].Trim(),
        //                //    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo,
        //                //    out enaCV);
        //                result[x.Key].DemandaPrimeiroEstagio = double.Parse(x.First(y => y[2].Trim() == "-")[6],
        //                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);


        //                if (x.Key == 1)
        //                {
        //                    //som 1.900MW (IT50Hz) e geracao 60Hz                        
        //                    result[x.Key].GerHidr = double.Parse(x.First(y => y[2].Trim() == "-")[10],
        //                              System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo)
        //                              + double.Parse(x.First(y => y[2].Trim() == "-")[16].Replace("-", "0").Trim(),
        //                              System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo)
        //                              + 1900d;
        //                }
        //                else
        //                {
        //                    result[x.Key].GerHidr = double.Parse(x.First(y => y[2].Trim() == "-")[10],
        //                              System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);
        //                }

        //                //Ter + TerAt
        //                result[x.Key].GerTerm = double.Parse(x.First(y => y[2].Trim() == "-")[8],
        //                 System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo)
        //                  + double.Parse(x.First(y => y[2].Trim() == "-")[9].Replace("-", "0").Trim(),
        //                      System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);

        //                result[x.Key].GerPeq = double.Parse(x.First(y => y[2].Trim() == "-")[7],
        //                     System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);


        //                result[x.Key].EarmI = earmI / 100;
        //                result[x.Key].EarmF = earmf / 100;
        //                result[x.Key].Cmo = cmo;
        //                result[x.Key].Cmo_pat1 = cmo1;
        //                result[x.Key].Cmo_pat2 = cmo2;
        //                result[x.Key].Cmo_pat3 = cmo3;

        //                if (BaseDeck.EnaMLT != null)
        //                {

        //                    result[x.Key].EnaMLT = result[x.Key].Ena / BaseDeck.EnaMLT[(SistemaEnum)x.Key][mesEstudo];
        //                    result[x.Key].EnaTHMLT = result[x.Key].EnaTH / BaseDeck.EnaMLT[(SistemaEnum)x.Key][mesEstudo == 0 ? 11 : mesEstudo - 1];

        //                }
        //            });


        //        if (numEstagios == 0)
        //        {

        //            for (int sis = 1; sis <= 4; sis++) result[sis].DemandaMes = result[sis].DemandaPrimeiroEstagio;
        //            datalines
        //                    .Where(x => x[2] == "-")
        //                    .Where(x => x[0] == "2")
        //                    .GroupBy(x => int.Parse(x[4]))
        //                    .Where(x => x.Key < 5).ToList().ForEach(x =>
        //                    {
        //                        result[x.Key].DemandaMesSeguinte = double.Parse(x.First()[6],
        //                            System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);

        //                    });

        //        }
        //        else
        //        {


        //            datalines
        //                   .Where(x => x[2] == "-")
        //                   .Where(x => x[0] != (numEstagios + 1).ToString())
        //                   .GroupBy(x => int.Parse(x[4]))
        //                   .Where(x => x.Key < 5).ToList().ForEach(x =>
        //                   {
        //                       int totaldias = 0;
        //                       result[x.Key].DemandaMes =
        //                           x.Sum(y =>
        //                           {

        //                               int peso = (
        //                                   y[0] == "1" ? (dadger.DataEstudo.Month != dadger.VAZOES_MesInicialDoEstudo ? dadger.DataEstudo.AddDays(6).Day : 7)
        //                                   : (
        //                                            y[0] == numEstagios.ToString() ? 7 - dias2mes : 7
        //                                        )
        //                                   );
        //                               totaldias += peso;
        //                               return

        //                                   double.Parse(y[6],
        //                                   System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo)
        //                                   * peso;
        //                           }) / (totaldias);
        //                   });

        //            datalines
        //                  .Where(x => x[2] == "-")
        //                  .Where(x => x[0] == (numEstagios + 1).ToString())
        //                  .GroupBy(x => int.Parse(x[4]))
        //                  .Where(x => x.Key < 5).ToList().ForEach(x =>
        //                  {
        //                      result[x.Key].DemandaMesSeguinte = double.Parse(x.First()[6],
        //                                    System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);

        //                  });
        //        }



        //    }
        //    catch { }
        //    return result;

        //}

    }
       

    public abstract class BaseDeck
    {

        public abstract Dictionary<string, DeckFile> Documents { get; }
        public abstract void GetFiles(string baseFolder);
        public abstract void CopyFilesToFolder(string folder);
        //public abstract Result GetResults();
        public string BaseFolder { get; set; }

        //static Dictionary<SistemaEnum, double[]> enaMLT = new Dictionary<SistemaEnum, double[]>();
        public static Dictionary<SistemaEnum, double[]> EnaMLT { get; set; }
    }

    public class DeckFile
    {

        public string BasePath
        {
            get { return System.IO.Path.Combine(BaseFolder, BaseFileName); }
            set
            {
                BaseFolder = System.IO.Path.GetDirectoryName(value);
                BaseFileName = System.IO.Path.GetFileName(value);
            }
        }
        public string BaseFileName { get; set; }
        public string BaseFolder { get; set; }

        public string Path { get { return System.IO.Path.Combine(Folder, FileName); } }
        public string Folder { get; set; }
        public string FileName { get; set; }

        BaseDocument document = null;
        public BaseDocument Document
        {
            get
            {

                if (document == null)
                {
                    try
                    {
                        document = DocumentFactory.Create(BasePath);
                    }
                    finally { }
                }
                return document;
            }
            set { document = value; }
        }

        public DeckFile(string baseFile)
        {
            BasePath = baseFile;
            FileName = BaseFileName;
        }

        public void BackUp()
        {
            System.IO.File.Copy(BasePath, BasePath + DateTime.Now.ToString("_yyyyMMddHHmmss.bak"), true);
        }
    }

    public static class DeckFactory
    {

        public static BaseDeck CreateDeck(string folder)
        {

            try
            {

                var files = Directory.GetFiles(folder);

                BaseDeck deck;

                if (files.Any(f => f.EndsWith("SISTEMA.DAT", StringComparison.OrdinalIgnoreCase)))
                {
                    deck = new ConsoleApp1.Newave.Deck();
                }
                else
                if (files.Any(f => f.ToUpper().Contains("DADGER.")))
                    deck = new ConsoleApp1.Decomp.Deck();
                else return null;

                deck.GetFiles(folder);
                deck.BaseFolder = folder;
                return deck;

            }
            catch
            {
                return null;
            }
        }
    }

    public enum SistemaEnum
    {
        SE = 1,
        S = 2,
        NE = 3,
        N = 4
    }
    public class Result
    {

        public string Tipo { get; set; }

        public string Comentario { get; set; }

        public Result() : this("") { }

        internal Result(string dir)
        {

            Dir = dir;
            Cortes = "";


            Sistemas = new List<SistResult>(){
                    new SistResult(){ Sistema = SistemaEnum.SE},
                    new SistResult(){ Sistema = SistemaEnum.S},
                    new SistResult(){ Sistema = SistemaEnum.NE},
                    new SistResult(){ Sistema = SistemaEnum.N},
                };

        }

        public SistResult this[SistemaEnum sis]
        {
            get { return Sistemas[(int)sis - 1]; }
        }

        public SistResult this[int sis]
        {
            get { return Sistemas[sis - 1]; }
        }

        public SistResult this[string sis]
        {

            get
            {
                switch (sis.Trim())
                {
                    case "SUDESTE":
                    case "SE":
                        return this[SistemaEnum.SE];
                    case "SUL":
                    case "S":
                        return this[SistemaEnum.S];
                    case "NORDESTE":
                    case "NE":
                        return this[SistemaEnum.NE];
                    case "NORTE":
                    case "N":
                        return this[SistemaEnum.N];
                    default: throw new Exception();
                };
            }
        }

        public List<SistResult> Sistemas { get; set; }

        public string Dir { get; set; }
        public string Cortes { get; set; }

        public class SistResult
        {


            public SistemaEnum Sistema { get; set; }

            public double EarmI { get; set; }
            public double EarmF { get; set; }
            public double Ena { get; set; }
            public double EnaTH { get; set; }

            public double Cmo { get; set; }
            public double Cmo_pat1 { get; set; }
            public double Cmo_pat2 { get; set; }
            public double Cmo_pat3 { get; set; }

            public double EnaMLT { get; set; }
            public double EnaTHMLT { get; set; }

            public double EnaSemCV { get; set; }

            public double DemandaPrimeiroEstagio { get; set; }
            public double DemandaMes { get; set; }
            public double DemandaMesSeguinte { get; set; }

            public double GerHidr { get; set; }
            public double GerTerm { get; set; }
            public double GerPeq { get; set; }
        }
    }
}
