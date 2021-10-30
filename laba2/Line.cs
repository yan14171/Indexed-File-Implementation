using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laba2
{
    public class Line
    {
        static int cnt = 0;

        public int cntCur;

        public Line()
        {
            cntCur = Line.cnt++;
        }

        public Line(string word)
        {
            if (word.Length > 2)
                throw new ArgumentException("Lines must be 2 bytes long");

            cntCur = Line.cnt++;

            Data = Encoding.UTF8.GetBytes(word,0,2);

            Key = keyGen();
        }

        public Line(byte[] bts)
        {
            if (bts.Length != 5)
                throw new ArgumentException("Wrong number of bytes passed to Line constructor");

            cntCur = Line.cnt++;
            data = bts[3..5];
            key = bts[0..3];
        }

        byte[] key;
        byte[] data;

        public byte[] Data { 
            get {
                return data;
            }
            private set
            {
                if(value.Length > 2)
                    throw new ArgumentException("Wrong number of bytes passed to data setter");

                data = value;
            }
        }

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

        private byte[] keyGen()
        {
            Random rand = new Random();

            byte[] key = new byte[3];

            key[0] = (byte)rand.Next(0, 9);

            key[1] = Encoding.UTF8.GetBytes("-")[0];

            key[2] = (byte)rand.Next(1, 257);


            return key;
        }

        public override string ToString()
        {
            return $"Key: {Key[0]} - {Key[2]} \t Data: {Encoding.UTF8.GetString(Data)}";
        }

        public byte[] ToBytes()
        {
            return key.Concat(data).ToArray();
        }
    }
}
