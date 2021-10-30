using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laba2
{
    public class IndexLine
    {
        public IndexLine(byte[] key, int lineNum)
        {
            Key = key;
            _lineNum = lineNum;
        }

        public IndexLine(byte[] bts)
        {
            if (bts.Length != 4)
                throw new ArgumentException("Wrong number of bytes passed to IndexLine constructor");

            _lineNum = bts[3];
            key = bts[0..3];
        }

        byte[] key;
        int _lineNum;

        public byte[] Key
        {
            get
            {
                return key;
            }
            private set
            {
                if (value.Length != 3)
                    throw new ArgumentException("Wrong number of bytes passed to key setter");

                key = value;
            }
        }

        public int LineNum { get { return _lineNum; }  private set { _lineNum = value; } }
        public override string ToString()
        {
            return $"Key: {Key[0]} - {Key[2]} \t Line number: {LineNum}";
        }
        public byte[] ToBytes()
        {
            return key.ToList().Concat(BitConverter.GetBytes(_lineNum)).ToArray();
        }
    }
}
