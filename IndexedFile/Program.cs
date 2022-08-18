using System;
using System.Linq;
using IndexedFile.Basics;

Random random = new Random();
FileSettings sts = new FileSettings()
{
    LZ = 2,
    LK = 3,
    KZ = 100,
    LB = 41
};

using (LABFile fl = new LABFile("labfile.lab", sts))
{


    //AddDataAndReadAllExample(fl);
    //ReadAllDataExample(fl);
    //GetOneLineExample(fl);
    AddDataReadAndDeleteExample(fl);
}


void AddDataAndReadAllExample(LABFile fl)
{
    int cnt = 0;

    while (true)
    {
        if (!fl.AddLine(new Line(RandomString(2)))) break;
        cnt++;
    }
    Console.WriteLine($"Added {cnt} random values to the base. Output:");
    ReadAllDataExample(fl);
}

void AddDataReadAndDeleteExample(LABFile fl)
{
    var line = new Line("PA");

    var line1 = new Line("AP");

    fl.AddLine(line);

    fl.AddLine(line1);

    ReadAllDataExample(fl);

    fl.DeleteLine(line.Key);

    ReadAllDataExample(fl);
    Console.WriteLine();
    Console.WriteLine(fl.ReadRawData());
}

void ReadAllDataExample(LABFile fl)
{
    Console.WriteLine("Readling data from file:");

    foreach (var item in fl.GetAllLines())
    {
        Console.WriteLine(item);
    }
}

void GetOneLineExample(LABFile fl)
{
    var line = new Line("PA");
    var line1 = new Line("PP");
    var line2 = new Line("AA");

    fl.AddLine(line);
    fl.AddLine(line1);
    fl.AddLine(line2);

    Console.WriteLine(fl.GetLine(line1.Key));
}


string RandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(Enumerable.Repeat(chars, length)
      .Select(s => s[random.Next(s.Length)]).ToArray());
}
