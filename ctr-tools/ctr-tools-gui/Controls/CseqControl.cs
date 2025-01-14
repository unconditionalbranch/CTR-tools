﻿using CTRFramework.Shared;
using CTRFramework.Sound;
using CTRFramework.Sound.CSeq;
using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CTRTools.Controls
{
    public partial class CseqControl : UserControl
    {
        public CSEQ seq;

        //??
        string loadedfile = "";


        public CseqControl()
        {
            InitializeComponent();

            LoadMeta();
        }

        public void LoadMeta()
        {
            if (!Meta.LoadMidiJson())
            {
                MessageBox.Show("Couldn't load Json!");
            }
            else
            {
                metaInstList.Items.Clear();
                comboBox1.Items.Clear();

                metaInstList.Items.AddRange(Meta.GetPatchList().ToArray());
                comboBox1.Items.AddRange(Meta.GetPatchList().ToArray());
            }

            Howl.samplenames = Meta.LoadNumberedList("samplenames.txt");
        }

        private void LoadCseq(string filename)
        {
            //Log.Clear();
            sequenceBox.Items.Clear();
            trackBox.Items.Clear();

            string name = Path.GetFileNameWithoutExtension(filename);

            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                if (name.Contains(comboBox1.Items[i].ToString()))
                    comboBox1.SelectedIndex = i;
            }

            try
            {
                seq = CSEQ.FromFile(filename);
                FillUI(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read CTR sequence!\r\n" + ex.ToString());
            }
        }

        private void FillUI(string fn)
        {
            instrumentList.Nodes.Clear();
            sequenceBox.Items.Clear();
            trackBox.Items.Clear();
            textBox2.Text = "";

            loadedfile = Path.GetFileName(fn);
            this.Text = "CTR CSEQ - " + loadedfile;

            textBox2.Text = "hi";//seq.header.ToString();

            int i = 0;

            foreach (Song s in seq.Songs)
            {
                sequenceBox.Items.Add("Sequence_" + i.ToString("X2"));
                i++;
            }

            TreeNode tn = new TreeNode("SampleDef");

            foreach (var sd in seq.samples)
            {
                TreeNode tn1 = new TreeNode(sd.Tag + (Howl.samplenames.ContainsKey(sd.SampleID) ? "_" + Howl.samplenames[sd.SampleID] : ""));
                tn.Nodes.Add(tn1);
            }

            TreeNode tn2 = new TreeNode("SampleDefReverb");

            foreach (var sd in seq.samplesReverb)
            {
                TreeNode tn3 = new TreeNode(sd.Tag + (Howl.samplenames.ContainsKey(sd.SampleID) ? "_" + Howl.samplenames[sd.SampleID] : ""));
                tn2.Nodes.Add(tn3);
            }

            instrumentList.Nodes.Add(tn2);
            instrumentList.Nodes.Add(tn);

            instrumentList.ExpandAll();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
                LoadCseq(ofd.FileName);
        }

        private void sequenceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            trackBox.Items.Clear();

            int i = 0;

            Song song = seq.Songs[sequenceBox.SelectedIndex];

            foreach (var track in song.tracks)
            {
                trackBox.Items.Add(track.Name);
                i++;
            }

            tabControl1.SelectedIndex = 1;
        }

        private void trackBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int x = sequenceBox.SelectedIndex;
            int y = trackBox.SelectedIndex;

            if (x != -1 && y != -1)
                this.trackInfoBox.Text = seq.Songs[x].tracks[y].ToString();

            tabControl1.SelectedIndex = 0;
        }

        private void sequenceBox_DoubleClick(object sender, EventArgs e)
        {
            sfd.FileName = Path.ChangeExtension(loadedfile, ".mid");

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                seq.Songs[sequenceBox.SelectedIndex].ExportMIDI(sfd.FileName, seq);
                //seq.ToSFZ(Path.ChangeExtension(sfd.FileName, ".sfz"));
            }

        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                switch (Path.GetExtension(files[0]))
                {
                    case ".cseq": LoadCseq(files[0]); break;
                    case ".bnk": LoadBank(files[0]); tabControl1.SelectedIndex = 3; break;
                    default: MessageBox.Show("Unsupported file."); break;
                }
            }
        }

        private void exportSEQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (seq != null)
                seq.Save(Path.Combine(Meta.BasePath, "test.cseq"));
        }

        Bank bnk = new Bank();

        public void LoadBank(string filename)
        {
            bnk = Bank.FromFile(filename);
            bnk.ExportAll(1, Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));

            listBox2.Items.Clear();
            foreach (var samp in bnk.samples)
            {
                listBox2.Items.Add(samp.Key);
            }

            if (seq != null)
            {
                seq.Bank = bnk;
                MessageBox.Show(seq.CheckBankForSamples() ? "samples OK!" : "samples missing");
                trackInfoBox.Text = seq.ListMissingSamples();
            }
        }


        private void loadBankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CTR Sound Bank (*.bnk)|*.bnk";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadBank(ofd.FileName);
            }
        }



        #region [Form stuff]

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void tutorialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trackInfoBox.Text = "halp!";//Properties.Resources.help;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            VagSample.DefaultSampleRate = 11025;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            VagSample.DefaultSampleRate = 22050;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            VagSample.DefaultSampleRate = 33075;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            VagSample.DefaultSampleRate = 44100;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        private void testJsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string[] files = Directory.GetFiles(fbd.SelectedPath);


                List<string> x = new List<string>();

                foreach (string s in files)
                {
                    CSEQ c = CSEQ.FromFile(s);

                    foreach (var sd in c.samples)
                        if (!x.Contains(sd.Tag))
                            x.Add(sd.Tag);

                    foreach (var sd in c.samplesReverb)
                        if (!x.Contains(sd.Tag))
                            x.Add(sd.Tag);
                }

                x.Sort();
                StringBuilder sb = new StringBuilder();

                foreach (string s in x) sb.AppendLine(s);

                trackInfoBox.Text = sb.ToString();
            }

            /*
            string track = "canyon";
            string instr = "long";
            int id = 0;


            MetaInst info = CTRJson.GetMetaInst(track, instr, id);
            textBox1.Text += "" + info.Midi + " " + info.Key + " " + info.Title + " " + info.Pitch;
            */
        }

        private void patchMIDIInstrumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CSEQ.PatchMidi = patchMIDIInstrumentsToolStripMenuItem.Checked;
        }

        private void exportSamplesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            seq?.ExportSamples();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CSEQ.PatchName = comboBox1.Items[comboBox1.SelectedIndex].ToString();
        }

        private void ignoreOriginalVolumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CSEQ.IgnoreVolume = ignoreOriginalVolumeToolStripMenuItem.Checked;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            AudioFileReader wave = null;
            WaveOut outputSound = null;
            string x = "";

            if (instrumentList.SelectedNode.Parent != null)
            {
                if (instrumentList.SelectedNode.Parent.Index == 1)
                {
                    var sd = seq.samples[instrumentList.SelectedNode.Index];
                    instrumentInfo.SelectedObject = sd;
                    x = seq.path + "\\" + seq.name + "\\" + sd.Tag + (Howl.samplenames.ContainsKey(sd.SampleID) ? "_" + Howl.samplenames[sd.SampleID] : "") + ".wav";
                }

                if (instrumentList.SelectedNode.Parent.Index == 0)
                {
                    var sd = seq.samplesReverb[instrumentList.SelectedNode.Index];
                    instrumentInfo.SelectedObject = sd;
                    x = seq.path + "\\" + seq.name + "\\" + sd.Tag + (Howl.samplenames.ContainsKey(sd.SampleID) ? "_" + Howl.samplenames[sd.SampleID] : "") + ".wav";
                }


                if (File.Exists(x))
                {
                    try
                    {
                        wave = new NAudio.Wave.AudioFileReader(x);

                        outputSound = new WaveOut();
                        outputSound.Init(wave);
                        outputSound.Play();
                    }
                    catch (Exception ex)
                    {
                        trackInfoBox.Text = ex.Message;
                    }
                }

            }
            else
            {
                instrumentInfo.SelectedObject = null;
            }
        }

        private void trackBox_DoubleClick(object sender, EventArgs e)
        {
            int x = sequenceBox.SelectedIndex;
            int y = trackBox.SelectedIndex;

            if (x != -1 && y != -1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "MIDI File (*.mid)|*.mid";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    seq.Songs[x].tracks[y].Import(ofd.FileName);
                }
            }

            tabControl1.SelectedIndex = 0;
        }

        private void copyInstrumentVolumeToTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CSEQ.UseSampleVolumeForTracks = copyInstrumentVolumeToTracksToolStripMenuItem.Checked;
        }

        private void alistAlBankSamplesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string[] files = Directory.GetFiles(fbd.SelectedPath);


                List<string> x = new List<string>();

                foreach (string s in files)
                {
                    Bank b = Bank.FromFile(s);

                    foreach (var v in b.samples)
                    {
                        x.Add(Path.GetFileNameWithoutExtension(s) + "," + v.Key.ToString("000"));
                    }
                }

                x.Sort();
                StringBuilder sb = new StringBuilder();

                foreach (string s in x) sb.AppendLine(s);

                trackInfoBox.Text = sb.ToString();
            }

        }

        private void reloadMetaFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            metaInstInfo.Text = Meta.GetMetaInstText(metaInstList.SelectedItem.ToString());
        }

        private void reloadMIDIMappingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadMeta();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "VAG File (*.vag)|*.vag";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(ofd.FileName)))
                {

                }
            }
        }

        private void importMIDIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (seq == null)
                seq = new CSEQ();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "MIDI File (*.mid)|*.mid";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                MidiFile midi = new MidiFile(ofd.FileName);

                Song newMidi = new Song();

                for (int i = 0; i < midi.Tracks; i++)
                {
                    CTrack track = new CTrack();
                    track.Index = i;
                    //track.name = $"track_{i.ToString("00")}";
                    track.FromMidiEventList(midi.Events.GetTrackEvents(i).ToList());
                    newMidi.tracks.Add(track);
                }

                foreach (var x in newMidi.tracks)
                {
                    Command c = new Command();
                    c.cseqEvent = CSEQEvent.ChangePatch;
                    c.pitch = (byte)x.Index;
                    c.wait = 0;
                    x.cseqEventCollection.Insert(0, c);
                }

                seq.Songs.Add(newMidi);

                FillUI(loadedfile);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (seq == null)
                return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Crash Team Racing CSEQ (*.cseq)|*.cseq";

            if (sfd.ShowDialog() == DialogResult.OK)
                seq.Save(sfd.FileName);
        }
    }
}
