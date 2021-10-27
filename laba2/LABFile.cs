using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace laba2
{
    internal class LABFile : IDisposable
    {
        public LABFile()
        {

        }

        public LABFile(string path, FileSettings settings)
        {
            meta = new byte[4];

            meta[0] = settings.LZ;
            meta[1] = settings.LK;
            meta[2] = settings.KZ;
            meta[3] = settings.LB;

            if (File.Exists(path))
            {
                byte[] metabt = new byte[24];
                _mainStr = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                _mainStr.Seek(-24, SeekOrigin.End);
                _mainStr.Read(metabt);
                indexStart = BitConverter.ToInt64(metabt[0..8]);
                overflowStart = BitConverter.ToInt64(metabt[8..16]);
                mainStart = BitConverter.ToInt64(metabt[16..24]);
            }
            else
            {
                _mainStr = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                _mainStr.Write(createStructure());
            }

            indexStart = 4;
        }

        FileStream _mainStr;

        byte[] meta;

        long indexStart,
             overflowStart,
             mainStart;

        byte[] createStructure()
        {
            List<byte> fin = new List<byte>();
            byte[] bts = new byte[1] { 0 };

            byte indexLineLength = (byte)(meta[1] + 1);
            _mainStr.Seek(0, SeekOrigin.Begin);
            fin.AddRange(meta);
            indexStart = fin.Count;

            for (int i = 0; i < meta[3] / indexLineLength; i++)
            { 
                fin.AddRange(bts);

                fin.AddRange(new byte[40]);
            }

            fin.AddRange(bts);
            overflowStart = fin.Count;
            fin.AddRange(new byte[80]);

            mainStart = fin.Count;
            fin.AddRange(new byte[500]);

            fin.AddRange(Encoding.UTF8.GetBytes("$$$"));
            fin.AddRange(BitConverter.GetBytes(indexStart));
            fin.AddRange(BitConverter.GetBytes(overflowStart));
            fin.AddRange(BitConverter.GetBytes(mainStart));

            return fin.ToArray();
        }

        public Line GetLine(byte[] key)
        {
            var EA = getBlockNumberAddressByKey(key);

            _mainStr.Seek(EA, SeekOrigin.Begin);

            byte blockCount = (byte)_mainStr.ReadByte();

            var lines = new List<IndexLine>();
            byte[] crntbytes = new byte[4];
            for (int i = 0; i < blockCount; i++)
            {
                _mainStr.Read(crntbytes, 0, 4);
                lines.Add(new IndexLine(crntbytes));
            }

            var index = lines
                .ToList()
                .BinarySearch
                (new IndexLine(key.Concat(new byte[] { 255 })
                                    .ToArray()), new IndexLineComparer());

            var indexLine = lines[index];

            _mainStr.Seek(mainStart, SeekOrigin.Begin);

            var linesCount = _mainStr.ReadByte();
            if (indexLine.LineNum + 1 > linesCount)
                throw new Exception("Seeking for line number, that exceeds the file maximum capacity");

            _mainStr.Seek(indexLine.LineNum * 5, SeekOrigin.Current);
            var mainBts = new byte[5];
            _mainStr.Read(mainBts);
            return new Line(mainBts);
        }

        public IEnumerable<Line> GetAllLines()
        {
            _mainStr.Seek(mainStart, SeekOrigin.Begin);

            var lineCount = _mainStr.ReadByte();
            byte[] crnt = new byte[5];

            for (int i = 0; i < lineCount; i++)
            {
                _mainStr.Read(crnt);

                if(crnt[1] == 0)
                {
                    i--;
                    continue;
                }
                yield return new Line(crnt);
            }
        }

        public bool AddLine(Line value)
        {
            var indexLine = new IndexLine(value.Key, value.cntCur);

            return InsertIndex(indexLine) && InsertMain(value);
        }

        public bool DeleteLine(byte[] key)
        {
            var mainIndex = DeleteIndex(key);

            DeleteMain(mainIndex);

            return true;
        }

        public string ReadRawData()
        {
            byte[] bts = new byte[_mainStr.Length];

            _mainStr.Read(bts);

            string result = "";

            foreach (var item in bts)
            {
                result += item.ToString();
                result += " ";
            }

            return result;
        }

        public void Dispose()
        {
            _mainStr.Close();
            _mainStr.Dispose();
        }

        #region private

        private bool addToOverflow(IndexLine indexLine)
        {
            _mainStr.Seek(overflowStart, SeekOrigin.Begin);
            var cnt = _mainStr.ReadByte();
            if (cnt > 80 / (meta[3] + 1))
                return false;

            _mainStr.Seek(overflowStart + cnt * (meta[3] + 1), SeekOrigin.Begin);
            _mainStr.Write(indexLine.ToBytes());
            return true;
        }

        private bool InsertMain(Line value)
        {
            _mainStr.Seek(mainStart, SeekOrigin.Begin);
            
            var lineCount = _mainStr.ReadByte();
            if (lineCount > meta[2])
                return false;

            var absPosition = mainStart + 1 + lineCount * (meta[1] + meta[0]);
            _mainStr.Seek(absPosition, SeekOrigin.Begin);
            _mainStr.Write(value.ToBytes());

            _mainStr.Seek(mainStart, SeekOrigin.Begin);
            _mainStr.WriteByte((byte)(++lineCount));
            return true;
        }

        private bool DeleteMain(int lineNumber)
        {
            _mainStr.Seek(mainStart, SeekOrigin.Begin);
            var lineCount = _mainStr.ReadByte();

            _mainStr.Seek(lineNumber * 5, SeekOrigin.Current);
            _mainStr.Write(new byte[5], 0, 5);    

            _mainStr.Seek(mainStart, SeekOrigin.Begin);
            _mainStr.WriteByte((byte)(--lineCount));
            return true;
        }

        private bool InsertIndex(IndexLine indexLine)
        {
            long EA = getBlockNumberAddressByKey(indexLine.Key);
            //should be on the count* byte

            _mainStr.Seek(EA, SeekOrigin.Begin);

            byte blockCount = (byte)_mainStr.ReadByte();
            if (blockCount >= meta[3] / (meta[1] + 1))
            {
                if (addToOverflow(indexLine)) return true;
                return false;
            }

            var lines = new List<IndexLine>();
            byte[] crntbytes = new byte[4];
            for (int i = 0; i < blockCount; i++)
            {
                _mainStr.Read(crntbytes, 0, 4);
                lines.Add(new IndexLine(crntbytes));
            }

            lines = insertIndex(lines.ToList(), indexLine);
            _mainStr.Seek(EA + 1, SeekOrigin.Begin);
            _mainStr.Write(new byte[meta[3]]);
            _mainStr.Seek(EA + 1, SeekOrigin.Begin);
            _mainStr.Write(lines
                    .Select(n => n.ToBytes())
                    .Aggregate((a, b) => a.Concat(b).ToArray())
                    .ToArray());
            _mainStr.Seek(EA, SeekOrigin.Begin);
            _mainStr.WriteByte(++blockCount);
            return true;
        }

        private int DeleteIndex(byte[] key)
        {
            long EA = getBlockNumberAddressByKey(key);
            _mainStr.Seek(EA,SeekOrigin.Begin);

            byte blockcount = (byte)_mainStr.ReadByte();

            var lines = new List<IndexLine>();
            byte[] crntbytes = new byte[4];
            for (int i = 0; i < blockcount; i++)
            {
                _mainStr.Read(crntbytes, 0, 4);
                lines.Add(new IndexLine(crntbytes));
            }

            var index = lines
                .ToList()
                .BinarySearch
                (new IndexLine(key.Concat(new byte[] { 255 })
                                    .ToArray()), new IndexLineComparer());

            if (index < 0) throw new ArgumentException("Such line couldn't be found");

            var deletedLineNumber = lines[index].LineNum;

            lines.RemoveAt(index);
            _mainStr.Seek(EA + 1, SeekOrigin.Begin);
            _mainStr.Write(new byte[meta[3]]);
            _mainStr.Seek(EA + 1, SeekOrigin.Begin);

            if(lines.Count != 0)
            _mainStr.Write(lines
                    .Select(n => n.ToBytes())
                    .Aggregate((a, b) => a.Concat(b).ToArray())
                    .ToArray());

            _mainStr.Seek(EA, SeekOrigin.Begin);
            _mainStr.WriteByte(--blockcount);
            return deletedLineNumber;
        }

        private List<IndexLine> insertIndex(List<IndexLine> lines, IndexLine indexLine)
        {
            var index = lines.BinarySearch(indexLine, new IndexLineComparer());

            if (index < 0)
                lines.Insert(~index , indexLine);
            else 
                lines.Insert(index, indexLine);

            return lines;
        }

        private long getBlockNumberAddressByKey(byte[] key)
        {
            var iLinesInBlock = meta[3] / (meta[1] + 1);
            var blockNumber = key[0] % iLinesInBlock;

            return indexStart + blockNumber * meta[3];
        }

        #endregion

    }
}