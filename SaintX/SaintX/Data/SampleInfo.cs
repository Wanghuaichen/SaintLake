﻿using SaintX.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaintX.Data
{
    class SampleInfos:IEnumerable
    {
        Dictionary<CellPosition, SampleInfo> allSampleInfos = new Dictionary<CellPosition, SampleInfo>();
        public void SetLine(int lineIndex, List<SampleInfo> cellInfosThisLine, int startIndex = 0)
        {
            for (int i = 0; i < cellInfosThisLine.Count; i++)
            {
                CellPosition cellPos = new CellPosition(lineIndex, i + startIndex);
                allSampleInfos[cellPos] = cellInfosThisLine[i];
            }
        }

        public bool Contains(int r, int c)
        {
            return allSampleInfos.ContainsKey(new CellPosition(r, c));
        }
        public List<SampleInfo> GetLine(int lineIndex)
        {
            return allSampleInfos.Where(x => x.Key.colIndex == lineIndex).Select(x => x.Value).ToList();
        }
        public void Clear()
        {
            allSampleInfos.Clear();
        }
        public SampleInfo this[int r, int c]
        {
            get
            {
                return allSampleInfos[new CellPosition(r, c)];
            }
            set
            {
                allSampleInfos[new CellPosition(r, c)] = value;
            }
        }
       
        public SampleInfo this[CellPosition cellPos]
        {
            get
            {
                return allSampleInfos[cellPos];
            }
            set
            {
                allSampleInfos[cellPos] = value;
            }
        }

        public bool AlreadyHasInfo()
        {
            var list = allSampleInfos.Values.ToList();
            return list.Exists(x => IsMeanfulAssay(x.colorfulAssay.Name));
        }

        private bool IsMeanfulAssay(string s)
        {
            return s != "" && s != Resources.EmptyAssay;
        }



        public IEnumerator GetEnumerator()
        {
            foreach (var pair in allSampleInfos)
                yield return pair;
        }
    }

    public class SampleInfo
    {
        public ColorfulAssay colorfulAssay;
        public string barcode;
        public SampleInfo(ColorfulAssay colorfulAssay, string barcode)
        {
            this.barcode = barcode;
            this.colorfulAssay = colorfulAssay;
        }
    }

    public struct CellPosition
    {
        public int colIndex;
        public int rowIndex;
        public CellPosition(int ix, int iy)
        {
            colIndex = ix;
            rowIndex = iy;
        }
    }
}