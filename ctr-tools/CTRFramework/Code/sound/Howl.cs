﻿using CTRFramework.Shared;
using CTRFramework.Sound.CSeq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace CTRFramework.Sound
{
    public enum HowlVersion
    {
        UsDemo = 0x6F,
        PalDemoOPSM = 0x71,
        PalDemoSpyro = 0x72,
        Proto = 0x7D,
        Ntsc = 0x80
    }

    public class Howl
    {
        public static Dictionary<int, string> seqnames = new Dictionary<int, string>();
        public static Dictionary<int, string> banknames = new Dictionary<int, string>();
        public static Dictionary<int, string> samplenames = new Dictionary<int, string>();

        string name;

        string reg;

        public HowlVersion version;     //freezes the game if changed, game code tests against fixed number for some reason. maybe like version.

        public static List<InstrumentShort> samplesSfx = new List<InstrumentShort>();
        List<InstrumentShort> samplesEngineSfx = new List<InstrumentShort>();

        public List<Bank> Banks = new List<Bank>();
        public List<CSEQ> Songs = new List<CSEQ>();

        public static string GetName(int x, Dictionary<int, string> dict)
        {
            string result = $"{x.ToString("0000")}_{ x.ToString("X4")}";

            if (dict.ContainsKey(x))
                result += "_" + dict[x];

            return result;
        }

        List<ushort> unk = new List<ushort>();



        List<int> ptrBanks = new List<int>();
        List<int> ptrSeqs = new List<int>();


        #region [Constructors, Factories]
        public Howl()
        {
        }

        public Howl(BinaryReaderEx br)
        {
            Read(br);
        }

        public static Howl FromReader(BinaryReaderEx br)
        {
            return new Howl(br);
        }

        public static Howl FromFile(string filename)
        {
            using (var br = new BinaryReaderEx(File.OpenRead(filename)))
            {
                return FromReader(br);
            }
        }
        #endregion

        private void KnownFileCheck(BinaryReaderEx br)
        {
            br.Jump(0);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Helpers.GetTextFromResource(Meta.XmlPath));

            var hash = MD5.Create().ComputeHash(br.BaseStream);
            string md5 = BitConverter.ToString(hash).Replace("-", "");

            br.Jump(0);

            foreach (XmlElement el in doc.SelectNodes("/data/howl/entry"))
            {
                if (md5.ToLower() == el["md5"].InnerText.ToLower())
                {
                    Console.WriteLine($"{md5}\r\n{el["name"].InnerText} [{el["region"].InnerText}] detected.");
                    banknames = Meta.LoadNumberedList(el["banks"].InnerText);
                    samplenames = Meta.LoadNumberedList(el["samples"].InnerText);

                    string[] lines = Helpers.GetLinesFromResource("howlnames.txt");

                    foreach (var line in lines)
                    {
                        if (line.Split(':')[0].Trim() == el["sequences"].InnerText)
                        {
                            string[] songs = line.Split(':')[1].Split(';');

                            for (int i = 0; i < songs.Length; i++)
                            {
                                seqnames.Add(i, songs[i].Trim());
                            }

                            break;
                        }
                    }

                    return;
                }
            }

            Console.WriteLine("Unknown HOWL file.");
        }

        public void Write(BinaryWriterEx bw)
        {
            Console.WriteLine("Writing HOWL...");

            bw.Write("HOWL".ToCharArray());
            bw.Write((int)version);
            bw.Seek(8);

            bw.Write(unk.Count);
            bw.Write(samplesSfx.Count);
            bw.Write(samplesEngineSfx.Count);

            bw.Write(Banks.Count);
            bw.Write(Songs.Count);

            bw.Write(unk.Count * 4 + (samplesSfx.Count + samplesEngineSfx.Count) * 8 + (Banks.Count + Songs.Count) * 2); //sampleDataSize

            foreach (var value in unk)
            {
                bw.Write((short)0);
                bw.Write(value);
            }

            foreach (var instrument in samplesSfx)
                instrument.Write(bw);

            foreach (var instrument in samplesEngineSfx)
                instrument.Write(bw);



            int ptrs = (int)bw.BaseStream.Position;

            bw.Seek(2 * (Banks.Count + Songs.Count));

            bw.JumpNextSector();

            List<uint> offsets = new List<uint>();

            foreach (var bank in Banks)
            {
                offsets.Add((uint)bw.BaseStream.Position);
                bank.Write(bw);
                bw.JumpNextSector();
            }

            foreach (var song in Songs)
            {
                offsets.Add((uint)bw.BaseStream.Position);
                song.Write(bw);
                bw.JumpNextSector();
            }

            bw.Jump(ptrs);

            foreach (var ptr in offsets)
                bw.Write((short)(ptr / Meta.SectorSize));

            Console.WriteLine("HOWL saved.");
        }

        public void Read(BinaryReaderEx br)
        {
            KnownFileCheck(br);

            char[] magic = br.ReadChars(4);

            if (new string(magic) != "HOWL")
                throw new Exception("Not a CTR HOWL file.");

            version = (HowlVersion)br.ReadInt32();
            int reserved1 = br.ReadInt32();
            int reserved2 = br.ReadInt32();

            if (reserved1 != 0)
                Helpers.Panic(this, PanicType.Assume, "reserved1 is not null. Possible error.");

            if (reserved2 != 0)
                Helpers.Panic(this, PanicType.Assume, "reserved2 is not null. Possible error.");

            uint numUnkTable = br.ReadUInt32();     //number of entries in an unknown array, messes up all samples if anything is modified
            uint numSfx = br.ReadUInt32();          //number of sample declarations, contains all sfx entries (not instruments)
            uint numEngineSfx = br.ReadUInt32();    //number of engine sound array entries

            uint numBanks = br.ReadUInt32();        //number of banks
            uint numSequences = br.ReadUInt32();    //number of sequences

            int sampleDataSize = br.ReadInt32();    //whole sample data size to the last seq pointer

            for (int i = 0; i < numUnkTable; i++)
            {
                if (br.ReadUInt16() != 0)
                    Helpers.Panic(this, PanicType.Assume, "upper word is not 0.");

                unk.Add(br.ReadUInt16());
            }

            samplesSfx = InstanceList<InstrumentShort>.FromReader(br, (uint)br.Position, numSfx);
            samplesEngineSfx = InstanceList<InstrumentShort>.FromReader(br, (uint)br.Position, numEngineSfx);


            for (int i = 0; i < numBanks; i++)
                ptrBanks.Add(br.ReadUInt16() * Meta.SectorSize);

            for (int i = 0; i < numSequences; i++)
                ptrSeqs.Add(br.ReadUInt16() * Meta.SectorSize);


            foreach (var ptr in ptrBanks)
            {
                br.Jump(ptr);
                Banks.Add(new Bank(br));
            }

            foreach (var ptr in ptrSeqs)
            {
                br.Jump(ptr);
                Songs.Add(CSEQ.FromReader(br));
            }

            for (int i = 0; i < Songs.Count; i++)
            {
                if (seqnames.ContainsKey(i))
                    Songs[i].name = seqnames[i];
                else
                    Songs[i].name = i.ToString("00");
            }

            Console.Write(ToString());
        }

        public void ExportCSEQ(string path)
        {
            Helpers.CheckFolder(path);

            CSEQ.PatchMidi = true;
            CSEQ.IgnoreVolume = true;

            foreach (var song in Songs)
            {
                CSEQ.PatchName = song.name;
                song.LoadMetaInstruments(song.name);

                song.Save(Path.Combine(path, $"{song.name}.cseq"));
                song.Songs[0].ExportMIDI(Path.Combine(path, $"{song.name}.mid"), song);
            }
        }

        public void ExportCSEQ(string path, BinaryReaderEx br)
        {
            CSEQ.PatchMidi = true;
            CSEQ.IgnoreVolume = true;

            string pathBank = Path.Combine(path, "banks");
            Helpers.CheckFolder(pathBank);

            for (int i = 0; i < ptrBanks.Count - 1; i++)
            {
                br.Jump(ptrBanks[i]);

                string fn = String.Format($"{i.ToString("00")}_{(banknames.ContainsKey(i) ? banknames[i] : "bank")}.bnk");
                Console.WriteLine("Extracting " + fn);

                fn = Path.Combine(pathBank, fn);

                Helpers.WriteToFile(fn, br.ReadBytes(ptrBanks[i + 1] - ptrBanks[i]));
            }

            Console.WriteLine("---");

            string pathSeq = Path.Combine(path, "songs");
            Helpers.CheckFolder(pathSeq);

            int j = 0;

            foreach (int ptrSeq in ptrSeqs)
            {
                string fn = "";

                if (reg != "")
                {
                    fn = String.Format(
                        "{0}_{1}.cseq",
                        j.ToString("00"),
                        seqnames.ContainsKey(j) ? seqnames[j] : "sequence"
                    );
                }
                else
                {
                    fn = String.Format("{0}_{1}.cseq", j.ToString("00"), "sequence");
                }

                Console.WriteLine("Extracting " + fn);

                fn = Path.Combine(pathSeq, fn);

                br.Jump(ptrSeq);
                int size = br.ReadInt32();
                br.Jump(ptrSeq);

                byte[] data = br.ReadBytes(size);
                Helpers.WriteToFile(fn, data);

                CSEQ seq = CSEQ.FromFile(fn);
                seq.name = seqnames[j];
                CSEQ.PatchName = seq.name;
                seq.LoadMetaInstruments(seq.name);
                seq.Songs[0].ExportMIDI(Path.ChangeExtension(fn, ".mid"), seq);

                j++;
            }
        }

        public void ExportAllSamples(string path)
        {
            string output = Path.Combine(path, "samples");
            Helpers.CheckFolder(output);

            int i = 0;

            foreach (var bank in Banks)
            {
                bank.ExportAll(i, output);
                i++;
            }
        }

        public static int GetFreq(int sampleId)
        {
            foreach (var sd in samplesSfx)
                if (sd.SampleID == sampleId)
                    return sd.Frequency;

            return -1;
        }

        public void Save(string filename)
        {
            Helpers.CheckFolder(Path.GetDirectoryName(filename));

            using (var bw = new BinaryWriterEx(File.OpenWrite(filename)))
            {
                Write(bw);
            }
        }

        public override string ToString()
        {
            return $"Version: {version.ToString()}\r\nSamples: {samplesSfx.Count}\r\nBanks: {Banks.Count}\r\nSequences: {Songs.Count}";
        }
    }
}
