﻿using Compass.CommomLibrary;
using Compass.CommomLibrary.Dadger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace Compass.DecompToolsShellX
{
    public partial class FrmCortes : Form
    {
        Dadger dadger;
        string selectedPath;
        List<CommomLibrary.Decomp.Deck> decomps = null;

        public FrmCortes(string[] paths)
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            decomps = paths.Select(x => DeckFactory.CreateDeck(x))
                .Where(x => x is CommomLibrary.Decomp.Deck)
                .Cast<CommomLibrary.Decomp.Deck>().ToList();

            if (decomps.FirstOrDefault() != null)
            {
                var deck = decomps.FirstOrDefault();
                this.dadger = deck[CommomLibrary.Decomp.DeckDocument.dadger].Document as Compass.CommomLibrary.Dadger.Dadger;
            }
        }

        public FrmCortes(Dadger dadger)
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            this.dadger = dadger;
        }

        public void CortesLoad()
        {
            if ((FcBlock)dadger.Blocos["FC"] != null)
            {
                FcBlock fc = (FcBlock)dadger.Blocos["FC"];

                txtInfoMne.Text = fc.CortesInfo.Mnemonico;
                txtInfo.Text = fc.CortesInfo.Arquivo;

                txtMne.Text = fc.Cortes.Mnemonico;
                txtCortes.Text = fc.Cortes.Arquivo;
            }
            else
                MessageBox.Show("dadger não encontrado");
        }

        private void FrmCortes_Load(object sender, EventArgs e)
        {
            CortesLoad();
        }

        public void OK(bool automatico = false)
        {
            if (automatico)
            {
                CortesLoad();

                var datadeck = dadger.VAZOES_DataDoEstudo;

                //@"Z:\6_decomp\03_Casos\2019_05\NW201905 - Resultados"
                //@"Z:\6_decomp\03_Casos\2019_05\deck_newave_2019_05"
                var path = "";
                // if (Directory.Exists($@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\NW{datadeck:yyyyMM}-Resultados"))
                if (Directory.Exists($@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\NW{datadeck:yyyyMM}-Resultados"))
                {
                    //path = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\NW{datadeck:yyyyMM}-Resultados";
                    path = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\NW{datadeck:yyyyMM}-Resultados";
                    //else if (Directory.Exists($@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\deck_newave_{datadeck:yyyy_MM}_ccee"))
                }
                else if (Directory.Exists($@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\NW{datadeck:yyyyMM}-Resultado"))
                {
                    //path = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\NW{datadeck:yyyyMM}-Resultados";
                    path = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\NW{datadeck:yyyyMM}-Resultado";
                    //else if (Directory.Exists($@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\deck_newave_{datadeck:yyyy_MM}_ccee"))
                }
                else if (Directory.Exists($@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\deck_newave_{datadeck:yyyy_MM}_ccee"))
                {
                    //path = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\deck_newave_{datadeck:yyyy_MM}_ccee";
                    path = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\deck_newave_{datadeck:yyyy_MM}_ccee";
                }
                else
                {
                    var i = 1;
                    var nwDir = "";
                    do
                    {
                        path = nwDir;
                        // nwDir = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\deck_newave_{datadeck:yyyy_MM}_ccee ({i++})";
                        nwDir = $@"Z:\6_decomp\03_Casos\{datadeck:yyyy_MM}\deck_newave_{datadeck:yyyy_MM}_ccee ({i++})";
                    } while (Directory.Exists(nwDir));
                }

                if (path == "")
                {
                    throw new Exception("Pasta com cortes não encontrada");
                }

                AlteraCortes(Path.Combine(path, "cortes.dat"));
            }
            FcBlock fc = (FcBlock)dadger.Blocos["FC"];

            fc.CortesInfo.Mnemonico = txtInfoMne.Text;
            fc.CortesInfo.Arquivo = txtInfo.Text;

            fc.Cortes.Mnemonico = txtMne.Text;
            fc.Cortes.Arquivo = txtCortes.Text;

            if (!automatico)
            {
                if (MessageBox.Show("Gravar Alterações?", "Alteração de cortes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {

                    this.Cursor = Cursors.WaitCursor;

                    dadger.SaveToFile();


                    if (decomps != null && decomps.Count() > 1)
                    {

                        foreach (var deck in decomps.Skip(1))
                        {

                            var dadgerCopy = deck[CommomLibrary.Decomp.DeckDocument.dadger].Document;
                            dadgerCopy.Blocos["FC"] = fc;
                            dadgerCopy.SaveToFile();
                        }
                    }

                    var caminhoCortes = dadger.CortesPath;

                    Compass.CommomLibrary.VazoesC.VazoesC vazoes = null;
                    if (decomps.First()[CommomLibrary.Decomp.DeckDocument.vazoesc] != null)
                    {
                        vazoes = (Compass.CommomLibrary.VazoesC.VazoesC)decomps.First()[CommomLibrary.Decomp.DeckDocument.vazoesc].Document;
                    }
                    else
                    {
                        vazoes =
                        Compass.CommomLibrary.DocumentFactory.Create(
                            System.IO.Path.Combine(
                             System.IO.Path.GetDirectoryName(caminhoCortes), "vazoes.dat"
                            )
                        ) as Compass.CommomLibrary.VazoesC.VazoesC;
                    }

                    Compass.CommomLibrary.Vazpast.Vazpast vazpast =
                        Compass.CommomLibrary.DocumentFactory.Create(
                            System.IO.Path.Combine(
                             System.IO.Path.GetDirectoryName(caminhoCortes), "vazpast.dat"
                            )
                        ) as Compass.CommomLibrary.Vazpast.Vazpast;


                    Compass.Services.Vazoes6.IncorporarVazpast(vazoes, vazpast, new DateTime(dadger.VAZOES_AnoInicialDoEstudo, dadger.VAZOES_MesInicialDoEstudo, 1));

                    foreach (var deck in decomps)
                    {
                        vazoes.SaveToFile(
                            System.IO.Path.Combine(deck.BaseFolder, "vazoes.dat"), true
                            );
                    }

                    this.Cursor = Cursors.Default;

                    MessageBox.Show("Cortes e Tendencias Hidrológicas alteradas");

                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
            }

            else
            {
                this.Cursor = Cursors.WaitCursor;

                dadger.SaveToFile();


                if (decomps != null && decomps.Count() > 1)
                {

                    foreach (var deck in decomps.Skip(1))
                    {

                        var dadgerCopy = deck[CommomLibrary.Decomp.DeckDocument.dadger].Document;
                        dadgerCopy.Blocos["FC"] = fc;
                        dadgerCopy.SaveToFile();
                    }
                }

                var caminhoCortes = dadger.CortesPath;

                Compass.CommomLibrary.VazoesC.VazoesC vazoes = null;
                if (decomps.First()[CommomLibrary.Decomp.DeckDocument.vazoesc] != null)
                {
                    vazoes = (Compass.CommomLibrary.VazoesC.VazoesC)decomps.First()[CommomLibrary.Decomp.DeckDocument.vazoesc].Document;
                }
                else
                {
                    vazoes =
                    Compass.CommomLibrary.DocumentFactory.Create(
                        System.IO.Path.Combine(
                         System.IO.Path.GetDirectoryName(caminhoCortes), "vazoes.dat"
                        )
                    ) as Compass.CommomLibrary.VazoesC.VazoesC;
                }

                Compass.CommomLibrary.Vazpast.Vazpast vazpast =
                    Compass.CommomLibrary.DocumentFactory.Create(
                        System.IO.Path.Combine(
                         System.IO.Path.GetDirectoryName(caminhoCortes), "vazpast.dat"
                        )
                    ) as Compass.CommomLibrary.Vazpast.Vazpast;


                Compass.Services.Vazoes6.IncorporarVazpast(vazoes, vazpast, new DateTime(dadger.VAZOES_AnoInicialDoEstudo, dadger.VAZOES_MesInicialDoEstudo, 1));

                foreach (var deck in decomps)
                {
                    vazoes.SaveToFile(
                        System.IO.Path.Combine(deck.BaseFolder, "vazoes.dat"), true
                        );
                }

                this.Cursor = Cursors.Default;

                Program.AutoClosingMessageBox.Show("Cortes e Tendencias Hidrológicas alteradas", "Caption", 2000);

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            OK();
        }

        public void AlteraCortes(string cortesname)
        {

            var cortes = cortesname.Split('\\').ToList();
            var refPath = dadger.File.Split('\\').ToList();


            for (int i = 0; i < Math.Min(cortes.Count, refPath.Count); i++)
            {
                if (cortes[i] == refPath[i])
                {
                    cortes.RemoveAt(i);
                    refPath.RemoveAt(i);
                    i--;
                }
            }

            var cortesRelPath = "";
            for (int i = 0; i < refPath.Count - 1; i++)
            {
                cortesRelPath += "../";
            }
            for (int i = 0; i < cortes.Count - 1; i++)
            {
                cortesRelPath += cortes[i] + "/";
            }

            var x1 = cortesRelPath + cortes.Last();
            var x2 = cortesRelPath + "cortesh" + System.IO.Path.GetExtension(x1);

            if (x1.Length > 60 || x2.Length > 60)
            {
                MessageBox.Show("Caminho muito longo, não é possível alterar");
            }
            else
            {
                txtCortes.Text = x1;
                txtInfo.Text = x2;

                selectedPath = System.IO.Path.GetDirectoryName(cortesname);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "cortes.*|cortes.*";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AlteraCortes(ofd.FileName);
            }
        }
    }
}
