﻿using System;
using System.ComponentModel;
using System.IO;
using CTRFramework;
using CTRtools.Helpers;

namespace CTRtools.CSEQ
{
    public class SampleDef : IRead, IWrite
    {

        [CategoryAttribute("General"), DescriptionAttribute("Sample volume.")]
        public byte Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        [CategoryAttribute("General"), DescriptionAttribute("Sample pitch.")]
        public ushort Pitch
        {
            get { return pitch; }
            set { pitch = value; }
        }

        [CategoryAttribute("General"), DescriptionAttribute("Sample ID.")]
        public ushort SampleID
        {
            get { return sampleID; }
            set { sampleID = value; }
        }


        private byte magic1;
        private byte volume;
        private ushort pitch;  //4096 is considered to be 44100
        private ushort sampleID;
        private short always0;

        public int frequency
        {
            //cents needed?
            get { return (int)(pitch * 44100.0 / 4096.0); }
        }


        public void Read(BinaryReader br)
        {
            magic1 = br.ReadByte();
            volume = br.ReadByte();
            pitch = br.ReadUInt16();
            sampleID = br.ReadUInt16();
            always0 = br.ReadInt16();

            if (magic1 != 1)
                throw new Exception(String.Format("SampleDef magic1 = {0}", magic1));

            if (always0 != 0)
                throw new Exception(String.Format("SampleDef always0 = {0} ", always0));
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((byte)magic1);
            bw.Write((byte)volume);
            bw.Write((ushort)pitch);
            bw.Write((ushort)sampleID);
            bw.Write((short)always0);
        }
    }
}
