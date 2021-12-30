using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compass.CommomLibrary;
using Compass.ExcelTools.Templates;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

namespace Compass.Services
{
    public class GeraDessem
    {
        public static void CriarEntdados(string path, DateTime dataEstudo, DateTime fimrev)
        {
            string pathBlocos = "C:\\Files\\Middle - Preço\\Resultados_Modelos\\DECODESS\\Arquivos_Base\\BlocosFixos";

            var entdadosFile = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("entdados")).First();
            var entdados = DocumentFactory.Create(entdadosFile) as Compass.CommomLibrary.EntdadosDat.EntidadosDat;

            #region adiciona fp
            var rests = entdados.BlocoFp.Where(x => x.Usina == 287).ToList();
            if (rests.Count == 0)
            {
                var rest = new Compass.CommomLibrary.EntdadosDat.FpLine();
                rest.IdBloco = "FP";
                rest.Usina = 287;
                rest.TipoFuncao = 2;
                rest.PontoVazTurb = 10;
                rest.PontoVolArm = 5;
                rest.VolUtilPerc = 100.00f;
                entdados.BlocoFp.Add(rest);
            }
            #endregion

            #region comentar RHEs
            List<int> restComent = new List<int> { 141, 143, 145, 272, 654};

            foreach (var rest in restComent)
            {
                foreach (var rhe in entdados.BlocoRhe.RheGrouped.Where(x => x.Key[1] == rest))
                {
                    foreach (var rh in rhe.Value)
                    {
                        rh[0] = "&" + rh[0];
                    }
                }
            }
            for (int i = 602; i <= 630; i++)
            {
                foreach (var rhe in entdados.BlocoRhe.RheGrouped.Where(x => x.Key[1] == i))
                {
                    foreach (var rh in rhe.Value)
                    {
                        rh[0] = "&" + rh[0];
                    }
                }
            }
            #endregion

            #region limpa PQ

            entdados.BlocoPq.Clear();

            #endregion

            #region BLOCO TM
            var intervalos = Tools.GetIntervalosHoararios(dataEstudo);
            string comentario = entdados.BlocoTm.First().Comment;
            foreach (var line in entdados.BlocoTm.ToList())
            {
                var dia = Convert.ToInt32(line.DiaInicial);
                if (dia <= dataEstudo.Day)
                {
                    entdados.BlocoTm.Remove(line);
                }
            }
            var blocoClone = new Compass.CommomLibrary.EntdadosDat.TmBlock();

            foreach (var linha in entdados.BlocoTm.ToList())
            {
                blocoClone.Add(linha);
            }

            entdados.BlocoTm.Clear();
            for (int i = 0; i < 24; i++)
            {
                var newline = new Compass.CommomLibrary.EntdadosDat.TmLine();
                newline.IdBloco = "TM";
                newline.DiaInicial = dataEstudo.Day.ToString();
                newline.HoraDiaInicial = i;
                newline.MeiaHora = 0;
                newline.Duracao = 0.5f;
                newline.Rede = 0;
                newline.NomePatamar = intervalos[i];
                if (i == 0)
                {
                    newline.Comment = comentario;
                }
                entdados.BlocoTm.Add(newline);
                var newline2 = new Compass.CommomLibrary.EntdadosDat.TmLine();
                newline2.IdBloco = "TM";
                newline2.DiaInicial = dataEstudo.Day.ToString();
                newline2.HoraDiaInicial = i;
                newline2.MeiaHora = 1;
                newline2.Duracao = 0.5f;
                newline2.Rede = 0;
                newline2.NomePatamar = intervalos[i];
                entdados.BlocoTm.Add(newline2);

            }
            foreach (var linha in blocoClone.ToList())
            {
                entdados.BlocoTm.Add(linha);
            }

            #endregion

            #region VE

            foreach (var line  in entdados.BlocoVe.ToList())
            {
                line.DiaInic = dataEstudo.Day.ToString();
                line.DiaFinal = dataEstudo.AddDays(1).Day.ToString();
            }

            #endregion

            #region IA

            foreach (var iaLine in entdados.BlocoIa.Where(x => x.SistemaA.Trim() == "N" && x.SistemaB.Trim() == "SE").ToList())
            {
                iaLine.IdBloco = "&" + iaLine.IdBloco;
            }

            #endregion

            #region TVIAG

            var tviagFile = Directory.GetFiles(pathBlocos).Where(x => Path.GetFileName(x).Contains("blocoTVIAG")).First();


            entdados.BlocoTviag.Clear();
            var tviaglines = File.ReadAllLines(tviagFile, Encoding.UTF8);
            string comments = null;
            foreach (var linha in tviaglines)
            {
                if (linha.StartsWith("&"))
                {
                    comments = comments == null ? linha : comments + Environment.NewLine + linha;
                }
                else
                {
                    var newL = entdados.BlocoTviag.CreateLine(linha);
                    newL.Comment = comments;
                    comments = null;
                    entdados.BlocoTviag.Add(newL);
                }
            }

            #endregion

            #region DP/DE

            var inicioRev = fimrev.AddDays(-6);
            int index = 0;
            for(DateTime d = inicioRev;d <= fimrev;d = d.AddDays(1))
            {
                if (d <= dataEstudo)
                {
                    index++;
                }
            }
            var dpFile = Directory.GetFiles(pathBlocos).Where(x => Path.GetFileName(x).Contains($"blocoDP{index}")).First();
            var dplines = File.ReadAllLines(dpFile, Encoding.UTF8);
            entdados.BlocoDp.Clear();

            comments = null;
            foreach (var linha in dplines)//copia os dados do blocoDP fixo
            {
                if (linha.StartsWith("&"))
                {
                    comments = comments == null ? linha : comments + Environment.NewLine + linha;
                }
                else
                {
                    var newL = entdados.BlocoDp.CreateLine(linha);
                    newL.Comment = comments;
                    comments = null;
                    entdados.BlocoDp.Add(newL);
                }
            }

            for (int s = 1; s <= 4; s++)//corrige as datas iniciais
            {
                DateTime dia = dataEstudo;
                var diaComp = Convert.ToInt32(entdados.BlocoDp.Where(x => x.Subsist == s).Select(x => x.DiaInic).First());
                foreach (var dp in entdados.BlocoDp.Where(x => x.Subsist == s).ToList())
                {
                    var dialinha = Convert.ToInt32(dp.DiaInic);
                    if (diaComp == dialinha)
                    {
                        dp.DiaInic = dia.Day.ToString();
                    }
                    else
                    {
                        diaComp = Convert.ToInt32(dp.DiaInic);
                        dia = dia.AddDays(1);

                        dp.DiaInic = dia.Day.ToString();
                    }
                }

            }
            foreach (var dp in entdados.BlocoDp.Where(x => x.Subsist == 11).ToList())
            {
                dp.DiaInic = dataEstudo.Day.ToString();
            }
            
            //DE
           
            var deFile = Directory.GetFiles(pathBlocos).Where(x => Path.GetFileName(x).Contains($"blocoDE{index}")).First();
            var delines = File.ReadAllLines(deFile, Encoding.UTF8);
            entdados.BlocoDe.Clear();

            comments = null;
            foreach (var linha in delines)//copia os dados do blocoDE fixo
            {
                if (linha.StartsWith("&"))
                {
                    comments = comments == null ? linha : comments + Environment.NewLine + linha;
                }
                else
                {
                    var newL = entdados.BlocoDe.CreateLine(linha);
                    newL.Comment = comments;
                    comments = null;
                    entdados.BlocoDe.Add(newL);
                }
            }

            for (int de = 1; de <= 5; de++)//corrige as datas iniciais
            {
                DateTime dia = dataEstudo;
                if (de == 5)
                {
                    dia = inicioRev;
                }
                var diaComp = Convert.ToInt32(entdados.BlocoDe.Where(x => x.NumDemanda == de).Select(x => x.DiaInic).First());
                foreach (var dem in entdados.BlocoDe.Where(x => x.NumDemanda == de).ToList())
                {
                    var dialinha = Convert.ToInt32(dem.DiaInic);
                    if (diaComp == dialinha)
                    {
                        dem.DiaInic = dia.Day.ToString();
                    }
                    else
                    {
                        diaComp = Convert.ToInt32(dem.DiaInic);
                        dia = dia.AddDays(1);

                        dem.DiaInic = dia.Day.ToString();
                    }
                }

            }
            foreach (var dem in entdados.BlocoDe.Where(x => (x.NumDemanda == 11) || (x.NumDemanda == 6)).ToList())
            {
                dem.DiaInic = dataEstudo.Day.ToString();
            }

            #endregion
            entdados.SaveToFile();

            #region AC/IT/GP

            var acFile = Directory.GetFiles(pathBlocos).Where(x => Path.GetFileName(x).Contains("blocoAC")).First();
            var itFile = Directory.GetFiles(pathBlocos).Where(x => Path.GetFileName(x).Contains("blocoIT")).First();
            var gpFile = Directory.GetFiles(pathBlocos).Where(x => Path.GetFileName(x).Contains("blocoGP")).First();
            var acTexto = File.ReadAllText(acFile, Encoding.UTF8);

            var itTexto = File.ReadAllLines(itFile, Encoding.UTF8);
            var gpTexto = File.ReadAllLines(gpFile, Encoding.UTF8);

            var entlinhas = File.ReadAllLines(entdadosFile, Encoding.UTF8).ToList();

            int indiceAC = entlinhas.IndexOf(entlinhas.Where(x => x.StartsWith("AC")).Last());
            entlinhas.Insert((indiceAC + 1), acTexto);

            var itLinha = itTexto.Where(x => x.StartsWith("IT")).First();
            var gpLinha = gpTexto.Where(x => x.StartsWith("GP")).First();

            int indiceIT = entlinhas.IndexOf(entlinhas.Where(x => x.StartsWith("IT")).First());
            int indiceGP = entlinhas.IndexOf(entlinhas.Where(x => x.StartsWith("GP")).First());

            entlinhas[indiceIT] = itLinha;
            entlinhas[indiceGP] = gpLinha;

            File.WriteAllLines(entdadosFile, entlinhas, Encoding.UTF8);

            #endregion

            var areacontfile = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("areacont")).First();
            var areacont = DocumentFactory.Create(areacontfile) as Compass.CommomLibrary.Areacont.Areacont;

            areacont.SaveToFile();

            var cotasr11File = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("cotasr11")).First();
            var cotasr11 = DocumentFactory.Create(cotasr11File) as Compass.CommomLibrary.Cotasr.Cotasr;

            foreach (var line in cotasr11.BlocoCot.ToList())
            {
                line.Dia = dataEstudo.AddDays(-1).Day;
            }
            cotasr11.SaveToFile();
        }



    }
}
