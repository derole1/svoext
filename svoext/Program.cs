using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace svoext
{
    class Program
    {
        enum FileType
        {
            SVO = 0,
            DDS = 1
        }

        struct SvoFile
        {
            public string filename;
            public FileType filetype;
            public int index;
            public int size;
            public int offset;
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(
                    "svoext by derole\n" +
                    "Usage:\n" + 
                    "svoext u [FileName] - Extract an svo archive"
                    );
                return;
            }
            switch (args[0])
            {
                case "u":
                    UnpackSvo(args[1]);
                    break;
                case "p":
                    PackSvo(args[1]);
                    break;
                default:
                    if (File.Exists(args[0]))
                    {
                        UnpackSvo(args[0]);
                    }
                    else
                    {
                        Console.WriteLine("Unknown file operation!");
                    }
                    break;
            }
            Console.ReadLine();
        }

        static void UnpackSvo(string filename)
        {
            BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open));
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "AVTS")
            {
                Console.WriteLine("Signature is incorrect!");
                return;
            }
            int fileCount = br.ReadInt32();
            br.BaseStream.Position += 0x78; //Skip header for now
            Console.WriteLine("Reading TOC");
            var fileList = new List<SvoFile>();
            for (int i=0; i<fileCount; i++)
            {
                var file = new SvoFile();
                file.filename = Encoding.ASCII.GetString(br.ReadBytes(0x200)).Trim('\0');
                file.filetype = (FileType)br.ReadInt32();
                file.index = br.ReadInt32();
                file.size = br.ReadInt32();
                file.offset = br.ReadInt32();
                br.BaseStream.Position += 0x1F0; //Skip padding?
                fileList.Add(file);
                Console.WriteLine("Found {0} (filetype:{1},index:{2},size:{3},offset:{4})", file.filename, Enum.GetName(typeof(FileType), file.filetype), file.index, file.size, file.offset);
            }
            Console.WriteLine("TOC read!");
            Console.WriteLine("Beginning extraction");
            string extPath = string.Format("{0}_ext", Path.GetFileNameWithoutExtension(filename));
            if (!Directory.Exists(extPath)) { Directory.CreateDirectory(extPath); }
            ExtractFromSvo(br, fileList, extPath);
            Console.WriteLine("Extraction done!");
        }

        static void PackSvo(string filename)
        {

        }

        static void ExtractFromSvo(BinaryReader br, List<SvoFile> files, string path)
        {
            foreach (var file in files)
            {
                br.BaseStream.Position = file.offset;
                File.WriteAllBytes(path + "\\" + file.filename, br.ReadBytes(file.size));
                Console.WriteLine("Extracted {0} (Offset:{1},Size:{2})", file.filename, file.offset, file.size);
            }
        }
    }
}
