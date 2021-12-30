﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Compass.CommomLibrary {
    public abstract class BaseDeck {

        public abstract Dictionary<string, DeckFile> Documents { get; }
        public abstract void GetFiles(string baseFolder);
        public abstract void CopyFilesToFolder(string folder);
        public abstract Result GetResults();
        public string BaseFolder { get; set; }

        //static Dictionary<SistemaEnum, double[]> enaMLT = new Dictionary<SistemaEnum, double[]>();
        public static Dictionary<SistemaEnum, double[]> EnaMLT { get; set; }
    }

    public static class DeckFactory {

        public static BaseDeck CreateDeck(string folder) {

            try {

                var files = Directory.GetFiles(folder);

                BaseDeck deck;

                if (files.Any(f => f.EndsWith("SISTEMA.DAT", StringComparison.OrdinalIgnoreCase))) {
                    deck = new Compass.CommomLibrary.Newave.Deck();
                } else if (files.Any(f => f.ToUpper().Contains("DADGER.")))
                    deck = new Compass.CommomLibrary.Decomp.Deck();
                else if (files.Any(f=>f.ToLower().Contains("operut.dat")))
                {
                    deck = new CommomLibrary.Dessem.Deck();
                }
                else return null;

                deck.GetFiles(folder);
                deck.BaseFolder = folder;
                return deck;

            } catch {
                return null;
            }
        }
    }

    public enum SistemaEnum {
        SE = 1,
        S = 2,
        NE = 3,
        N = 4
    }
    public class Result {

        public string Tipo { get; set; }

        public string Comentario { get; set; }

        public Result() : this("") { }

        internal Result(string dir) {

            Dir = dir;
            Cortes = "";


            Sistemas = new List<SistResult>(){
                    new SistResult(){ Sistema = SistemaEnum.SE},
                    new SistResult(){ Sistema = SistemaEnum.S},
                    new SistResult(){ Sistema = SistemaEnum.NE},
                    new SistResult(){ Sistema = SistemaEnum.N},
                };

        }

        public SistResult this[SistemaEnum sis] {
            get { return Sistemas[(int)sis - 1]; }
        }

        public SistResult this[int sis] {
            get { return Sistemas[sis - 1]; }
        }

        public SistResult this[string sis] {

            get {
                switch (sis.Trim()) {
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

        public class SistResult {
            

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

        public List<GNLResult> GNL_Result { get; set; }

        public class GNLResult
        {
            public int Posto { get; set; }
            public int Sistema { get; set; }

            public int semana { get; set; }

            public double GNL_pat1 { get; set; }
            public double GNL_pat2 { get; set; }
            public double GNL_pat3 { get; set; }

            public GNLResult(object[,] dados)
            {
                Posto = int.Parse(dados[0, 0].ToString());
                Sistema = int.Parse(dados[0, 1].ToString());
                semana = int.Parse(dados[0, 2].ToString());
                GNL_pat1 = Math.Round(Double.Parse(dados[0, 3].ToString()), 2);
                GNL_pat2 = Math.Round(Double.Parse(dados[0, 4].ToString()), 2);
                GNL_pat3 = Math.Round(Double.Parse(dados[0, 5].ToString()), 2);
            }

        }

        public List<CMO_Mensal> CMO_Mensal_Result { get; set; }
        public class CMO_Mensal
        {
            public int semana { get; set; }
            public int submercado { get; set; }
            public double CMO_Men { get; set; }

            public CMO_Mensal(object[,] dados)
            {
                semana = int.Parse(dados[0, 0].ToString());
                submercado = int.Parse(dados[0, 1].ToString());
                CMO_Men = Math.Round(Double.Parse(dados[0, 2].ToString()), 2);
            }


        }

    }
}
