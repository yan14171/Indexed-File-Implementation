﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexedFile.Basics
{
    public class IndexLineComparer : IComparer<IndexLine>
    {
        public int Compare(IndexLine x, IndexLine y)
        {
            return x.Key[2] - y.Key[2];
        }
    }
}
