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
                    char c = System.Convert.ToChar(reader.Read());
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
                            c = System.Convert.ToChar(reader.Read());
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
            
            (string, string, int) GetParam(int count)
            {
                string h, t;
                if(isCI)
                {
                    t = wordList[++count];
                    h = wordList[++count];
                }
                else
                {
                    h = wordList[++count];
                    t = wordList[++count];
                }
                return (h, t, count);
            }

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
                    string head, middle, tail;

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
                            (head, tail, i) = GetParam(i);

                            Krz(Convert(head), Convert(tail));
                            break;
                        case "ata":
                            (head, tail, i) = GetParam(i);

                            Ata(Convert(head), Convert(tail));
                            break;
                        case "nta":
                            (head, tail, i) = GetParam(i);

                            Nta(Convert(head), Convert(tail));
                            break;
                        case "ada":
                            (head, tail, i) = GetParam(i);

                            Ada(Convert(head), Convert(tail));
                            break;
                        case "ekc":
                            (head, tail, i) = GetParam(i);

                            Ekc(Convert(head), Convert(tail));
                            break;
                        case "dal":
                            (head, tail, i) = GetParam(i);
                            
                            Dal(Convert(head), Convert(tail));
                            break;
                        case "dto":
                            (head, tail, i) = GetParam(i);

                            Dto(Convert(head), Convert(tail));
                            break;
                        case "dtosna":
                            (head, tail, i) = GetParam(i);

                            Dtosna(Convert(head), Convert(tail));
                            break;
                        case "dro":
                        case "dRo":
                            (head, tail, i) = GetParam(i);

                            Dro(Convert(head), Convert(tail));
                            break;
                        case "malkrz":
                        case "malkRz":
                            (head, tail, i) = GetParam(i);

                            Malkrz(Convert(head), Convert(tail));
                            break;
                        case "lat":
                            if (isCI)
                            {
                                middle = wordList[++i];
                                tail = wordList[++i];
                                head = wordList[++i];
                            }
                            else
                            {
                                head = wordList[++i];
                                middle = wordList[++i];
                                tail = wordList[++i];
                            }

                            Lat(Convert(head), Convert(middle), Convert(tail));
                            break;
                        case "latsna":
                            if (isCI)
                            {
                                middle = wordList[++i];
                                tail = wordList[++i];
                                head = wordList[++i];
                            }
                            else
                            {
                                head = wordList[++i];
                                middle = wordList[++i];
                                tail = wordList[++i];
                            }

                            Latsna(Convert(head), Convert(middle), Convert(tail));
                            break;
                        case "kak":
                            throw new NotSupportedException("Not Supported 'kak'");
                        case "nac":
                            Nac(Convert(wordList[++i]));
                            break;
                        case "fi":
                            head = wordList[++i];
                            tail = wordList[++i];
                            bool isCompare = Enum.TryParse(wordList[++i].ToUpper(), out Mnemonic mne);

                            if(isCompare)
                            {
                                FiType fiType = null;

                                switch(mne)
                                {
                                    case Mnemonic.CLO:
                                        fiType = CLO;
                                        break;
                                    case Mnemonic.NIV:
                                        fiType = NIV;
                                        break;
                                    case Mnemonic.LLO:
                                        fiType = LLO;
                                        break;
                                    case Mnemonic.LLONYS:
                                        fiType = LLONYS;
                                        break;
                                    case Mnemonic.XOLO:
                                        fiType = XOLO;
                                        break;
                                    case Mnemonic.XOLONYS:
                                        fiType = XOLONYS;
                                        break;
                                    case Mnemonic.XTLO:
                                        fiType = XTLO;
                                        break;
                                    case Mnemonic.XTLONYS:
                                        fiType = XTLONYS;
                                        break;
                                    case Mnemonic.XYLO:
                                        fiType = XYLO;
                                        break;
                                    case Mnemonic.XYLONYS:
                                        fiType = XYLONYS;
                                        break;
                                    default:
                                        throw new ApplicationException($"Invalid CompareOperand '{mne}'");
                                }

                                Fi(Convert(head), Convert(tail), fiType);
                            }

                            break;
                        case "inj":
                            if (isCI)
                            {
                                tail = wordList[++i];
                                middle = wordList[++i];
                                head = wordList[++i];
                            }
                            else
                            {
                                head = wordList[++i];
                                middle = wordList[++i];
                                tail = wordList[++i];
                            }

                            Inj(Convert(head), Convert(middle), Convert(tail));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public Operand Convert(string str)
        {
            Operand ToRegisterOperand(Register reg)
            {
                switch (reg)
                {
                    case Register.F0:
                        return F0;
                    case Register.F1:
                        return F1;
                    case Register.F2:
                        return F2;
                    case Register.F3:
                        return F3;
                    case Register.F5:
                        return F5;
                    case Register.XX:
                        return XX;
                    default:
                        throw new ApplicationException($"InvalidParameter '{reg}'");
                }
            }
            
            bool add = str.IndexOf('+') != -1;
            bool seti = str.Last() == '@';
            Operand result;

            if (seti)
            {
                str = str.Remove(str.Length - 1);
            }

            if (add)
            {
                string[] param = str.Split('+');
                uint val1, val2;

                bool succReg1 = Enum.IsDefined(typeof(Register), param[0].ToUpper());
                bool succReg2 = Enum.IsDefined(typeof(Register), param[1].ToUpper());
                bool succVal1 = uint.TryParse(param[0], out val1);
                bool succVal2 = uint.TryParse(param[1], out val2);
                
                if (!succReg1 && !succVal1)
                {
                    throw new ApplicationException($"Invalid Parameter '{str}'");
                }

                if (!succReg2 && !succVal2)
                {
                    throw new ApplicationException($"Invalid Parameter '{str}'");
                }

                if(succVal1 && succVal2)
                {
                    throw new ApplicationException($"Invalid Parameter '{str}'");
                }

                Operand op1 = null, op2 = null;

                if(succReg1)
                {
                    op1 = ToRegisterOperand((Register)Enum.Parse(typeof(Register), param[0], true));
                }

                if(succReg2)
                {
                    op2 = ToRegisterOperand((Register)Enum.Parse(typeof(Register), param[1], true));
                }

                if (succReg1 && succVal2)
                {
                    Console.WriteLine("result: {0} {1}", op1, val2);
                    result = op1 + val2;
                }
                else if (succReg2 && succVal1)
                {
                    Console.WriteLine("result: {0} {1}", val1, op2);
                    result = val1 + op2;
                }
                else
                {
                    result = op1 + op2;
                }
            }
            else
            {
                if (uint.TryParse(str, out uint val))
                {
                    result = ToOperand(val);
                }
                else if (Enum.TryParse(str.ToUpper(), out Register reg))
                {
                    result = ToRegisterOperand(reg);
                }
                else
                {
                    result = ToOperand(str);
                }
            }

            if(seti)
            {
                return Seti(result);
            }
            else
            {
                return result;
            }
        }
    }
}
