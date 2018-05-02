using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LkCommon;
using LkCommon.Translator;
using System.IO;

namespace Ladirvirelyl.Core
{
    class LkAssembler : DynamicAssembler
    {
        IList<string> inFiles;
        IDictionary<string, bool> kuexok;

        public LkAssembler(List<string> inFiles) : base()
        {
            this.inFiles = inFiles;
            this.kuexok = new Dictionary<string, bool>();
        }

        public void Execute(string outFile)
        {
            int count = 0;
            foreach (var inFile in inFiles)
            {
                IList<string> wordList = Read(inFile);
                Analyze(wordList, count++);
            }

            Write(outFile);
        }

        private IList<string> Read(string inFile)
        {
            List<string> wordList = new List<string>();

            using (var reader = new StreamReader(inFile, new UTF8Encoding(false)))
            {
                StringBuilder buffer = new StringBuilder();

                while (!reader.EndOfStream)
                {
                    char c = Convert.ToChar(reader.Read());
                    if (char.IsWhiteSpace(c))
                    {
                        if (buffer.Length > 0)
                        {
                            wordList.Add(buffer.ToString());
                            buffer.Clear();
                        }
                    }
                    else if (c == '@')
                    {
                        if (buffer.Length > 0)
                        {
                            buffer.Append(c);
                            wordList.Add(buffer.ToString());
                            buffer.Clear();
                        }
                        else if (wordList.Count > 0)
                        {
                            string str = wordList[wordList.Count - 1];
                            if (char.IsDigit(str.Last()))
                            {
                                wordList[wordList.Count - 1] = str + c;
                            }
                            else
                            {
                                throw new ApplicationException("Invalid Pattern '@'");
                            }
                        }
                        else
                        {
                            throw new ApplicationException("Invalid Pattern '@'");
                        }
                    }
                    else if (c == '+')
                    {
                        if (buffer.Length == 0)
                        {
                            string str = wordList[wordList.Count - 1];
                            if (char.IsDigit(str.Last()))
                            {
                                buffer.Append(str);
                                wordList.RemoveAt(wordList.Count - 1);
                            }
                            else
                            {
                                throw new ApplicationException("Invalid Pattern '+'");
                            }
                        }

                        buffer.Append(c);

                        c = ' ';
                        while (!reader.EndOfStream && char.IsWhiteSpace(c))
                        {
                            c = Convert.ToChar(reader.Read());
                        }

                        if (char.IsDigit(c) || c == 'f')
                        {
                            buffer.Append(c);
                        }
                        else
                        {
                            throw new ApplicationException("Invalid Pattern '+'");
                        }

                    }
                    else
                    {
                        buffer.Append(c);
                    }
                }
            }

            return wordList;
        }

        private void Analyze(IList<string> wordList, int v)
        {
            bool isMain = false;
            bool isCI = false;
            
            for (int i = 0; i < wordList.Count; i++)
            {
                var str = wordList[i];

                if (str == "'c'i")
                {
                    isCI = true;
                }
                else if (str == "'i'c")
                {
                    isCI = false;
                }
                else
                {
                    string label;
                    Operand head, middle, tail;
                    Mnemonic mne;

                    (Operand, Operand) GetParam(IList<string> list, ref int count)
                    {
                        if (isCI)
                        {

                        }
                        else
                        {

                        }

                        return (null, null);
                    }
                    
                    switch (str)
                    {
                        case "nll":
                            Nll(wordList[++i]);

                            if (wordList[i + 1] == "l'")
                            {
                                throw new ApplicationException($"Wrong label nll {wordList[i]} l'");
                            }
                            break;
                        case "l'":
                            if(i == 0)
                            {
                                throw new ApplicationException($"Wrong label l'");
                            }

                            L(wordList[++i]);
                            break;
                        case "kue":
                            label = wordList[++i];

                            kuexok[label] = true;
                            isMain = false;
                            break;
                        case "xok":
                            label = wordList[++i];


                            if (!kuexok.ContainsKey(label))
                            {
                                kuexok[label] = false;
                            }
                            break;
                        case "krz":
                        case "kRz":
                            (head, tail) = GetParam(wordList, ref i);

                            break;
                        case "ata":
                        case "nta":
                        case "ada":
                        case "ekc":
                        case "dal":
                        case "dto":
                        case "dtosna":
                        case "dro":
                        case "dRo":
                        case "malkrz":
                        case "malkRz":
                            break;
                        case "lat":
                        case "latsna":
                            mne = (Mnemonic)Enum.Parse(typeof(Mnemonic), str);
                            break;
                        case "kak":
                            throw new NotSupportedException("Not Supported 'kak'");
                        case "nac":
                            mne = (Mnemonic)Enum.Parse(typeof(Mnemonic), str);
                            break;
                        case "fi":
                            break;
                        case "inj":
                            mne = (Mnemonic)Enum.Parse(typeof(Mnemonic), str);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

    }
}
