﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CTRFramework.Shared;

namespace CTRFramework
{
    //just a made up name
    public class ColData : IRead
    {
        public ushort flag;
        public ushort id;
        public Vector3s v1;
        public Vector3s v2;
        public Vector3s v3;
        public Vector3s v4;
        public uint ptrQuadBlock;

        public ColData(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            flag = br.ReadUInt16();
            id = br.ReadUInt16();
            v1 = new Vector3s(br);
            v2 = new Vector3s(br);
            v3 = new Vector3s(br);
            v4 = new Vector3s(br);
            ptrQuadBlock = br.ReadUInt32();
        }

        public override string ToString()
        {
            return String.Format(
            "id = {1} | flag = {0} | ptr = {6}\r\n{2} {3}\r\n{4} {5}",
            flag, id, v1, v2, v3,v4, ptrQuadBlock.ToString("X8")
            );
        }
    }
}
