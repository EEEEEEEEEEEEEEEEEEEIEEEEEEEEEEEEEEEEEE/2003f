using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LkCommon.Translator
{
    public abstract class DynamicAssembler
    {
        private IList<Code> codeList;
        private IDictionary<string, int> labels;
        private int count;

        protected static readonly Operand F0 = new Operand(Register.F0);
        protected static readonly Operand F1 = new Operand(Register.F1);
        protected static readonly Operand F2 = new Operand(Register.F2);
        protected static readonly Operand F3 = new Operand(Register.F3);
        protected static readonly Operand F5 = new Operand(Register.F5);
        protected static readonly Operand XX = new Operand(Register.XX);

        protected static readonly FiType XTLO = new FiType(Mnemonic.XTLO);
        protected static readonly FiType XYLO = new FiType(Mnemonic.XYLO);
        protected static readonly FiType CLO = new FiType(Mnemonic.CLO);
        protected static readonly FiType XOLO = new FiType(Mnemonic.XOLO);
        protected static readonly FiType LLO = new FiType(Mnemonic.LLO);
        protected static readonly FiType NIV = new FiType(Mnemonic.NIV);
        protected static readonly FiType XTLONYS = new FiType(Mnemonic.XTLONYS);
        protected static readonly FiType XYLONYS = new FiType(Mnemonic.XYLONYS);
        protected static readonly FiType XOLONYS = new FiType(Mnemonic.XOLONYS);
        protected static readonly FiType LLONYS = new FiType(Mnemonic.LLONYS);

        protected DynamicAssembler()
        {
            codeList = new List<Code>();
            labels = new Dictionary<string, int>();
            count = 0;
        }


        /// <summary>
        /// 指定されたファイル名のファイルにバイナリを書き込みます
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        public void Write(string fileName)
        {
            using (var writer = new BinaryWriter(File.OpenWrite(fileName)))
            {
                foreach (var code in this.codeList)
                {
                    writer.Write((byte)code.Mnemonic);
                    
                    foreach (var opd in code.Operands)
                    {
                        if(opd.IsLabel)
                        {
                            if(this.labels.ContainsKey(opd.Label))
                            {
                                uint val = (uint)(LkConstant.DEFAULT_INITIAL_NX + this.labels[opd.Label]);
                                opd.ModRm.DispImm = val;
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }

                        }

                        WriteModRm(writer, opd.ModRm);
                    }
                }
            }
        }

        protected Operand Seti(Operand opd)
        {
            return opd.ToAddressing();
        }

        protected Operand ToOperand(uint val)
        {
            return new Operand(val);
        }

        protected Operand ToOperand(string label, bool address = true)
        {
            return new Operand(label);
        }

        #region GeneratingMethod
        
        private void Append(Mnemonic mne, Operand opd1, Operand opd2, Operand opd3 = null)
        {
            bool IsFi()
            {
                return mne == Mnemonic.LLONYS
                    || mne == Mnemonic.XTLONYS
                    || mne == Mnemonic.XOLONYS
                    || mne == Mnemonic.XYLONYS
                    || mne == Mnemonic.CLO
                    || mne == Mnemonic.NIV
                    || mne == Mnemonic.LLO
                    || mne == Mnemonic.XTLO
                    || mne == Mnemonic.XOLO
                    || mne == Mnemonic.XYLO;
            }

            Code code = new Code();
            Console.WriteLine("{0:X4}: {1:X2} {2} {3} {4} {5}", count, (byte)mne, mne, opd1, opd2, opd3);

            if (!IsFi() && (opd2.IsLabel || opd2.IsImm))
            {
                throw new ArgumentException($"Invalid Argument: {opd2}");
            }

            if (opd3 != null && (opd3.IsLabel || opd3.IsImm))
            {
                throw new ArgumentException($"Invalid Argument: {opd3}");
            }
            code.Mnemonic = mne;

            code.Operands.Add(new CodeOperand
            {
                ModRm = ToModRm(opd1),
                Label = opd1.IsLabel ? opd1.Label : null,
            });

            code.Operands.Add(new CodeOperand
            {
                ModRm = ToModRm(opd2),
                Label = opd2.IsLabel ? opd2.Label : null,
            });

            if(opd3 != null)
            {

                code.Operands.Add(new CodeOperand
                {
                    ModRm = ToModRm(opd3),
                    Label = null,
                });
            }

            count += code.CodeLength;
            codeList.Add(code);
        }

        private void WriteModRm(BinaryWriter writer, ModRm modrm)
        {
            uint disp = modrm.DispImm;

            writer.Write(modrm.Code);

            if ((modrm.Mode & 0x18) == 0x18) { return; }

            int count;
            switch (modrm.Mode & 0x7)
            {
                case 0x4:
                    count = 3;
                    break;
                case 0x5:
                    count = 2;
                    break;
                case 0x6:
                    count = 0;
                    break;
                default:
                    count = 4;
                    break;
            }

            for (int i = count; i < 4; i++)
            {
                writer.Write((byte)(disp >> ((3 - i) * 8)));
            }
        }

        private ModRm ToModRm(Operand opd)
        {
            ModRm modrm = new ModRm();

            if (opd.IsLabel)
            {
                modrm.Reg = (byte)Register.F0;
                modrm.Mode = 6;
                modrm.Disp32 = 0;
            }
            else if (opd.IsImm)
            {
                modrm.Reg = 0;
                modrm.Mode = GetDispType(opd.Disp.Value);
                modrm.Disp32 = opd.Disp.Value;
            }
            else if (opd.SecondReg.HasValue)
            {
                if (opd.IsAddress)
                {
                    modrm.Reg = (byte)opd.Reg.Value;
                    modrm.Mode = (byte)(0b11000 | (byte)opd.SecondReg.Value);
                    modrm.Disp32 = 0U;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else if (opd.Disp.HasValue)
            {
                modrm.Reg = (byte)opd.Reg.Value;
                if (opd.IsAddress)
                {
                    modrm.Mode = (byte)(0b10000 | GetDispType(opd.Disp.Value));
                    modrm.Disp32 = opd.Disp.Value;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else {
                modrm.Reg = (byte)opd.Reg.Value;
                modrm.Mode = (byte)(opd.IsAddress ? 0b10000 : 0);
                modrm.Disp32 = 0U;
            }

            return modrm;
        }

        private byte GetDispType(uint disp)
        {
            if (disp <= byte.MaxValue && disp >= byte.MinValue)
            {
                return 0x4;
            }
            else if (disp <= ushort.MaxValue && disp >= ushort.MinValue)
            {
                return 0x5;
            }
            else
            {
                return 0x6;
            }
        }

        #endregion

        #region Operation
        
        /// <summary>
        /// 後置ラベルを定義します．
        /// </summary>
        /// <param name="name">ラベル名</param>
        protected void L(string name)
        {
            int? count = this.codeList.LastOrDefault()?.CodeLength;
            if(count.HasValue)
            {
                this.labels.Add(name, this.count - count.Value);
            }
            else
            {
                throw new ArgumentException();
            }
        }
        
        /// <summary>
        /// 前置ラベルを定義します．
        /// </summary>
        /// <param name="name">ラベル名</param>
        protected void Nll(string name)
        {
            this.labels.Add(name, this.count);
        }

        /// <summary>
        /// ataを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Ata(uint val, Operand opd)
        {
            Ata(new Operand(val), opd);
        }
        
        /// <summary>
        /// ataを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Ata(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.ATA, opd1, opd2);
        }

        /// <summary>
        /// ntaを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Nta(uint val, Operand opd)
        {
            Nta(new Operand(val), opd);
        }
        
        /// <summary>
        /// ntaを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Nta(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.NTA, opd1, opd2);
        }

        /// <summary>
        /// adaを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Ada(uint val, Operand opd)
        {
            Ada(new Operand(val), opd);
        }

        /// <summary>
        /// adaを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Ada(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.ADA, opd1, opd2);
        }

        /// <summary>
        /// ekcを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Ekc(uint val, Operand opd)
        {
            Ekc(new Operand(val), opd);
        }

        /// <summary>
        /// ekcを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Ekc(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.EKC, opd1, opd2);
        }

        /// <summary>
        /// dtoを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Dto(uint val, Operand opd)
        {
            Dto(new Operand(val), opd);
        }

        /// <summary>
        /// dtoを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Dto(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.DTO, opd1, opd2);
        }

        /// <summary>
        /// droを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Dro(uint val, Operand opd)
        {
            Dro(new Operand(val), opd);
        }

        /// <summary>
        /// droを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Dro(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.DRO, opd1, opd2);
        }

        /// <summary>
        /// dtosnaを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Dtosna(uint val, Operand opd)
        {
            Dtosna(new Operand(val), opd);
        }

        /// <summary>
        /// dtosnaを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Dtosna(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.DTOSNA, opd1, opd2);
        }

        /// <summary>
        /// dalを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Dal(uint val, Operand opd)
        {
            Dal(new Operand(val), opd);
        }

        /// <summary>
        /// dalを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Dal(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.DAL, opd1, opd2);
        }

        /// <summary>
        /// nacを表すメソッドです．
        /// </summary>
        /// <param name="opd">オペランド</param>
        protected void Nac(Operand opd)
        {
            Append(Mnemonic.DAL, new Operand(0), opd);
        }

        /// <summary>
        /// krzを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Krz(uint val, Operand opd)
        {
            Krz(new Operand(val), opd);
        }

        /// <summary>
        /// krzを表すメソッドです．
        /// </summary>
        /// <param name="name">ラベルや関数名</param>
        /// <param name="opd">オペランド</param>
        protected void Krz(string name, Operand opd)
        {
            Krz(new Operand(name, true), opd);
        }

        /// <summary>
        /// krzを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Krz(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.KRZ, opd1, opd2);
        }

        /// <summary>
        /// malkrzを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Malkrz(uint val, Operand opd)
        {
            Malkrz(new Operand(val), opd);
        }

        /// <summary>
        /// malkrzを表すメソッドです．
        /// </summary>
        /// <param name="name">ラベルや関数名</param>
        /// <param name="opd">オペランド</param>
        protected void Malkrz(string name, Operand opd)
        {
            Malkrz(new Operand(name, true), opd);
        }

        /// <summary>
        /// malkrzを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Malkrz(Operand opd1, Operand opd2)
        {
            Append(Mnemonic.MALKRZ, opd1, opd2);
        }

        /// <summary>
        /// fi系を表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd">オペランド</param>
        protected void Fi(uint val, Operand opd, FiType f)
        {
            Append(f.mne, new Operand(val), opd);
        }

        /// <summary>
        /// fi系を表すメソッドです．
        /// </summary>
        /// <param name="opd">オペランド</param>
        /// <param name="val">即値</param>
        protected void Fi(Operand opd, uint val, FiType f)
        {
            Fi(opd, new Operand(val), f);
        }

        /// <summary>
        /// fi系を表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        protected void Fi(Operand opd1, Operand opd2, FiType f)
        {
            Append(f.mne, opd1, opd2);
        }

        /// <summary>
        /// injを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd2">オペランド</param>
        /// <param name="opd3">オペランド</param>
        protected void Inj(uint val, Operand opd2, Operand opd3)
        {
            Inj(new Operand(val), opd2, opd3);
        }

        /// <summary>
        /// injを表すメソッドです．
        /// </summary>
        /// <param name="name">ラベルや関数名</param>
        /// <param name="opd2">オペランド</param>
        /// <param name="opd3">オペランド</param>
        protected void Inj(string name, Operand opd2, Operand opd3)
        {
            Inj(new Operand(name, true), opd2, opd3);
        }

        /// <summary>
        /// injを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        /// <param name="opd3">オペランド</param>
        protected void Inj(Operand opd1, Operand opd2, Operand opd3)
        {
            Append(Mnemonic.INJ, opd1, opd2, opd3);
        }

        /// <summary>
        /// latを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd2">オペランド</param>
        /// <param name="opd3">オペランド</param>
        protected void Lat(uint val, Operand opd2, Operand opd3)
        {
            Lat(new Operand(val), opd2, opd3);
        }

        /// <summary>
        /// latを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        /// <param name="opd3">オペランド</param>
        protected void Lat(Operand opd1, Operand opd2, Operand opd3)
        {
            Append(Mnemonic.LAT, opd1, opd2, opd3);
        }

        /// <summary>
        /// latsnaを表すメソッドです．
        /// </summary>
        /// <param name="val">即値</param>
        /// <param name="opd2">オペランド</param>
        /// <param name="opd3">オペランド</param>
        protected void Latsna(uint val, Operand opd2, Operand opd3)
        {
            Latsna(new Operand(val), opd2, opd3);
        }

        /// <summary>
        /// latsnaを表すメソッドです．
        /// </summary>
        /// <param name="opd1">オペランド</param>
        /// <param name="opd2">オペランド</param>
        /// <param name="opd3">オペランド</param>
        protected void Latsna(Operand opd1, Operand opd2, Operand opd3)
        {
            Append(Mnemonic.LAT, opd1, opd2, opd3);
        }
    }

    #endregion
}
