﻿using Compass.CommomLibrary;
using Compass.ExcelTools;
using Compass.Services;
using System.Globalization;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Compass.DecompToolsShellX
{

    class Program
    {

        static Dictionary<string, Action<string>> actions = new Dictionary<string, Action<string>>();

        static void Main(string[] args)
        {
            actions.Add("abrir", open);
            actions.Add("vazoes", vazoes);
            actions.Add("vazoes6", vazoes6);
            actions.Add("earm", armazenamento);
            actions.Add("resultado", resultado);
            actions.Add("duplicar", duplicar);
            actions.Add("corte", corte_tendencia);
            actions.Add("cortes", cortes_tendencia);
            actions.Add("dgernwd", dgernwd);
            actions.Add("ons2ccee", ons2ccee);
            actions.Add("dessem2ccee", dessem2ccee);
            actions.Add("convdecodess", convDecodess);
            actions.Add("inviab", tratarInviabilidade);
            actions.Add("resultados", resultados);
            actions.Add("previvaz", previvaz);
            actions.Add("tendhidr", tendhidr);
            actions.Add("plddessem", pldDessem);
            actions.Add("uhdessem", uhDessem);

            //uhdessem"Z:\7_dessem\DecksDiarios\12_2020\RV3\29_12_2020_16_25_25"

            //convdecodess "Z:\teste_decodess\DEC_ONS_012021_RV1_VE_ccee"
            //convdecodess "Z:\teste_decodess\DEC_ONS_122020_RV3_VE_ccee"
            //convdecodess "Z:\teste_decodess\DEC_ONS_122020_RV2_VE_ccee"
            //dessem2ccee "C:\ConversaoDessem\DS_ONS_112020_RV2D16_teste|true"

            //dessem2ccee "P:\Bruno Araujo\ConversaoDessem\DS_ONS_112020_RV2D16_teste|true"

            //ons2ccee "Z:\6_decomp\03_Casos\2020_11\teste_bruno\Nova pasta\DEC_ONS_112020_RV2_VE|true"

            // ons2ccee "Z:\6_decomp\03_Casos\2020_11\teste_bruno\Teste_CVLINE\DEC_ONS_112020_RV0_VE|true"
            //previvaz "C:\Files\Middle - Preço\16_Chuva_Vazao\2020_07\RV4\20-07-20\testeBruno\CV_ACOMPH_FUNC_Atualizado\CHUVAVAZAO_CENARIO_1087970864.xlsm"|true --> para encadear o previvaz
            //  previvaz "C:\Files\Middle - Preço\16_Chuva_Vazao\2020_07\RV3\20-07-14\testeBruno\CV_ACOMPH_FUNC_EURO\CHUVAVAZAO_CENARIO_-883830657.xlsm"
            //   previvaz "C:\Files\Middle - Preço\16_Chuva_Vazao\2020_07\RV4\20-07-20\testeBruno\CPM_CV_FUNC_d-1_EURO\Propagacoes_Automaticas.txt""

            if (args.Length > 1)
            {
                var action = args[0].ToLower();

                if (actions.ContainsKey(action))
                {
                    actions[action].Invoke(args[1]);
                }
                else if (args.Length >= 2)
                {
                    actions[action].Invoke(args[1] + "|" + args[2]);
                }
            }
            else
            {
                resultado("");
            }
        }

        static void vazoes(string path)
        {
            Vazoes gevazp = null;

            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;
                gevazp = new Compass.Services.Vazoes();
                var files = gevazp.Run(dir, true);

                var prevcenRel = files.FirstOrDefault(f => f.FullPath.EndsWith("prevcen.rel", StringComparison.OrdinalIgnoreCase));
                if (prevcenRel != null)
                {

                    var relContent = File.ReadAllText(prevcenRel.FullPath);
                    var pat = @"USINA\s*:.+VALOR:\s+-\d+";

                    var vNegativas = System.Text.RegularExpressions.Regex.Matches(relContent, pat, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (vNegativas.Count > 0)
                    {

                        var alert = "";

                        foreach (System.Text.RegularExpressions.Match m in vNegativas)
                        {
                            alert += m.Value + "\r\n";
                        }

                        MessageBox.Show(alert, "Vazoes Incrementais Negativas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }



            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (gevazp != null)
                    gevazp.ClearTempFolder();
            }
        }

        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }

        static void vazoes6(string path)
        {
            Vazoes6 gevazp = null;

            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;
                gevazp = new Compass.Services.Vazoes6();
                var files = gevazp.Run(dir, true);




            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (gevazp != null)
                    gevazp.ClearTempFolder();
            }
        }

        static void previvaz(string path)
        {
            Previvaz previvaz = null;
            bool encad = false;
            if (path.Contains("true"))
            {
                var command = path.Split('|');
                path = command[0];
                encad = Convert.ToBoolean(command[1]);
            }


            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path) && path.EndsWith("Propagacoes_Automaticas.txt"))
                {
                    path = path.Substring(0, path.IndexOf("Propagacoes_Automaticas.txt"));
                    Previvaz.ProcessResultsPart2(path, encad);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path) && path.EndsWith("xlsm", StringComparison.OrdinalIgnoreCase))
                    {
                        Previvaz.RunCenario(path, true, encad); //encad);
                    }
                    else
                    {
                        string dir;
                        if (Directory.Exists(path))
                        {
                            dir = path;
                        }
                        else if (File.Exists(path))
                        {
                            dir = Path.GetDirectoryName(path);
                        }
                        else
                            return;

                        previvaz = new Compass.Services.Previvaz();
                        previvaz.Run(dir);
                    }
                }

            }
            catch (Exception ex)
            {
               // System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        static void open(string filePath)
        {

            Microsoft.Office.Interop.Excel.Application xlApp = null;

            try
            {
                var doc = DocumentFactory.Create(filePath);

                xlApp = Helper.StartExcel();
                xlApp.Cursor = XlMousePointer.xlWait;
                xlApp.ScreenUpdating = false;

                var xlWb = xlApp.Workbooks.Add();

                var info = xlWb.SetInfosheet(doc);

                xlWb.WriteDocumentToWorkbook(doc);

                info.BottonComments = doc.BottonComments;

                //xlApp.WindowState = XlWindowState.xlMaximized;
                //xlWb.Windows[1].WindowState = XlWindowState.xlMaximized;
                xlApp.ActiveWindow.Activate();
                xlWb.Windows[1].Activate();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (xlApp != null)
                {
                    xlApp.Cursor = XlMousePointer.xlDefault;
                    xlApp.ScreenUpdating = true;

                    Helper.Release(xlApp);
                }
            }
        }

        static void armazenamento(string path)
        {
            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;

                var deck = DeckFactory.CreateDeck(dir);

                Compass.CommomLibrary.Decomp.ConfigH configH;
                if (deck is Compass.CommomLibrary.Decomp.Deck)
                {

                    var dadger = (Compass.CommomLibrary.Dadger.Dadger)DocumentFactory.Create(deck.Documents["DADGER."].BasePath);
                    var hidr = (Compass.CommomLibrary.HidrDat.HidrDat)DocumentFactory.Create(deck.Documents["HIDR.DAT"].BasePath);


                    configH = new Compass.CommomLibrary.Decomp.ConfigH(dadger, hidr);

                }
                else if (deck is Compass.CommomLibrary.Newave.Deck)
                {

                    var confhddat = (Compass.CommomLibrary.ConfhdDat.ConfhdDat)DocumentFactory.Create(deck.Documents["CONFHD.DAT"].BasePath);
                    var modifdat = BaseDocument.Create<Compass.CommomLibrary.ModifDatNW.ModifDatNw>(File.ReadAllText(deck.Documents["MODIF.DAT"].BasePath));
                    var hidr = (Compass.CommomLibrary.HidrDat.HidrDat)DocumentFactory.Create(deck.Documents["HIDR.DAT"].BasePath);

                    configH = new Compass.CommomLibrary.Decomp.ConfigH(confhddat, hidr, modifdat);

                }
                else
                {
                    MessageBox.Show("Deck não identificado");
                    return;
                }



                double[] earmAtual = configH.GetEarms();
                double[] earmMax = configH.GetEarmsMax();


                var dtEarm = new System.Data.DataTable();

                dtEarm.Columns.Add("Sistema");
                dtEarm.Columns.Add("EarmMax");
                dtEarm.Columns.Add("EarmIni");
                dtEarm.Columns.Add("EarmIni_Perc");

                //var rs2 = new List<object>();

                var fmt = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

                int i = 0;
                foreach (var sb in configH.index_sistemas)
                {
                    dtEarm.Rows.Add(
                        sb.Item2.ToString(),
                        earmMax[i].ToString("N1", fmt),
                        earmAtual[i].ToString("N1", fmt),
                        (earmAtual[i] / earmMax[i]).ToString("00.0%", fmt));
                    //rs2.Add(new { Sistema = sb.Item2.ToString(), EarmMax = earmMax[i].ToString("N1", fmt), EarmIni = earmAtual[i].ToString("N1", fmt), EarmIni_Perc = (earmAtual[i] / earmMax[i]).ToString("00.0%", fmt) });
                    i++;
                }

                FormViewer.Show("EARM calculado - " + dir, new ResultDataSource { Title = "Armazenamento", DataSource = dtEarm });


            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void resultado(string path)
        {
            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                {
                    FormViewer.Show("", new Result());
                    return;
                }

                var deck = DeckFactory.CreateDeck(dir);

                if (deck is CommomLibrary.Newave.Deck || deck is CommomLibrary.Decomp.Deck)
                {

                    var results = deck.GetResults();
                    FormViewer.Show(dir, results);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void resultados(string path)
        {
            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path.Remove(path.Length - 1) : path;
                }
                else
                    return;


                var dirs = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories)
                    .AsParallel()//.WithDegreeOfParallelism(4)                       
                    .Select(x => new
                    {
                        dir = x.Remove(0, dir.Length),
                        deck = DeckFactory.CreateDeck(x),
                    });

                var dNw = dirs.Where(x => x.deck is CommomLibrary.Newave.Deck)
                    .Select(x => new
                    {
                        x.dir,
                        x.deck,
                        result = x.deck.GetResults()
                    }).Where(x => x.result != null).ToList();

                var dDc = dirs.Where(x => x.deck is CommomLibrary.Decomp.Deck).AsParallel()
                    .Select(x => new
                    {
                        x.dir,
                        x.deck,
                        result = x.deck.GetResults()
                    }).Where(x => x.result != null).ToList();

                if (dNw.Count() > 0) FormViewer.Show("NEWAVE", dNw.Select(x => x.result).ToArray());
                if (dDc.Count() > 0) FormViewer.Show("DECOMP", dDc.Select(x => x.result).ToArray());

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void dgernwd(string path)
        {
            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;

                Services.Deck.CreateDgerNewdesp(dir);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        static void duplicar(string path)
        {

            string newPath;

            duplicar(path, out newPath);

        }

        static void duplicar(string path, out string newPath)
        {
            newPath = "";
            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;

                var dirInfo = new DirectoryInfo(dir);
                var parentDir = dirInfo.Parent.FullName;
                var dirName = dirInfo.Name;

                var i = 0;
                var cloneDir = "";
                do
                {
                    cloneDir = Path.Combine(parentDir, dirName + " (" + ++i + ")");
                } while (Directory.Exists(cloneDir));

                var deck = DeckFactory.CreateDeck(dir);

                newPath = cloneDir;

                deck.CopyFilesToFolder(cloneDir);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void ons2ccee(string commands)
        {
            //Z:\6_decomp\03_Casos\2019_04\deck_newave_2019_04
            //"Z:\\6_decomp\\03_Casos\\2019_05\\DEC_ONS_052019_RV1_VE"

            var command = commands.Split('|');

            //var data = command[0].Substring(command[0].Length - 7, 7).Split('_');
            var path = command[0];

            try
            {
                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;

                var dirInfo = new DirectoryInfo(dir);
                var parentDir = dirInfo.Parent.FullName;
                var dirName = dirInfo.Name + "_ccee";

                var i = 0;
                var cloneDir = "";
                do
                {
                    cloneDir = Path.Combine(parentDir, dirName + " (" + ++i + ")");
                } while (Directory.Exists(cloneDir));



                var deck = DeckFactory.CreateDeck(dir);

                if (!(deck is Compass.CommomLibrary.Newave.Deck || deck is Compass.CommomLibrary.Decomp.Deck))
                {
                    throw new NotImplementedException("Deck não reconhecido para a execução");
                }

                deck.CopyFilesToFolder(cloneDir);
                dynamic cceeDeck = DeckFactory.CreateDeck(cloneDir);


                if (cceeDeck is Compass.CommomLibrary.Newave.Deck && (command.Length > 1 && command[1] == "true"))
                {
                    var frm = new FrmOnsReCcee(cceeDeck);
                    frm.Salvar();
                    //PreliminarAutorun(cceeDeck.BaseFolder, "/home/compass/sacompass/previsaopld/cpas_ctl_common/scripts/newave25.sh");
                    PreliminarAutorun(cceeDeck.BaseFolder, "/home/compass/sacompass/previsaopld/cpas_ctl_common/scripts/newave27.sh");
                }
                else if (cceeDeck is Compass.CommomLibrary.Decomp.Deck && (command.Length > 1 && command[1] == "true"))
                {
                    var frm = new FrmDcOns2Ccee(cceeDeck);
                    frm.Salvar();

                    var frmCortes = new FrmCortes(new string[] { cceeDeck.BaseFolder });
                    frmCortes.OK(true);

                    PreliminarAutorun(cceeDeck.BaseFolder, "/home/compass/sacompass/previsaopld/cpas_ctl_common/scripts/decomp301Viab.sh preliminar");
                }
                else if (cceeDeck is Compass.CommomLibrary.Newave.Deck)
                {
                    Thread thread = new Thread(nwOnsReCcee);
                    thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA                                                                  //thread.Start(redat);
                    thread.Start(cceeDeck);
                    thread.Join();
                }
                else if (cceeDeck is Compass.CommomLibrary.Decomp.Deck)
                {
                    Thread thread = new Thread(dcOns2CceeSTA);
                    thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                    thread.Start(cceeDeck);
                    thread.Join(); //Wait for the thread to end   
                }
            }
            catch (Exception ex)
            {
                var texto = ex.ToString();
                if (ex.ToString().Contains("reconhecido"))
                {
                    texto = "Deck não reconhecido para a execução por falta de arquivos!";
                }
                Compass.CommomLibrary.Tools.SendMail(texto, "alex.marques@cpas.com.br; bruno.araujo@cpas.com.br; pedro.modesto@cpas.com.br; natalia.biondo@cpas.com.br;", "Falha ao converter deck");


            }

        }

        public static void AtualizarCadastroPLD(string path, int ano, double pldMin, double pldMax, double pldMaxEst)
        {

            CommomLibrary.PldDessem.PldDessem limites = new CommomLibrary.PldDessem.PldDessem();


            var pldLimitesLines = File.ReadAllLines(@"C:\Sistemas\PricingExcelTools\files\PLD_SEMI_HORA.txt").Skip(1).ToList();
            foreach (var line in pldLimitesLines)
            {
                var dados = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (Convert.ToInt32(dados[0]) == ano)
                {
                    limites.Ano = Convert.ToInt32(dados[0].Replace('.', ','));
                    limites.PldMin = Convert.ToDouble(dados[1].Replace('.', ','));
                    limites.PldMax = Convert.ToDouble(dados[2].Replace('.', ','));
                    limites.PldMaxEst = Convert.ToDouble(dados[3].Replace('.', ','));
                    if (limites.PldMin != pldMin || limites.PldMax != pldMax || limites.PldMaxEst != pldMaxEst)
                    {
                        if (System.Windows.Forms.MessageBox.Show("ATENÇÃO!!!\nOs valores informados são diferentes dos padrões.\nDeseja continuar?", "Trata PLD", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (System.Windows.Forms.MessageBox.Show("ATENÇÃO!!!\nDeseja atualizar os dados padrões para o ano informado?", "Trata PLD", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            {
                                var novotexto = new List<string>();
                                var texto = File.ReadAllLines(@"C:\Sistemas\PricingExcelTools\files\PLD_SEMI_HORA.txt").ToList();
                                novotexto.Add(texto[0]);
                                for (int i = 1; i < texto.Count(); i++)
                                {
                                    var partes = texto[i].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                                    var anoTeste = Convert.ToInt32(partes[0].Replace('.', ','));
                                    if (anoTeste == ano)
                                    {
                                        novotexto.Add(ano.ToString() + "\t" + pldMin.ToString().Replace('.', ',') + "\t" + pldMax.ToString().Replace('.', ',') + "\t" + pldMaxEst.ToString().Replace('.', ','));
                                    }
                                    else
                                    {
                                        novotexto.Add(texto[i]);
                                    }

                                }
                                File.WriteAllLines(@"C:\Sistemas\PricingExcelTools\files\PLD_SEMI_HORA.txt", novotexto);

                            }
                            TrataPld(path, ano, pldMin, pldMax, pldMaxEst);
                            break;

                        }
                    }
                    else
                    {
                        TrataPld(path, ano, pldMin, pldMax, pldMaxEst);
                        break;
                    }
                }
            }
            if (limites.Ano == 0)
            {
                if (System.Windows.Forms.MessageBox.Show("ATENÇÃO!!!\nOs valores para o ano informado ainda não existem no cadastro.\nDeseja continuar?", "Trata PLD", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (System.Windows.Forms.MessageBox.Show("Deseja incluir os valores no cadastro?", "Trata PLD", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        var texto = File.ReadAllLines(@"C:\Sistemas\PricingExcelTools\files\PLD_SEMI_HORA.txt").ToList();
                        texto.Add(ano.ToString() + "\t" + pldMin.ToString().Replace('.', ',') + "\t" + pldMax.ToString().Replace('.', ',') + "\t" + pldMaxEst.ToString().Replace('.', ','));
                        File.WriteAllLines(@"C:\Sistemas\PricingExcelTools\files\PLD_SEMI_HORA.txt", texto);

                        TrataPld(path, ano, pldMin, pldMax, pldMaxEst);
                    }
                    else
                    {
                        TrataPld(path, ano, pldMin, pldMax, pldMaxEst);
                    }

                }
            }


        }
        public static void TrataPld(string path, int ano, double pldMin, double pldMax, double pldMaxEst)
        {
            var dir = path;
            var anoPld = ano;
            var limInf = pldMin;
            var limMax = pldMax;
            var limEst = pldMaxEst;
            int i;

            List<Tuple<int, string, double>> Plds = new List<Tuple<int, string, double>>();


            var pmoFile = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).ToLower().Contains("pdo_cmosist.dat")).FirstOrDefault();
            if (pmoFile != null)
            {

                var linhas = File.ReadAllLines(pmoFile);
                foreach (var l in linhas)
                {
                    var campos = l.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    if (l != "")
                    {
                        if (int.TryParse(campos[0], System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out i))
                        {
                            Tuple<int, string, double> Pld = new Tuple<int, string, double>(i, campos[2].Trim(), Convert.ToDouble(campos[3].Replace('.', ',')));
                            Plds.Add(Pld);

                        }
                    }

                }

                List<Tuple<int, double>> dadosSE = new List<Tuple<int, double>>();
                List<Tuple<int, double>> dadosSUL = new List<Tuple<int, double>>();
                List<Tuple<int, double>> dadosNE = new List<Tuple<int, double>>();
                List<Tuple<int, double>> dadosN = new List<Tuple<int, double>>();
                List<Tuple<int, double>> dadosFC = new List<Tuple<int, double>>();


                var PldSE = Plds.Where(x => x.Item2 == "SE").ToList();
                var PldSUL = Plds.Where(x => x.Item2 == "S").ToList();
                var PldNE = Plds.Where(x => x.Item2 == "NE").ToList();
                var PldN = Plds.Where(x => x.Item2 == "N").ToList();
                var PldFC = Plds.Where(x => x.Item2 == "FC").ToList();

                int hora = 1;
                for (int h = 1; h <= 48; h += 2)
                {
                    var hora1SE = PldSE.Where(x => x.Item1 == h).Select(x => x.Item3).First();
                    var hora2SE = PldSE.Where(x => x.Item1 == (h + 1)).Select(x => x.Item3).First();
                    var mediaSE = (hora1SE + hora2SE) / 2;
                    dadosSE.Add(new Tuple<int, double>(hora, mediaSE));

                    var hora1S = PldSUL.Where(x => x.Item1 == h).Select(x => x.Item3).First();
                    var hora2S = PldSUL.Where(x => x.Item1 == (h + 1)).Select(x => x.Item3).First();
                    var mediaS = (hora1S + hora2S) / 2;
                    dadosSUL.Add(new Tuple<int, double>(hora, mediaS));

                    var hora1NE = PldNE.Where(x => x.Item1 == h).Select(x => x.Item3).First();
                    var hora2NE = PldNE.Where(x => x.Item1 == (h + 1)).Select(x => x.Item3).First();
                    var mediaNE = (hora1NE + hora2NE) / 2;
                    dadosNE.Add(new Tuple<int, double>(hora, mediaNE));

                    var hora1N = PldN.Where(x => x.Item1 == h).Select(x => x.Item3).First();
                    var hora2N = PldN.Where(x => x.Item1 == (h + 1)).Select(x => x.Item3).First();
                    var mediaN = (hora1N + hora2N) / 2;
                    dadosN.Add(new Tuple<int, double>(hora, mediaN));

                    var hora1FC = PldFC.Where(x => x.Item1 == h).Select(x => x.Item3).First();
                    var hora2FC = PldFC.Where(x => x.Item1 == (h + 1)).Select(x => x.Item3).First();
                    var mediaFC = (hora1FC + hora2FC) / 2;
                    dadosFC.Add(new Tuple<int, double>(hora, mediaFC));

                    hora++;
                }

                var finalSE = GetPLdDessem(dadosSE, limInf, limMax, limEst);
                var finalSUL = GetPLdDessem(dadosSUL, limInf, limMax, limEst);
                var finalNE = GetPLdDessem(dadosNE, limInf, limMax, limEst);
                var finalN = GetPLdDessem(dadosN, limInf, limMax, limEst);
                var finalFC = GetPLdDessem(dadosFC, limInf, limMax, limEst);

                StringBuilder pldDados = new StringBuilder();
                pldDados.AppendFormat("{0,4}", "HORA");
                pldDados.AppendFormat("{0,6}", "SIST");
                pldDados.AppendFormat("{0,8}", "PLD");
                pldDados.AppendLine();

                foreach (var dad in finalSE)
                {
                    pldDados.AppendFormat("{0,4}", $"{dad.Item1}");
                    pldDados.AppendFormat("{0,6}", "SE");
                    pldDados.AppendFormat("{0,8}", $"{Math.Round(dad.Item2, 2)}");
                    pldDados.AppendLine();

                }
                foreach (var dad in finalSUL)
                {
                    pldDados.AppendFormat("{0,4}", $"{dad.Item1}");
                    pldDados.AppendFormat("{0,6}", "S");
                    pldDados.AppendFormat("{0,8}", $"{Math.Round(dad.Item2, 2)}");
                    pldDados.AppendLine();

                }
                foreach (var dad in finalNE)
                {
                    pldDados.AppendFormat("{0,4}", $"{dad.Item1}");
                    pldDados.AppendFormat("{0,6}", "NE");
                    pldDados.AppendFormat("{0,8}", $"{Math.Round(dad.Item2, 2)}");
                    pldDados.AppendLine();

                }
                foreach (var dad in finalN)
                {
                    pldDados.AppendFormat("{0,4}", $"{dad.Item1}");
                    pldDados.AppendFormat("{0,6}", "N");
                    pldDados.AppendFormat("{0,8}", $"{Math.Round(dad.Item2, 2)}");
                    pldDados.AppendLine();

                }
                foreach (var dad in finalFC)
                {
                    pldDados.AppendFormat("{0,4}", $"{dad.Item1}");
                    pldDados.AppendFormat("{0,6}", "FC");
                    pldDados.AppendFormat("{0,8}", $"{Math.Round(dad.Item2, 2)}");
                    pldDados.AppendLine();

                }
                File.WriteAllText(Path.Combine(dir, "PLD_HORARIO.txt"), pldDados.ToString());

                var textoFinal = "Processo realizado com sucesso!";
                Program.AutoClosingMessageBox.Show(textoFinal, "Trata PLD", 5000);
            }
            else
            {
                var textoFinal = "pdo_cmosist.dat não existe!!!. Encerrando processo.";
                Program.AutoClosingMessageBox.Show(textoFinal, "Trata PLD", 30000);
            }
        }

        static List<Tuple<int, double>> GetPLdDessem(List<Tuple<int, double>> mediasHoras, double limInf, double limMax, double limEst)
        {
            var lista = verificaLimitesPLd(mediasHoras, limInf, limMax, limEst);
            int cont = 0;
            var mediaDiaria = lista.Average(x => x.Item2);

            if (mediaDiaria > limEst)
            {
                var pldHoras = lista.Select(x => x.Item2).ToList();

                while (mediaDiaria > limEst && cont <= 30)
                {
                    var fator = limEst / mediaDiaria;
                    List<double> valores = new List<double>();
                    foreach (var item in pldHoras)
                    {
                        var val = item * fator;
                        valores.Add(val);
                    }

                    mediaDiaria = valores.Average();
                    pldHoras = valores;
                    cont++;
                }
                List<Tuple<int, double>> listaAjustada = new List<Tuple<int, double>>();
                for (int i = 1; i <= pldHoras.Count(); i++)
                {
                    listaAjustada.Add(new Tuple<int, double>(i, pldHoras[i - 1]));
                }

                return listaAjustada;
            }
            return lista;
        }

        static List<Tuple<int, double>> verificaLimitesPLd(List<Tuple<int, double>> mediasHoras, double limInf, double limMax, double limEst)
        {
            List<Tuple<int, double>> lista = new List<Tuple<int, double>>();
            double valor = 0;
            foreach (var dado in mediasHoras)
            {
                if (dado.Item2 < limInf)
                {
                    valor = limInf;
                }
                else if (dado.Item2 > limMax)
                {
                    valor = limMax;
                }
                else
                {
                    valor = dado.Item2;
                }
                lista.Add(new Tuple<int, double>(dado.Item1, valor));
            }

            return lista;
        }

        static void uhDessem(string path)
        {
            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path.Remove(path.Length - 1) : path;
                }
                else
                    return;
                var dirs = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories);


                var texto = "Escolha o arquivo base!";
                MessageBox.Show(texto, "Caption");

                Thread thread = new Thread(GetPDO_OPER);
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start(dirs);
                thread.Join(); //Wait for the thread to end 

               




            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        static void pldDessem(string commands)
        {

            var command = commands.Split('|');

            var path = command[0];

            if (command.Count() > 1 && command[1] == "true")
            {
                var ano = DateTime.Today.Year;
                CommomLibrary.PldDessem.PldDessem limites = new CommomLibrary.PldDessem.PldDessem();


                var pldLimitesLines = File.ReadAllLines(@"C:\Sistemas\PricingExcelTools\files\PLD_SEMI_HORA.txt").Skip(1).ToList();
                foreach (var line in pldLimitesLines)
                {
                    var dados = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    if (Convert.ToInt32(dados[0]) == ano)
                    {
                        limites.Ano = Convert.ToInt32(dados[0].Replace('.', ','));
                        limites.PldMin = Convert.ToDouble(dados[1].Replace('.', ','));
                        limites.PldMax = Convert.ToDouble(dados[2].Replace('.', ','));
                        limites.PldMaxEst = Convert.ToDouble(dados[3].Replace('.', ','));
                    }
                }
                if (limites.Ano == 0)
                {
                    var dados = pldLimitesLines.Last().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    limites.Ano = Convert.ToInt32(dados[0].Replace('.', ','));
                    limites.PldMin = Convert.ToDouble(dados[1].Replace('.', ','));
                    limites.PldMax = Convert.ToDouble(dados[2].Replace('.', ','));
                    limites.PldMaxEst = Convert.ToDouble(dados[3].Replace('.', ','));
                }
                TrataPld(path, limites.Ano, limites.PldMin, limites.PldMax, limites.PldMaxEst);
            }
            else
            {
                Thread thread = new Thread(pldDessemTHSTA);
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start(path);
                thread.Join(); //Wait for the thread to end  
            }




        }

        static void Corteshdecodess(object caminho)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "cortesh.*|cortesh.*";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var arqName = ofd.FileName;
                if (arqName != null || arqName != "")
                {
                    File.Copy(ofd.FileName, Path.Combine(caminho.ToString(), ofd.FileName.Split('\\').Last()), true);
                }
            }
        }

        static void GetPDO_OPER(object dirs)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PDO_OPER_USIH.*|PDO_OPER_USIH.*";//"PDO_OPER_USIH.*|pdo_oper_usih.*"
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var arqName = ofd.FileName;
                if (arqName != null || arqName != "")
                {
                    var pdoOper = File.ReadAllLines(ofd.FileName);
                    string linhadata = pdoOper.Where(x => x.Contains("Data do Caso")).First();
                    DateTime dataPdo = Convert.ToDateTime(linhadata.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last());
                    var revisao = Tools.GetCurrRev(dataPdo);
                    List<Tuple<int, int, float, float>> UHS = new List<Tuple<int, int, float, float>>();
                    var feriados = Tools.feriados;
                    int atualizados = 0;
                    string decksAtualizados = "Decks atualizados:\n";
                    for (int i = 62; i < pdoOper.Count(); i++)
                    {
                        
                        if (pdoOper[i] != "")
                        {
                            float d = 0;
                            var campos = pdoOper[i].Split(';').ToList();
                            var hora = Convert.ToInt32(campos[0]);
                            var usina = Convert.ToInt32(campos[2]);
                            var volIni = float.TryParse(campos[6], System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out d) ? d : 0;
                            var volFim = float.TryParse(campos[8], System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out d) ? d : 0;
                            UHS.Add(new Tuple<int, int, float, float>(hora, usina, volIni, volFim));
                        }

                    }


                    foreach (var dir in (string[])dirs)
                    {
                       
                        var dadvazFile = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).ToLower().Contains("dadvaz.dat")).First();
                        var entdadosFile = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).ToLower().Contains("entdados.dat")).First();
                        if (dadvazFile != null && entdadosFile != null)
                        {
                            var dadvaz = DocumentFactory.Create(dadvazFile) as Compass.CommomLibrary.Dadvaz.Dadvaz;
                            var dataline = dadvaz.BlocoData.First();
                            DateTime dataDeck = new DateTime(dataline.Ano, dataline.Mes, dataline.Dia);

                            var entdados = DocumentFactory.Create(entdadosFile) as Compass.CommomLibrary.EntdadosDat.EntidadosDat;
                            var uhLines = entdados.BlocoUh.ToList();
                            int horaUHS = 44;
                            int incremento = 0;
                            for (DateTime dia = dataPdo; dia <= revisao.revDate; dia = dia.AddDays(1))
                            {
                                if (dia == dataDeck)
                                {
                                    horaUHS = horaUHS + incremento;
                                }
                                incremento = incremento + 4;
                            }

                            if (dataDeck >= dataPdo && dataDeck <= revisao.revDate)
                            {
                                if (dataDeck == dataPdo)
                                {
                                    foreach (var line in uhLines)
                                    {
                                        
                                        var vol = UHS.Where(x => x.Item1 == 1 && x.Item2 == line.Usina).Select(x => x.Item3).FirstOrDefault();
                                        if (vol > 0)
                                        {
                                            line.VolArm = vol;
                                        }

                                    }
                       
                                }
                                else
                                {
                                    foreach (var line in uhLines)
                                    {
                                        var vol = UHS.Where(x => x.Item1 == horaUHS && x.Item2 == line.Usina).Select(x => x.Item4).First();
                                        if (vol > 0)
                                        {
                                            line.VolArm = vol;
                                        }

                                    }
                                }

                                atualizados++;
                                decksAtualizados = decksAtualizados + dir.Split('\\').Last() + "\n";

                                entdados.SaveToFile(createBackup: true);

                            }
                        }
                        
                    }

                    if (atualizados > 0)
                    {
                        var texto = decksAtualizados;
                        MessageBox.Show(texto, "Caption");
                    }
                    else
                    {
                        var texto = "Os decks não foram atualizados.Arquivo base incompatível.";
                        MessageBox.Show(texto, "Caption");
                    }

                    //File.Copy(ofd.FileName, Path.Combine(dirs.ToString(), ofd.FileName.Split('\\').Last()), true);
                }
            }
        }

        static void convDecodess(string commands)
        {
            string camArqsBase = "C:\\Files\\Middle - Preço\\Resultados_Modelos\\DECODESS\\Arquivos_Base";
            var arqsBase = Directory.GetFiles(camArqsBase).ToList();
            List<string> arqsBaseAlvo = new List<string> { "renovaveis", "ils_tri", "config", "decodess", "cadterm", "curvtviag", "cotasr11", "areacont", "rstlpp", "respotele", "restseg" };
            DateTime dataEstudo = new DateTime();
            string camCortes = string.Empty;


            var command = commands.Split('|');

            var path = command[0];
            var cloneDir = "";

            try
            {
                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;

                var dirInfo = new DirectoryInfo(dir);
                var parentDir = dirInfo.Parent.FullName;
                var dirName = dirInfo.Name + "_Decodess";

                var i = 0;
                do
                {
                    cloneDir = Path.Combine(parentDir, dirName + " (" + ++i + ")");
                } while (Directory.Exists(cloneDir));

                if (!Directory.Exists(cloneDir))
                {
                    Directory.CreateDirectory(cloneDir);
                }

                var deck = DeckFactory.CreateDeck(dir) as Compass.CommomLibrary.Decomp.Deck;

                if (!(deck is Compass.CommomLibrary.Decomp.Deck))
                {
                    throw new NotImplementedException("Deck não reconhecido para a execução");
                }
                if (deck is Compass.CommomLibrary.Decomp.Deck)
                {
                    List<string> arqsAlvo = new List<string> { "dadger", "vazoes.rv", "hidr", "mapcut", "cortdeco", "mlt", "dadgnl", "relato.rv", "relgnl" };
                    var arqsDecomp = Directory.GetFiles(dir).ToList();
                    foreach (var arqs in arqsDecomp.Where(x => (!x.ToLower().Contains(".bak")) && (!x.ToLower().Contains(".origjirstoant")) && (!x.ToLower().Contains(".temp.modif"))).ToList())
                    {
                        var fileName = Path.GetFileName(arqs);
                        foreach (var item in arqsAlvo)
                        {
                            if (fileName.ToLower().Contains(item))
                            {
                                File.Copy(arqs, Path.Combine(cloneDir, fileName), true);
                            }
                        }
                    }

                    foreach (var arq in arqsBase)
                    {
                        var fileBase = Path.GetFileName(arq);
                        foreach (var item in arqsBaseAlvo)
                        {
                            if (fileBase.ToLower().Contains(item))
                            {
                                File.Copy(arq, Path.Combine(cloneDir, fileBase), true);
                            }
                        }
                    }
                    var dadgerFile = Directory.GetFiles(cloneDir).Where(x => Path.GetFileName(x).ToLower().Contains("dadger")).First();
                    //// var deckDecodessBase = DeckFactory.CreateDeck(cloneDir) as Compass.CommomLibrary.Decomp.Deck;
                    var dadgerBase = deck[CommomLibrary.Decomp.DeckDocument.dadger].Document as Compass.CommomLibrary.Dadger.Dadger;
                    var dadger = DocumentFactory.Create(dadgerFile) as Compass.CommomLibrary.Dadger.Dadger;

                    foreach (var line in dadger.BlocoAc.ToList())
                    {
                        if (line.Usina == 285 || line.Usina == 287)
                        {
                            if (line.Mnemonico.Contains("JUSMED") || line.Mnemonico.Contains("COTVOL"))
                            {
                                line[0] = "&" + line[0];
                            }
                        }
                    }


                    for (int u = 171; u <= 174; u++)
                    {
                        foreach (var line in dadger.BlocoCT.Where(x => x.Cod == u).ToList())
                        {
                            if (line.Cod == u)
                            {

                                line.Cvu1 = 0;
                                line.Cvu2 = 0;
                                line.Cvu3 = 0;
                                line.Cod = 60;
                            }
                        }
                    }

                    foreach (var line in dadger.BlocoCT.Where(x => x.Cod == 183 || x.Cod == 65).ToList())
                    {
                        line.Cvu1 = 0;
                        line.Cvu2 = 0;
                        line.Cvu3 = 0;
                        line.Cod = 383;

                    }

                    foreach (var line in dadger.BlocoCT.Where(x => x.Cod == 463).ToList())
                    {

                        line[0] = "&" + line[0];

                    }

                    dataEstudo = dadger.DataEstudo;
                    var fc = (Compass.CommomLibrary.Dadger.FcBlock)dadger.Blocos["FC"];

                    camCortes = dadgerBase.CortesPath.ToLower().Replace("cortes.dat", "cortesh.dat");
                    if (File.Exists(camCortes))
                    {
                        File.Copy(camCortes, Path.Combine(cloneDir, camCortes.Split('\\').Last()));
                    }
                    else
                    {
                        if (command.Count() < 2)
                        {
                            var texto = "Camminho do arquivo Cortesh.dat não existe, defina um caminho existente";
                            MessageBox.Show(texto, "ATENCÃO!");

                            Thread thread = new Thread(Corteshdecodess);
                            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                            thread.Start(cloneDir);
                            thread.Join(); //Wait for the thread to end      


                        }
                    }
                    var arqsClonedir = Directory.GetFiles(cloneDir);
                    if (arqsClonedir.All(x => !x.ToLower().Contains("cortesh")))
                    {
                        throw new NotImplementedException("Cortesh não encontrado para a execução");
                    }

                    var todosArqs = arqsAlvo.Union(arqsBaseAlvo);
                    foreach (var item in todosArqs)
                    {
                        if (arqsClonedir.All(x => !x.ToLower().Contains(item)))
                        {
                            throw new NotImplementedException($"{item} não encontrado para a execução");
                        }
                    }



                    dadger.SaveToFile();
                    TrataBlocoRHEDessem(dadgerFile);//desloca as colunas referentes ao numero das restrições para a esquerda pq o decodess esta considerando as colunas de 5 a 7 
                    var configFile = Directory.GetFiles(cloneDir).Where(x => Path.GetFileName(x).ToLower().Contains("config.dat")).First();
                    var decodessFile = Directory.GetFiles(cloneDir).Where(x => Path.GetFileName(x).ToLower().Contains("decodess.arq")).First();

                    TrataConfig(configFile, dataEstudo);
                    TrataDecodessArq(decodessFile, cloneDir);
                    ExecutaDecodess(cloneDir);

                    if (command.Count() < 2)
                    {
                        if (System.Windows.Forms.MessageBox.Show("Deseja criar decks diários? ", "Decodess", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            CriarDecksDiarios(cloneDir, dataEstudo);
                        }
                    }

                    else if (command.Count() > 1 && command[1] == "true")
                    {
                        CriarDecksDiarios(cloneDir, dataEstudo);
                    }
                    var textoFinal = "Conversão realizada com sucesso!";
                    Program.AutoClosingMessageBox.Show(textoFinal, "Caption", 5000);
                    if (command.Count() > 1 && command[1] == "true")
                    {

                        Compass.CommomLibrary.Tools.SendMail(textoFinal, "carlos.paes@cpas.com.br; thamires.baptista@cpas.com.br; alex.marques@cpas.com.br; bruno.araujo@cpas.com.br; pedro.modesto@cpas.com.br; natalia.biondo@cpas.com.br;", "Conversão Decodess");

                    }
                }


                //deck.CopyFilesToFolder(cloneDir);

                //dynamic cceeDeck = DeckFactory.CreateDeck(cloneDir);
            }
            catch (Exception ex)
            {
                if (command.Count() > 1 && command[1] == "true")
                {
                    var texto = ex.ToString();
                    if (ex.ToString().Contains("reconhecido"))
                    {
                        texto = "Deck não reconhecido para a execução por falta de arquivos!";
                    }
                    Compass.CommomLibrary.Tools.SendMail(texto, "carlos.paes@cpas.com.br; thamires.baptista@cpas.com.br; alex.marques@cpas.com.br; bruno.araujo@cpas.com.br; pedro.modesto@cpas.com.br; natalia.biondo@cpas.com.br;", "Falha na conversão Decodess");
                    if (Directory.Exists(cloneDir))
                    {
                        Directory.Delete(cloneDir, true);
                    }
                }
                else
                {
                    var texto = "Falha na execução! Erro: " + ex.ToString();
                    Program.AutoClosingMessageBox.Show(texto, "Caption", 5000);
                    if (Directory.Exists(cloneDir))
                    {
                        Directory.Delete(cloneDir, true);
                    }
                }
            }
        }

        public static void CriarDecksDiarios(string dirBase, DateTime dataEstudo)
        {
            var rev = Tools.GetCurrRev(dataEstudo);
            var camDessemDiario = $@"Z:\7_dessem\DecksDiarios\{rev.revDate:MM_yyyy}\RV{rev.rev}\{DateTime.Now:dd_MM_yyyy_HH_mm_ss}";
            var camdessemBase = camDessemDiario + "\\Base";
            if (!Directory.Exists(camdessemBase))
            {
                Directory.CreateDirectory(camdessemBase);
            }
            var arqsBase = Directory.GetFiles(dirBase);

            foreach (var arq in arqsBase)
            {
                File.Copy(arq, Path.Combine(camdessemBase, arq.Split('\\').Last()), true);
            }

            DownloadOperuh(camdessemBase, dataEstudo, rev.revDate);

            for (DateTime d = dataEstudo; d <= rev.revDate; d = d.AddDays(1))
            {
                var path = $"{camDessemDiario}\\Dessem_{d:dd_MM_yyyy}";
                CopiaArqsDessem(camdessemBase, path, d);
                CopiaTermdat(path);
                ComentaDessemArq(path);
                CriarDadvaz(path, d);
                TrataPtoper(path, d);
                Services.GeraDessem.CriarEntdados(path, d, rev.revDate);

            }
        }

        public static void DownloadOperuh(string dirBase, DateTime ini, DateTime fim)
        {

            string localpath = Path.Combine(Path.GetTempPath(), "operuh");


            if (!Directory.Exists(localpath))
            {
                Directory.CreateDirectory(localpath);
            }

            bool existe = false;
            int cont = 0;
            ChromeOptions options = new ChromeOptions();

            options.AddUserProfilePreference("download.default_directory", localpath);
            options.AddArgument("--no-sandbox");
            options.AddArgument("--verbose");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-software-rasterizer");
            
            IWebDriver driver = new ChromeDriver("C:\\Sistemas\\PricingExcelTools\\files\\chromedriver_win32",options);
            
            try
            {
                //driver.Manage().Window.Maximize();
                driver.Url = "https://integracaoagentes.ons.org.br/FSAR-H/SitePages/Exibir_Forms_FSARH.aspx#";
                driver.FindElement(By.Id("username")).SendKeys("bruno.araujo@cpas.com.br");
                driver.FindElement(By.Name("submit.IdentificarUsuario")).Click();
                driver.FindElement(By.Id("password")).SendKeys("Br@compass");
                driver.FindElement(By.Name("submit.Signin")).Click();
                Thread.Sleep(10000);

                for (DateTime d = ini; d <= fim; d = d.AddDays(1))
                {
                    existe = false;
                    cont = 0;
                    while (existe == false && cont <= 5)
                    {

                        driver.FindElement(By.Id("buttonGerarArq")).Click();
                        Thread.Sleep(2000);

                        driver.FindElement(By.Id("dessemDataInicial")).Clear();
                        Thread.Sleep(1000);

                        driver.FindElement(By.Id("dessemDataInicial")).SendKeys(d.ToString("dd/MM/yyyy"));
                        Thread.Sleep(1000);

                        driver.FindElement(By.Id("dessemDataFinal")).Clear();
                        Thread.Sleep(1000);

                        driver.FindElement(By.Id("dessemDataFinal")).SendKeys(fim.ToString("dd/MM/yyyy"));
                        Thread.Sleep(1000);

                        driver.FindElement(By.Id("gerarTxtDessem")).Click();

                        // WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                        Thread.Sleep(7000);
                        // wait.Until<bool>(x => existe = Directory.GetFiles(localpath).Any(y => Path.GetFileName(y).ToLower().Contains("operuh") && Path.GetFileName(y).ToLower().EndsWith(".dat")));


                        var arq = Directory.GetFiles(localpath).Where(y => Path.GetFileName(y).ToLower().Contains("operuh") && Path.GetFileName(y).ToLower().EndsWith(".dat")).FirstOrDefault();

                        if (arq == null || arq == "")
                        {
                            Thread.Sleep(10000);
                            arq = Directory.GetFiles(localpath).Where(y => Path.GetFileName(y).ToLower().Contains("operuh") && Path.GetFileName(y).ToLower().EndsWith(".dat")).FirstOrDefault();

                        }

                        if (File.Exists(arq))
                        {
                            File.Move(arq, Path.Combine(dirBase, $"operuh_{d:ddMMyyyy}.DAT"));
                            existe = true;
                            if (File.Exists(arq))
                            {
                                File.Delete(arq);
                            }
                        }
                        else
                        {
                            cont++;
                        }
                        if (cont > 5)
                        {
                            throw new NotImplementedException("Falha ao baixar operuh.dat!!! Tentativas excedidas");
                        }
                    }
                    
                }

                if (Directory.Exists(localpath))
                {
                    Directory.Delete(localpath, true);
                }
                driver.Quit();
            }
            catch (Exception e)
            {
                if (Directory.Exists(localpath))
                {
                    Directory.Delete(localpath, true);
                }
                if (Directory.Exists(dirBase))
                {
                    Directory.Delete(dirBase, true);
                }
                driver.Quit();
                throw;
            }
        }
        public static void CopiaTermdat(string path)
        {
            string camArqsBase = "C:\\Files\\Middle - Preço\\Resultados_Modelos\\DECODESS\\Arquivos_Base";
            var termdatFile = Directory.GetFiles(camArqsBase).Where(x => Path.GetFileName(x).ToLower().Contains("termdat.dat")).First();
            File.Copy(termdatFile, Path.Combine(path, termdatFile.Split('\\').Last()), true);
        }
        public static void TrataPtoper(string path, DateTime data)
        {
            string camArqsBase = "C:\\Files\\Middle - Preço\\Resultados_Modelos\\DECODESS\\Arquivos_Base";

            var ptoperFile = Directory.GetFiles(camArqsBase).Where(x => Path.GetFileName(x).ToLower().Contains("ptoper")).FirstOrDefault();

            if (ptoperFile != null)
            {
                File.Copy(ptoperFile, Path.Combine(path, ptoperFile.Split('\\').Last()), true);

            }

            //var ptoper = DocumentFactory.Create(ptoperFile) as Compass.CommomLibrary.PtoperDat.PtoperDat;

            //foreach (var line in ptoper.BlocoPtoper.ToList())
            //{
            //    //line.DiaIni = data.Day.ToString();
            //}
            //ptoper.SaveToFile();
        }
        public static void CriarDadvaz(string path, DateTime data)
        {
            var dadvazFile = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("dadvaz.dat")).First();
            var dadvaz = DocumentFactory.Create(dadvazFile) as Compass.CommomLibrary.Dadvaz.Dadvaz;
            var dataLine = dadvaz.BlocoData.First();
            dataLine.Dia = data.Day;
            dataLine.Mes = data.Month;
            dataLine.Ano = data.Year;

            var diaLine = dadvaz.BlocoDia.First();
            int dia = 0;
            switch (data.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    dia = 1;
                    break;
                case DayOfWeek.Sunday:
                    dia = 2;
                    break;
                case DayOfWeek.Monday:
                    dia = 3;
                    break;
                case DayOfWeek.Tuesday:
                    dia = 4;
                    break;
                case DayOfWeek.Wednesday:
                    dia = 5;
                    break;
                case DayOfWeek.Thursday:
                    dia = 6;
                    break;
                case DayOfWeek.Friday:
                    dia = 7;
                    break;
                default:
                    dia = 1;
                    break;

            }
            diaLine.diainicial = dia;

            var vazoes = dadvaz.BlocoVazoes.ToList();

            foreach (var vaz in vazoes)
            {
                vaz.DiaInic = data.Day.ToString();
            }


            dadvaz.SaveToFile();
        }
        public static void ComentaDessemArq(string path)
        {
            var dessemArqFile = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("dessem.arq")).First();
            var dessemArq = DocumentFactory.Create(dessemArqFile) as Compass.CommomLibrary.DessemArq.DessemArq;

            var curvtviagFile = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("curvtviag.dat")).First();
            var curvtviagName = Path.GetFileName(curvtviagFile);

            var cotasr11File = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("cotasr11.dat")).First();
            var cotasr11Name = Path.GetFileName(cotasr11File);

            var termdatFile = Directory.GetFiles(path).Where(x => Path.GetFileName(x).ToLower().Contains("termdat.dat")).First();
            var termdatName = Path.GetFileName(termdatFile);


            foreach (var line in dessemArq.BlocoArq.ToList())
            {
                if (line.Minemonico.Contains("INDELET"))
                {
                    line.Minemonico = "&" + line.Minemonico;
                }
                if (line.Minemonico.Contains("CURVT"))
                {
                    line.Minemonico = "CURVTVIAG";
                    line.NomeArq = curvtviagName;
                }
                if (line.Minemonico.Contains("COTASR"))
                {
                    line.Minemonico = "COTASR11";
                    line.NomeArq = cotasr11Name;
                }
                if (line.Minemonico.Contains("CADTERM"))
                {
                    line.NomeArq = termdatName;
                }
                
            }


            //var lines = File.ReadAllLines(dessemArq).ToList();
            //int indice = 0;
            //if (lines.Any(x => x.StartsWith("INDELET")))
            //{

            //    indice = lines.IndexOf(lines.Where(x => x.StartsWith("INDELET")).First());
            //    string frase = lines[indice];
            //    frase = "&" + frase;
            //    lines[indice] = frase;
            //    File.WriteAllLines(dessemArq, lines);
            //}
            dessemArq.SaveToFile();
        }
        public static void CopiaArqsDessem(string fonte, string dest, DateTime data)
        {
            var arqsBase = Directory.GetFiles(fonte);
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            foreach (var arq in arqsBase)
            {//operuh_12122020
                var operuh = $"operuh_{data:ddMMyyyy}.DAT";
                var nameFile = Path.GetFileName(arq);

                if (nameFile == operuh)
                {
                    File.Copy(arq, Path.Combine(dest, "operuh.DAT"), true);

                }
                else if (!nameFile.ToLower().Contains("operuh"))
                {
                    File.Copy(arq, Path.Combine(dest, arq.Split('\\').Last()), true);
                }
            }
        }
        public static void ExecutaDecodess(string diretorio)
        {
            try
            {
                var arquivos = Directory.GetFiles(diretorio);
                var tempFolder = @"Z:\shared\DESSEM\decodess_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
                Directory.CreateDirectory(tempFolder);

                foreach (var arq in arquivos)
                {
                    File.Copy(arq, Path.Combine(tempFolder, arq.Split('\\').Last()), true);
                }

                if (Services.Linux.Run(tempFolder, @"/home/compass/sacompass/previsaopld/cpas_ctl_common/scripts/decodess.sh", "decodess", true, true, "hide"))
                {
                    var tempArqs = Directory.GetFiles(tempFolder);
                    foreach (var temps in tempArqs)
                    {
                        File.Copy(temps, Path.Combine(diretorio, temps.Split('\\').Last()), true);
                    }
                    if (Directory.Exists(tempFolder))
                        Directory.Delete(tempFolder, true);
                }
            }
            catch (Exception e)
            {

            }

        }

        public static void TrataDecodessArq(string decodessFile, string cloneDir)
        {
            var decodess = DocumentFactory.Create(decodessFile) as Compass.CommomLibrary.DecodessArq.DecodessArq;
            var arqsClonedir = Directory.GetFiles(cloneDir);
            var decoLines = decodess.BlocoArqs.ToList();

            foreach (var arq in arqsClonedir.Where(x => !x.ToLower().Contains(".bak")))
            {
                var fileName = Path.GetFileNameWithoutExtension(arq);
                var fileNameEx = Path.GetFileName(arq);

                foreach (var line in decoLines)
                {
                    if (line.NomeArq.ToLower().Contains(fileName.ToLower()))
                    {
                        line.NomeArq = fileNameEx;
                    }
                }
            }

            decodess.SaveToFile();

        }

        public static void TrataBlocoRHEDessem(string daderFile)
        {
            var linhas = File.ReadAllLines(daderFile, Encoding.UTF8).ToList();
            var novotexto = new List<string>();
            var linhaBloco = new List<string> { "RE", "LU", "FU", "FT", "FI", "FE" };

            foreach (var linha in linhas)
            {
                var dados = linha.Split(' ').ToList();
                if (linhaBloco.Any(x => x.Equals(dados[0])))
                {
                    var texto = linha;
                    texto = texto.Remove(2, 1);
                    texto = texto.Insert(7, " ");
                    novotexto.Add(texto);
                }
                else
                {
                    novotexto.Add(linha);
                }
            }
            File.WriteAllLines(daderFile, novotexto, Encoding.UTF8);
        }
        public static void TrataConfig(string configFile, DateTime dataEstudo)
        {
            Dictionary<int, Tuple<int, int>> tipoDias = new Dictionary<int, Tuple<int, int>>() {//<mes,<Tipo dia util, Tipo feriado>>
                    {1, new Tuple<int,int>(3,4)},//jan
                    {2, new Tuple<int,int>(3,4)},//fev
                    {3, new Tuple<int,int>(3,4)},//marc
                    {4, new Tuple<int,int>(1,2)},//abril
                    {5, new Tuple<int,int>(5,6)},//maio
                    {6, new Tuple<int,int>(5,6)},//jun
                    {7, new Tuple<int,int>(5,6)},//jul
                    {8, new Tuple<int,int>(5,6)},//ago
                    {9, new Tuple<int,int>(1,2)},//set
                    {10, new Tuple<int,int>(1,2)},//out
                    {11, new Tuple<int,int>(3,4)},//nov
                    {12, new Tuple<int,int>(3,4)},//dez

                };



            var feriados = Tools.feriados;
            var config = DocumentFactory.Create(configFile) as Compass.CommomLibrary.ConfigDat.ConfigDat;

            var dataLine = config.BlocoData.First();
            dataLine.Dia = dataEstudo.Day;
            dataLine.Mes = dataEstudo.Month;
            dataLine.Ano = dataEstudo.Year;


            string diaAbrev = "";
            for (DateTime d = dataEstudo; d <= dataEstudo.AddDays(6); d = d.AddDays(1))
            {
                switch (d.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        diaAbrev = "SAB";
                        break;
                    case DayOfWeek.Sunday:
                        diaAbrev = "DOM";
                        break;
                    case DayOfWeek.Monday:
                        diaAbrev = "SEG";
                        break;
                    case DayOfWeek.Tuesday:
                        diaAbrev = "TER";
                        break;
                    case DayOfWeek.Wednesday:
                        diaAbrev = "QUA";
                        break;
                    case DayOfWeek.Thursday:
                        diaAbrev = "QUI";
                        break;
                    case DayOfWeek.Friday:
                        diaAbrev = "SEX";
                        break;
                    default:
                        diaAbrev = "";
                        break;

                }

                Boolean ehFeriado = false;
                int tipo = 0;
                var diaLine = config.BlocoDia.Where(x => x.Minemonico == diaAbrev).First();
                if (feriados.Any(x => x.Date == d.Date))
                {
                    ehFeriado = true;
                }
                if (ehFeriado || d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    tipo = tipoDias[d.Month].Item2;
                }
                else
                {
                    tipo = tipoDias[d.Month].Item1;
                }
                diaLine.TipoDia = tipo;
            }







            config.SaveToFile();
        }

        static void dessem2ccee(string commands)
        {
            //Z:\6_decomp\03_Casos\2019_04\deck_newave_2019_04
            //"Z:\\6_decomp\\03_Casos\\2019_05\\DEC_ONS_052019_RV1_VE"

            var command = commands.Split('|');

            //var data = command[0].Substring(command[0].Length - 7, 7).Split('_');
            var path = command[0];
            var cloneDir = "";

            try
            {
                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;

                var dirInfo = new DirectoryInfo(dir);
                var parentDir = dirInfo.Parent.FullName;
                var dirName = dirInfo.Name + "_ccee";

                var i = 0;
                do
                {
                    cloneDir = Path.Combine(parentDir, dirName + " (" + ++i + ")");
                } while (Directory.Exists(cloneDir));



                var deck = DeckFactory.CreateDeck(dir);

                if (!(deck is Compass.CommomLibrary.Dessem.Deck))
                {
                    throw new NotImplementedException("Deck não reconhecido para a execução");
                }

                deck.CopyFilesToFolder(cloneDir);

                dynamic cceeDeck = DeckFactory.CreateDeck(cloneDir);

                Boolean status = ConverteDessem(cloneDir);

                if (status)
                {
                    string texto = "Sucesso ao converter deck dessem!";
                    Program.AutoClosingMessageBox.Show(texto, "Caption", 5000);
                    if (command.Count() > 1 && command[1] == "true")
                    {
                        Compass.CommomLibrary.Tools.SendMail(texto, "carlos.paes@cpas.com.br; thamires.baptista@cpas.com.br; alex.marques@cpas.com.br; bruno.araujo@cpas.com.br; pedro.modesto@cpas.com.br; natalia.biondo@cpas.com.br;", "Sucesso ao converter deckDessem");
                    }
                }
                else
                {
                    string texto = "Falha ao converter deck dessem, diretório ou arquivos decomp inexistentes";
                    Program.AutoClosingMessageBox.Show(texto, "Caption", 5000);
                    if (Directory.Exists(cloneDir))
                    {
                        Directory.Delete(cloneDir, true);
                    }
                    if (command.Count() > 1 && command[1] == "true")
                    {

                        Compass.CommomLibrary.Tools.SendMail(texto, "carlos.paes@cpas.com.br; thamires.baptista@cpas.com.br; alex.marques@cpas.com.br; bruno.araujo@cpas.com.br; pedro.modesto@cpas.com.br; natalia.biondo@cpas.com.br;", "Falha ao converter deckDessem");
                    }
                }


            }
            catch (Exception ex)
            {
                if (command.Count() > 1 && command[1] == "true")
                {
                    var texto = ex.ToString();
                    if (ex.ToString().Contains("reconhecido"))
                    {
                        texto = "Deck não reconhecido para a execução por falta de arquivos!";
                    }
                    Compass.CommomLibrary.Tools.SendMail(texto, "carlos.paes@cpas.com.br; thamires.baptista@cpas.com.br; alex.marques@cpas.com.br; bruno.araujo@cpas.com.br; pedro.modesto@cpas.com.br; natalia.biondo@cpas.com.br;", "Falha ao converter deckDessem");
                    if (Directory.Exists(cloneDir))
                    {
                        Directory.Delete(cloneDir, true);
                    }
                }
                else
                {
                    var texto = "Deck não reconhecido para a execução por falta de arquivos! Erro: " + ex.ToString();
                    Program.AutoClosingMessageBox.Show(texto, "Caption", 10000);
                    if (Directory.Exists(cloneDir))
                    {
                        Directory.Delete(cloneDir, true);
                    }
                }


            }

        }

        public static Boolean ConverteDessem(string dir)
        {
            var deckestudo = DeckFactory.CreateDeck(dir) as Compass.CommomLibrary.Dessem.Deck;

            var dadvaz = deckestudo[CommomLibrary.Dessem.DeckDocument.dadvaz].Document.File;
            var dadlinhas = File.ReadAllLines(dadvaz).ToList();
            var dados = dadlinhas[9].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            DateTime dataEstudo = new DateTime(Convert.ToInt32(dados[3]), Convert.ToInt32(dados[2]), Convert.ToInt32(dados[1]));

            Boolean continua = CopiaArqDecomp(dataEstudo, dir);

            if (continua)
            {
                #region trata operut
                var operut = deckestudo[CommomLibrary.Dessem.DeckDocument.operut].Document as Compass.CommomLibrary.Operut.Operut;

                // var usinas = operut.BlocoInit.Where(x => x.Usina == 15).ToList();
                foreach (var usina in operut.BlocoInit.Where(x => x.Usina == 15).ToList())
                {
                    usina.Status = 0;
                    usina.Geracao = 0;
                }

                operut.SaveToFile(createBackup: true);
                #endregion

                #region trata ptoper
                var ptoper = deckestudo[CommomLibrary.Dessem.DeckDocument.ptoper].Document as Compass.CommomLibrary.PtoperDat.PtoperDat;

                //var usinas = ptoper.BlocoPtoper.Where(x => x.usina == 15).ToList();
                foreach (var usina in ptoper.BlocoPtoper.Where(x => x.usina == 15).ToList())
                {
                    usina.ValorFixado = 0;
                }

                ptoper.SaveToFile(createBackup: true);
                #endregion

                #region dessem.arq
                var dessemArq = deckestudo[CommomLibrary.Dessem.DeckDocument.dessem].Document.File;
                var lines = File.ReadAllLines(dessemArq).ToList();
                int indice = 0;
                if (lines.Any(x => x.StartsWith("INDELET")))
                {

                    indice = lines.IndexOf(lines.Where(x => x.StartsWith("INDELET")).First());
                    string frase = lines[indice];
                    frase = "&" + frase;
                    lines[indice] = frase;
                    File.WriteAllLines(dessemArq, lines);
                }

                #endregion

                #region renovaveis.dat

                var renovaveis = deckestudo[CommomLibrary.Dessem.DeckDocument.renovaveis].Document.File;
                var renoLines = File.ReadAllLines(renovaveis);
                List<string> modifReno = new List<string>();
                foreach (var item in renoLines)
                {
                    if (item.Split(';').First().Trim() == "EOLICA")
                    {
                        var textos = item.Split(';').ToList();

                        textos[textos.IndexOf(textos.Last()) - 1] = "0";
                        string linha = "";
                        foreach (var parte in textos)
                        {
                            linha += parte;
                            if (parte != textos.Last())
                            {
                                linha += ";";
                            }

                        }

                        modifReno.Add(linha);
                    }
                    else
                    {
                        modifReno.Add(item);
                    }
                }
                File.WriteAllLines(renovaveis, modifReno);

                #endregion

                #region trata entdados

                var entdados = deckestudo[CommomLibrary.Dessem.DeckDocument.entdados].Document as Compass.CommomLibrary.EntdadosDat.EntidadosDat;
                foreach (var line in entdados.BlocoRd.ToList())
                {
                    line[0] = "&" + line[0];
                }
                foreach (var line in entdados.BlocoTm.ToList())
                {
                    line.Rede = 0;
                }
                foreach (var line in entdados.BlocoPq.ToList())
                {
                    line[0] = "&" + line[0];
                }

                ComentaCICE(entdados);
                TrataIa(entdados, dataEstudo);

                TrataRhe(entdados, dataEstudo);
                entdados.SaveToFile(createBackup: true);
                RestsegRstlpp(dataEstudo, dir);
                #endregion

                return continua;
            }
            else
            {
                return continua;
            }



        }

        public static Boolean CopiaArqDecomp(DateTime dataEstudo, string dirTosave)
        {
            Boolean Ok = false;
            int contArq = 0;
            DateTime Ve;
            if (dataEstudo.DayOfWeek == DayOfWeek.Friday)
            {
                Ve = dataEstudo.AddDays(-1);
            }
            else
            {
                Ve = dataEstudo;
            }
            var rev = Tools.GetCurrRev(Ve);
            string mapcut = "mapcut.rv" + rev.rev.ToString();
            string cortdeco = "cortdeco.rv" + rev.rev.ToString();
            for (int i = 1; i <= 10; i++)
            {
                string camDecomp = @"Z:\6_decomp\03_Casos\" + rev.revDate.ToString("yyyy_MM") + "\\DEC_ONS_" + rev.revDate.ToString("MMyyyy") + "_RV" + rev.rev.ToString() + $"_VE_ccee ({i})";
                //string camDecomp = @"Z:\6_decomp\03_Casos\" + rev.revDate.ToString("yyyy_MM") + "\\teste_bruno\\DEC_ONS_" + rev.revDate.ToString("MMyyyy") + "_RV" + rev.rev.ToString() + $"_VE_ccee ({i})";

                if (Directory.Exists(camDecomp))
                {
                    var arqs = Directory.GetFiles(camDecomp).ToList();
                    foreach (var arq in arqs)
                    {
                        var filename = Path.GetFileName(arq);
                        if ((filename.ToLower() == mapcut) || (filename.ToLower() == cortdeco))
                        {
                            File.Copy(arq, Path.Combine(dirTosave, filename), true);
                            contArq++;
                        }
                    }
                    if (contArq == 2)
                    {
                        Ok = true;
                        return Ok;
                    }
                }
            }
            return Ok;

        }

        public static void RestsegRstlpp(DateTime dataEstudo, string dir)
        {
            //C:\Files\Middle - Preço\Resultados_Modelos\DESSEM\CCEE_DS\2020\12_dez\RV2\DS_CCEE_122020_SEMREDE_RV2D18
            var novoRestseg = new List<string>();
            var novoRstlpp = new List<string>();

            bool ok = false;
            var dtAtual = DateTime.Today.AddDays(1);
            var datalimite = DateTime.Today.AddDays(-180);
            string restsegRef = null;
            string rstlppRef = null;

            while (ok == false)
            {
                if (dtAtual < datalimite)
                {
                    throw new NotImplementedException("restseg e rstlpp de referência  não encontrados para a execução");

                }
                var revisao = Tools.GetCurrRev(dtAtual);
                var mesAbrev = Tools.GetMonthNumAbrev(revisao.revDate.Month);
                var camRef = $@"C:\Files\Middle - Preço\Resultados_Modelos\DESSEM\CCEE_DS\{revisao.revDate:yyyy}\{mesAbrev}\RV{revisao.rev}\DS_CCEE_{revisao.revDate:MMyyyy}_SEMREDE_RV{revisao.rev}D{dtAtual.Day:00}";

                if (Directory.Exists(camRef))
                {
                     restsegRef = Directory.GetFiles(camRef).Where(x => Path.GetFileName(x).ToLower().Contains("restseg")).FirstOrDefault();
                     rstlppRef = Directory.GetFiles(camRef).Where(x => Path.GetFileName(x).ToLower().Contains("rstlpp")).FirstOrDefault();

                    if (restsegRef != null && rstlppRef != null)
                    {
                        ok = true;
                    }
                }
                dtAtual = dtAtual.AddDays(-1);
            }

            var restseg = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).ToLower().Contains("restseg")).FirstOrDefault();
            var rstlpp = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).ToLower().Contains("rstlpp")).FirstOrDefault();

            if (restseg == null)
            {
                File.Copy(restsegRef, Path.Combine(dir, restsegRef.Split('\\').Last()),true);
            }
            if (rstlpp == null)
            {
                File.Copy(rstlppRef, Path.Combine(dir, rstlpp.Split('\\').Last()), true);
            }

            restseg = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).ToLower().Contains("restseg")).FirstOrDefault();
            rstlpp = Directory.GetFiles(dir).Where(x => Path.GetFileName(x).ToLower().Contains("rstlpp")).FirstOrDefault();

            #region rstlpp
            var rstlppLines = File.ReadAllLines(rstlpp, Encoding.GetEncoding("iso-8859-1")).ToList();
            var rstlppRefLines = File.ReadAllLines(rstlppRef, Encoding.GetEncoding("iso-8859-1")).ToList();

            foreach (var line in rstlppLines)
            {
                if (line.StartsWith("&"))
                {
                    novoRstlpp.Add(line);
                }
                else
                {
                    var minemonico = line.Split(' ').First();
                    var texto = "";
                    switch (minemonico)
                    {
                        case "RSTSEG":
                        case "ADICRS":
                            texto = line.Substring(0, 19);
                            break;
                        case "PARAM":
                            texto = line.Substring(0, 10);
                            break;
                        case "RESLPP":
                            texto = line.Substring(0, 15);
                            break;
                        case "VPARM":
                            texto = line.Substring(0, 19);
                            break;
                         
                    }
                    if (texto != "")
                    {
                        var linha = rstlppRefLines.Where(x => x.Contains(texto)).FirstOrDefault();
                        if (linha != null)
                        {
                            if (linha.StartsWith("&"))
                            {
                                novoRstlpp.Add("&" + line);
                            }
                            else
                            {
                                novoRstlpp.Add(line);
                            }
                        }
                        else
                        {
                            novoRstlpp.Add(line);
                        }
                    }
                    else
                    {
                        novoRstlpp.Add(line);
                    }
                }
            }
            File.WriteAllLines(rstlpp, novoRstlpp, Encoding.GetEncoding("iso-8859-1"));
            #endregion

            #region restseg

            var restsegLines = File.ReadAllLines(restseg, Encoding.GetEncoding("iso-8859-1")).ToList();
            var restsegRefLines = File.ReadAllLines(restsegRef, Encoding.GetEncoding("iso-8859-1")).ToList();

            foreach (var l in restsegLines)
            {
                if (l.StartsWith("&"))
                {
                    novoRestseg.Add(l);
                }
                else
                {
                    var linha = restsegRefLines.Where(x => x.Contains(l)).FirstOrDefault();

                    if (linha != null)
                    {
                        if (linha.StartsWith("&"))
                        {
                            novoRestseg.Add("&" + l);
                        }
                        else
                        {
                            novoRestseg.Add(l);
                        }
                    }
                    else
                    {
                        novoRestseg.Add(l);
                    }
                }
                
            }
            File.WriteAllLines(restseg, novoRestseg, Encoding.GetEncoding("iso-8859-1"));

            #endregion

        }
        public static void TrataRhe(Compass.CommomLibrary.EntdadosDat.EntidadosDat entdados, DateTime dataEstudo)
        {
            List<int> restComent = new List<int> { 141, 142, 143, 144, 145, 146, 147, 272, 800, 801, 802, 803, 804, 805, 827, 828, 840, 844, 846, 847, 919, 920, 921, 922, 923, 937, 985, 990 };
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
            for (int i = 602; i <= 649; i++)
            {
                foreach (var rhe in entdados.BlocoRhe.RheGrouped.Where(x => x.Key[1] == i))
                {
                    foreach (var rh in rhe.Value)
                    {
                        rh[0] = "&" + rh[0];
                    }
                }
            }

            for (int i = 901; i <= 990; i++)
            {
                foreach (var rhe in entdados.BlocoRhe.RheGrouped.Where(x => x.Key[1] == i))
                {
                    foreach (var rh in rhe.Value)
                    {
                        rh[2] = " I";
                    }
                }
            }

        }

        public static void TrataIa(Compass.CommomLibrary.EntdadosDat.EntidadosDat entdados, DateTime dataEstudo)
        {
            foreach (var line in entdados.BlocoIa.ToList())
            {
                Boolean apagaCom = false;
                int i;
                int indice = entdados.BlocoIa.IndexOf(line);
                if (line.Comment != null)
                {
                    var comentarios = line.Comment.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var texto = line.Comment;
                    foreach (var item in comentarios.Where(x => x.StartsWith("&IA")).ToList())
                    {
                        apagaCom = true;
                        string linha = string.Empty;
                        if (item != comentarios.Last())
                        {
                            linha = item + "\r\n";
                        }
                        else
                        {
                            linha = item;
                        }
                        texto = texto.Replace(linha, "");
                        var newLine = entdados.BlocoIa.CreateLine(item.Substring(1));

                        if (int.TryParse(newLine.DiaInic, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out i))
                        {
                            if (i <= dataEstudo.Day)
                            {
                                entdados.BlocoIa.Insert(indice, newLine);
                                indice += 1;
                            }
                            else
                            {
                                newLine[0] = "&" + newLine[0];
                                entdados.BlocoIa.Insert(indice, newLine);
                                indice += 1;
                            }
                        }
                        else
                        {
                            if (newLine.DiaInic == "I")
                            {
                                entdados.BlocoIa.Insert(indice, newLine);
                                indice += 1;
                            }
                            else if (newLine.DiaInic == "F")
                            {
                                newLine[0] = "&" + newLine[0];
                                entdados.BlocoIa.Insert(indice, newLine);
                                indice += 1;
                            }

                        }
                        if (item == comentarios.Last() && texto != "")
                        {
                            entdados.BlocoIa.First().Comment = texto;
                        }
                    }

                }

                if (int.TryParse(line.DiaInic, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out i))
                {
                    if (i <= dataEstudo.Day)
                    {
                        line[0] = "&" + line[0];
                    }
                }
                else if (line.DiaInic == "I")
                {
                    line[0] = "&" + line[0];
                }

                if (apagaCom)
                {
                    line.Comment = null;
                }
            }
        }

        public static void ComentaCICE(Compass.CommomLibrary.EntdadosDat.EntidadosDat entdados)
        {
            Dictionary<int, int> barras = new Dictionary<int, int>() {
                    {7059,1 },
                    {7054,1 },
                    {7055,1 },
                    {7057,1 },
                    {3962,1 },
                    {3963,1 },
                    {9637,1 },
                    {181,1 },
                    {185,1 },
                    {190,1 },
                    {3112,1 },
                    {8100,4 },
                    {3010,1 },
                    {85,1 },
                    {86,1 },
                    {9605,1 },

                };

            List<int> restComent = new List<int> { 101, 102, 122, 111, 112, 121, 131, 132, 141, 142, 151, 152, 302, 301, 311, 312, 501, 511, 502, 512 };
            foreach (var item in restComent.ToList())
            {
                foreach (var line in entdados.BlocoCice.Where(x => x.IdContrato == item).ToList())
                {
                    line[0] = "&" + line[0];
                }
                foreach (var line in entdados.BlocoCice.ToList())
                {
                    var chave = line.Sist_Barra;
                    if (barras.ContainsKey(chave))
                    {
                        line.Sist_Barra = barras[chave];
                    }
                }
            }


        }
        static void cortes_tendencia(string path)
        {
            string dir;
            if (Directory.Exists(path))
            {
                dir = path;
            }
            else
                return;

            var decks = Directory.GetDirectories(dir)
                .Where(x => Directory.GetFiles(x, "dadger.*", SearchOption.TopDirectoryOnly).Length > 0);

            corte_tendencia(decks.ToArray());
        }

        static void corte_tendencia(string path)
        {

            string dir;
            if (Directory.Exists(path))
            {
                dir = path;
            }
            else if (File.Exists(path))
            {
                dir = Path.GetDirectoryName(path);
            }
            else
                return;

            corte_tendencia(new string[] { dir });
        }

        static void corte_tendencia(params string[] decks)
        {
            try
            {

                if (decks.Count() > 0)
                {
                    Thread thread = new Thread(cortesTHSTA);
                    thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                    thread.Start(decks);
                    thread.Join(); //Wait for the thread to end      

                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {

            }
        }

        //static void corte(string path) {

        //    try {
        //        string dir;
        //        if (Directory.Exists(path)) {
        //            dir = path;
        //        } else if (File.Exists(path)) {
        //            dir = Path.GetDirectoryName(path);
        //        } else
        //            return;

        //        var deck = DeckFactory.CreateDeck(dir);

        //        if (deck is CommomLibrary.Decomp.Deck) {
        //            Thread thread = new Thread(cortesSTA);
        //            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
        //            thread.Start(((CommomLibrary.Decomp.Deck)deck)[CommomLibrary.Decomp.DeckDocument.dadger].BasePath);
        //            thread.Join(); //Wait for the thread to end                    
        //        }

        //    } catch (Exception ex) {
        //        System.Windows.Forms.MessageBox.Show(ex.Message);
        //    } finally {

        //    }
        //}

        //static void cortes(string path) {

        //    try {

        //        string dir;
        //        if (Directory.Exists(path)) {
        //            dir = path;
        //        } else
        //            return;

        //        var decks = Directory.GetDirectories(dir)
        //            .Where(x => Directory.GetFiles(x, "dadger.*", SearchOption.TopDirectoryOnly).Length > 0);

        //        if (decks.Count() > 0) {
        //            corte(decks.First());

        //            if (corteOK) {
        //                var deckBase = DeckFactory.CreateDeck(decks.First());

        //                var dadgerBase = (Compass.CommomLibrary.Dadger.Dadger)((CommomLibrary.Decomp.Deck)deckBase)[CommomLibrary.Decomp.DeckDocument.dadger].Document;
        //                var fcBase = (Compass.CommomLibrary.Dadger.FcBlock)dadgerBase.Blocos["FC"];

        //                foreach (var deck in decks.Skip(1)) {

        //                    var deckCopy = DeckFactory.CreateDeck(deck);

        //                    var dadgerCopy = (Compass.CommomLibrary.Dadger.Dadger)((CommomLibrary.Decomp.Deck)deckCopy)[CommomLibrary.Decomp.DeckDocument.dadger].Document;
        //                    dadgerCopy.Blocos["FC"] = fcBase;

        //                    dadgerCopy.SaveToFile();
        //                }
        //            }
        //        }


        //    } catch (Exception ex) {
        //        System.Windows.Forms.MessageBox.Show(ex.Message);
        //    } finally {

        //    }
        //}

        //static bool corteOK = false;
        //static void cortesSTA(object path) {
        //    var dadger = (Compass.CommomLibrary.Dadger.Dadger)DocumentFactory.Create((string)path);

        //    var frm = new FrmCortes(dadger);

        //    corteOK = frm.ShowDialog() == DialogResult.OK;
        //}



        static void PreliminarAutorun(string path, string comando)
        {
            try
            {
                var nameCommand = "DcNwPreli" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var comm = new { CommandName = nameCommand, EnviarEmail = true, WorkingDirectory = path, Command = comando, User = "AutoRun", IgnoreQueue = true };

                var cont = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(comm));
                cont.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();

                var responseTsk = httpClient.PostAsync("http://azcpspldv02.eastus.cloudapp.azure.com:5015/api/Command", cont);
                responseTsk.Wait();
                var response = responseTsk.Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception();
                }

            }
            catch (Exception erro)
            {
                Program.AutoClosingMessageBox.Show("Deu erro: " + erro.Message, "Caption", 2000);
            }
        }

        static void pldDessemTHSTA(object path)
        {
            var frm = new FrmPldDessem((string)path);
            frm.ShowDialog();
        }

        static void cortesTHSTA(object paths)
        {
            var frm = new FrmCortes((string[])paths);
            frm.ShowDialog();
        }

#if DEBUG
        public string apiUrl = @"http://azcpspldv02.eastus.cloudapp.azure.com:5014/api/";
#else
        public string apiUrl = @"http://azcpspldv02.eastus.cloudapp.azure.com:5014/api/";
#endif

        static void setConfigFile()
        {

            string path = "Compass.DecompTools.dll.config";

            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
            typeof(System.Configuration.ConfigurationManager).GetField("s_initState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, 0);

        }

        static void tratarInviabilidades(string path)
        {

            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path.Remove(path.Length - 1) : path;
                }
                else
                    return;


                var dirs = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories)
                        .AsParallel()//.WithDegreeOfParallelism(4)                       
                        .Select(x => new
                        {
                            dir = x.Remove(0, dir.Length),
                            deck = DeckFactory.CreateDeck(x),
                        });

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        static void tratarInviabilidade(string path)
        {
            try
            {

                string dir;
                if (Directory.Exists(path))
                {
                    dir = path;
                }
                else if (File.Exists(path))
                {
                    dir = Path.GetDirectoryName(path);
                }
                else
                    return;

                var deck = DeckFactory.CreateDeck(dir) as Compass.CommomLibrary.Decomp.Deck;

                if (deck != null)
                {

                    var fi = System.IO.Directory.GetFiles(dir, "inviab_unic.*", SearchOption.TopDirectoryOnly).FirstOrDefault();

                    if (fi != null)
                    {
                        var inviab = (Compass.CommomLibrary.Inviab.Inviab)DocumentFactory.Create(fi);
                        Services.Deck.DesfazerInviabilidades(deck, inviab);

                        string newPath;
                        duplicar(dir, out newPath);

                        var originalFile = deck[CommomLibrary.Decomp.DeckDocument.dadger].Document.File;
                        var newFile = originalFile.Replace(dir, newPath);

                        deck[CommomLibrary.Decomp.DeckDocument.dadger].Document.SaveToFile(newFile, true);

                    }
                    else
                        throw new Exception("Arquivo inviab_unic.xxx não encontrado.");
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void tendhidr(string path)
        {
            try
            {

                Thread thread = new Thread(tendhidrSTA);
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start(path);
                thread.Join(); //Wait for the thread to end                    

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {

            }
        }

        static void tendhidrSTA(object path)
        {
            string pa = (string)path;
            var frm = new FrmTendenciaHidr();

            if (System.IO.File.Exists(pa))
            {

                if (pa.ToLowerInvariant().EndsWith("vazoes.dat"))
                {
                    frm.VazoesDat = pa;
                }
                else if (pa.ToLowerInvariant().EndsWith("vazpast.dat"))
                {
                    frm.VazpastDat = pa;
                }
            }

            frm.ShowDialog();
        }

        static void nwOnsReCcee(object ONSDeck)
        {
            var deck = ONSDeck;
            var frm = new FrmOnsReCcee();
            frm.deckONS = deck as CommomLibrary.Newave.Deck;

            frm.ShowDialog();
        }

        static void dcOns2CceeSTA(object dcDeck)
        {
            var deck = dcDeck as Compass.CommomLibrary.Decomp.Deck;
            var frm = new FrmDcOns2Ccee(deck);
            //frm.Deck = deck;

            frm.ShowDialog();
        }
    }
}
