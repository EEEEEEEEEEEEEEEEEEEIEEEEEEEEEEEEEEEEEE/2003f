using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LkCommon.Processor
{
    public class Emulator
    {
        #region Constants

        /// <summary>
        /// レジスタ数
        /// </summary>
        private static readonly int REGISTER_COUNT = LkConstant.REGISTER_COUNT;

        /// <summary>
        /// 2003fのF5レジスタのデフォルト値
        /// </summary>
        private static readonly uint DEFAULT_INITIAL_F5 = LkConstant.DEFAULT_INITIAL_F5;

        /// <summary>
        /// 2003fのNXレジスタのデフォルト値
        /// </summary>
        private static readonly uint DEFAULT_INITIAL_NX = LkConstant.DEFAULT_INITIAL_NX;

        /// <summary>
        /// アプリケーションのリターンアドレス
        /// </summary>
        private static readonly uint DEFAULT_RETURN_ADDRESS = LkConstant.DEFAULT_RETURN_ADDRESS;

        /// <summary>
        /// デバッグ用出力アドレス
        /// </summary>
        private static readonly uint TVARLON_KNLOAN_ADDRESS = LkConstant.TVARLON_KNLOAN_ADDRESS;

        #endregion

        #region Fields

        /// <summary>
        /// メモリ
        /// </summary>
        readonly Memory memory;

        /// <summary>
        /// ジャンプフラグ
        /// </summary>
        bool flags;
        
        /// <summary>
        /// 汎用レジスタ
        /// </summary>
        readonly IDictionary<Register, uint> registers;

        /// <summary>
        /// デバッグ用出力バッファ
        /// </summary>
        List<string> debugBuffer;

        #endregion

        #region Properties

        /// <summary>
        /// メモリの内容を表すDictionaryを返す．読み込み専用
        /// </summary>
        public IReadOnlyDictionary<uint, byte> Memory
        {
            get => this.memory.Binaries;
        }

        /// <summary>
        /// メモリの内容を表示するかどうか
        /// </summary>
        public bool ViewMemory { get; set; }

        /// <summary>
        /// レジスタの内容を表示するかどうか
        /// </summary>
        public bool ViewRegister { get; set; }

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Emulator()
        {
            this.memory = new Memory();
            this.flags = false;
            this.debugBuffer = new List<string>();

            this.registers = new Dictionary<Register, uint>
            {
                [Register.F0] = 0,
                [Register.F1] = 0,
                [Register.F2] = 0,
                [Register.F3] = 0,
                [Register.F4] = 0,
                [Register.F5] = DEFAULT_INITIAL_F5,
                [Register.F6] = 0,
                [Register.XX] = DEFAULT_INITIAL_NX,
            };

            this.memory.SetValue32(DEFAULT_INITIAL_F5, DEFAULT_RETURN_ADDRESS);
        }

        /// <summary>
        /// バイナリコードを読み込みます．
        /// </summary>
        /// <param name="binary">2003fバイナリデータ</param>
        public void Read(byte[] binary)
        {
            if ((DEFAULT_INITIAL_NX + binary.LongLength) >= (long)uint.MaxValue) {
                throw new ApplicationException("Too Large Programme");
            }

            uint u = DEFAULT_INITIAL_NX;
            foreach (var item in binary)
            {
                this.memory[u++] = item;
            }
        }

        /// <summary>
        /// 指定された名称のファイルパスからバイナリコードを読み込みます．
        /// </summary>
        /// <param name="filepath">2003fバイナリデータを保持するファイルのパス</param>
        public void Read(string filepath)
        {
            Read(File.ReadAllBytes(filepath));
        }

        /// <summary>
        /// 読み込んだバイナリコードを実行します．
        /// </summary>
        public void Run()
        {
            try
            {
                while (this.registers[Register.XX] != DEFAULT_RETURN_ADDRESS)
                {
                    if (this.registers[Register.XX] == TVARLON_KNLOAN_ADDRESS)
                    {
                        debugBuffer.Add(this.memory.GetValue32(this.registers[Register.F5] + 4).ToString());
                        this.registers[Register.XX] = this.memory.GetValue32(this.registers[Register.F5]);

                        continue;
                    }

                    Mnemonic code = (Mnemonic)this.memory.GetValue8(this.registers[Register.XX]);
                    //Console.WriteLine("nx = {0:X08}, code = {1:X02}", this.registers[Register.XX], code);

                    this.registers[Register.XX] += 1;

                    #region コード分岐
                    switch (code)
                    {
                        case Mnemonic.ATA:
                            Ata();
                            break;
                        case Mnemonic.NTA:
                            Nta();
                            break;
                        case Mnemonic.ADA:
                            Ada();
                            break;
                        case Mnemonic.EKC:
                            Ekc();
                            break;
                        case Mnemonic.DTO:
                            Dto();
                            break;
                        case Mnemonic.DRO:
                            Dro();
                            break;
                        case Mnemonic.DTOSNA:
                            Dtosna();
                            break;
                        case Mnemonic.DAL:
                            Dal();
                            break;
                        case Mnemonic.KRZ:
                            Krz();
                            break;
                        case Mnemonic.MALKRZ:
                            Malkrz();
                            break;
                        case Mnemonic.KRZ8I:
                            Krz8i();
                            break;
                        case Mnemonic.KRZ16I:
                            Krz16i();
                            break;
                        case Mnemonic.KRZ8C:
                            Krz8c();
                            break;
                        case Mnemonic.KRZ16C:
                            Krz16c();
                            break;
                        case Mnemonic.LLONYS:
                            Llonys();
                            break;
                        case Mnemonic.XTLONYS:
                            Xtlonys();
                            break;
                        case Mnemonic.XOLONYS:
                            Xolonys();
                            break;
                        case Mnemonic.XYLONYS:
                            Xylonys();
                            break;
                        case Mnemonic.CLO:
                            Clo();
                            break;
                        case Mnemonic.NIV:
                            Niv();
                            break;
                        case Mnemonic.LLO:
                            Llo();
                            break;
                        case Mnemonic.XTLO:
                            Xtlo();
                            break;
                        case Mnemonic.XOLO:
                            Xolo();
                            break;
                        case Mnemonic.XYLO:
                            Xylo();
                            break;
                        case Mnemonic.INJ:
                            Inj();
                            break;
                        case Mnemonic.LAT:
                            Lat();
                            break;
                        case Mnemonic.LATSNA:
                            Latsna();
                            break;
                        default:
                            throw new NotImplementedException($"Not Implemented: {code:X}");
                    }
                    #endregion
                }

                if (ViewRegister)
                {
                    for (int i = 0; i < this.registers.Count; i++)
                    {
                        if (this.registers.ContainsKey((Register)i))
                        {
                            Console.WriteLine("{0} = {1:X08}", (Register)i, this.registers[(Register)i]);
                        }
                    }
                }

                if (ViewMemory)
                {
                    foreach (var item in this.memory.Binaries.OrderBy(x => x.Key))
                    {
                        Console.WriteLine("{0:X08}: {1:X02}", item.Key, item.Value);
                    }
                }

                Console.WriteLine("[{0}]", string.Join(",", this.debugBuffer));
            }
            catch (Exception ex)
            {
                if (ViewRegister)
                {
                    for (int i = 0; i < this.registers.Count; i++)
                    {
                        if (this.registers.ContainsKey((Register)i))
                        {
                            Console.WriteLine("{0} = {1:X08}", (Register)i, this.registers[(Register)i]);
                        }
                    }
                }

                if (ViewMemory)
                {
                    foreach (var item in this.memory.Binaries.OrderBy(x => x.Key))
                    {
                        Console.WriteLine("{0:X08}: {1:X02}", item.Key, item.Value);
                    }
                }

                Console.WriteLine("[{0}]", string.Join(",", this.debugBuffer));

                throw new Exception("Emulator error", ex);
            }
        }

        #region Operators

        /// <summary>
        /// ataの処理を行います．
        /// </summary>
        void Ata()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = GetValue(second) + GetValue(first);
            SetValue(second, val);
        }
        
        /// <summary>
        /// ntaの処理を行います．
        /// </summary>
        void Nta()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = GetValue(second) - GetValue(first);
            SetValue(second, val);
        }

        /// <summary>
        /// adaの処理を行います．
        /// </summary>
        void Ada()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = GetValue(second) & GetValue(first);
            SetValue(second, val);
        }

        /// <summary>
        /// ekcの処理を行います．
        /// </summary>
        void Ekc()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = GetValue(second) | GetValue(first);
            SetValue(second, val);
        }

        /// <summary>
        /// dtoの処理を行います．
        /// </summary>
        void Dto()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            int shift = (int)GetValue(first);
            uint val;

            if (shift >= 32)
            {
                val = 0u;
            }
            else
            {
                val = GetValue(second) >> shift;
            }
            SetValue(second, val);
        }

        /// <summary>
        /// droの処理を行います．
        /// </summary>
        void Dro()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            int shift = (int)GetValue(first);
            uint val;

            if (shift >= 32)
            {
                val = 0u;
            }
            else
            {
                val = GetValue(second) << shift;
            }

            SetValue(second, val);
        }

        /// <summary>
        /// dtosnaの処理を行います．
        /// </summary>
        void Dtosna()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            int shift = (int)GetValue(first);
            uint val;

            if (shift >= 32)
            {
                val = (uint)((int)GetValue(second) >> 31);
            }
            else
            {
                val = (uint)((int)GetValue(second) >> shift);
            }
            SetValue(second, val);
        }

        /// <summary>
        /// dalの処理を行います．
        /// </summary>
        void Dal()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = ~(GetValue(second) ^ GetValue(first));
            SetValue(second, val);
        }

        /// <summary>
        /// krzの処理を行います．
        /// </summary>
        void Krz()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = GetValue(first);
            SetValue(second, val);
        }

        /// <summary>
        /// malkrzの処理を行います．
        /// </summary>
        void Malkrz()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            if (flags)
            {
                uint val = GetValue(first);
                SetValue(second, val);
                flags = false;
            }
        }

        /// <summary>
        /// krz8iの処理を行います．
        /// </summary>
        void Krz8i()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);
            
            int val = GetValue8Top(first);
            SetValue(second, (uint)((val << 24) >> 24));
        }

        /// <summary>
        /// krz16iの処理を行います．
        /// </summary>
        void Krz16i()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            int val = GetValue16Top(first);
            SetValue(second, (uint)((val << 16) >> 16));
        }

        /// <summary>
        /// krz8cの処理を行います．
        /// </summary>
        void Krz8c()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = GetValue(first);
            SetValue8Top(second, val);
        }

        /// <summary>
        /// krz16cの処理を行います．
        /// </summary>
        void Krz16c()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            uint val = GetValue(first);
            SetValue16Top(second, val);
        }

        /// <summary>
        /// fi ~ llonysの処理を行います．
        /// </summary>
        void Llonys()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = GetValue(first) > GetValue(second);
        }

        /// <summary>
        /// fi ~ xtlonysの処理を行います．
        /// </summary>
        void Xtlonys()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = GetValue(first) <= GetValue(second);
        }

        /// <summary>
        /// fi ~ xolonysの処理を行います．
        /// </summary>
        void Xolonys()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = GetValue(first) >= GetValue(second);
        }

        /// <summary>
        /// fi ~ xylonysの処理を行います．
        /// </summary>
        void Xylonys()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = GetValue(first) < GetValue(second);
        }

        /// <summary>
        /// fi ~ cloの処理を行います．
        /// </summary>
        void Clo()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = GetValue(first) == GetValue(second);
        }

        /// <summary>
        /// fi ~ nivの処理を行います．
        /// </summary>
        void Niv()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = GetValue(first) != GetValue(second);
        }

        /// <summary>
        /// fi ~ lloの処理を行います．
        /// </summary>
        void Llo()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = (int)GetValue(first) > (int)GetValue(second);
        }

        /// <summary>
        /// fi ~ xtloの処理を行います．
        /// </summary>
        void Xtlo()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = (int)GetValue(first) <= (int)GetValue(second);
        }

        /// <summary>
        /// fi ~ xoloの処理を行います．
        /// </summary>
        void Xolo()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = (int)GetValue(first) >= (int)GetValue(second);
        }

        /// <summary>
        /// fi ~ xyloの処理を行います．
        /// </summary>
        void Xylo()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);

            this.flags = (int)GetValue(first) < (int)GetValue(second);
        }

        /// <summary>
        /// injの処理を行います．
        /// </summary>
        void Inj()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);
            GetModRm(out ModRm third);

            uint val1 = GetValue(first);
            uint val2 = GetValue(second);
            SetValue(second, val1);
            SetValue(third, val2);
        }

        /// <summary>
        /// latの処理を行います．
        /// </summary>
        void Lat()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);
            GetModRm(out ModRm third);

            ulong val1 = (ulong)GetValue(second) * GetValue(first);
            SetValue(third, (uint)(val1 >> 32));
            SetValue(second, (uint)(val1 & 0xFFFFFFFFU));
        }

        /// <summary>
        /// latsnaの処理を行います．
        /// </summary>
        void Latsna()
        {
            GetModRm(out ModRm first);
            GetModRm(out ModRm second);
            GetModRm(out ModRm third);

            long val = GetValue(second);

            long val1 = ((val >> 31 == 0 ? 0 : 0xFFFFFFFFL << 32) | val) * (int)GetValue(first);
            SetValue(third, (uint)(val1 >> 32));
            SetValue(second, (uint)(val1 & 0xFFFFFFFFU));
        }

        #endregion

        #region ModRm

        /// <summary>
        /// バイナリデータからModRMを取得します．
        /// </summary>
        /// <param name="modrm">ModRM</param>
        void GetModRm(out ModRm modrm)
        {
            modrm = new ModRm
            {
                Code = this.memory.GetValue8(this.registers[Register.XX])
            };

            this.registers[Register.XX] += 1;

            switch (modrm.Mode & 0xF)
            {
                case 0x4:
                    modrm.Imm8 = this.memory.GetValue8(this.registers[Register.XX]);
                    this.registers[Register.XX] += 1;
                    break;
                case 0x5:
                    modrm.Imm16 = this.memory.GetValue16(this.registers[Register.XX]);
                    this.registers[Register.XX] += 2;
                    break;
                case 0x6:
                    modrm.Imm32 = this.memory.GetValue32(this.registers[Register.XX]);
                    this.registers[Register.XX] += 4;
                    break;
                case 0x8:
                case 0x9:
                case 0xA:
                case 0xB:
                case 0xD:
                case 0xF:
                    modrm.DispImm = modrm.Mode & 0x7U;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// ModRMを元に値を取得します．
        /// </summary>
        /// <param name="modrm">ModRM</param>
        /// <returns>取得した値</returns>
        byte GetValue8Top(ModRm modrm)
        {
            switch (modrm.Mode)
            {
                case 0x0:
                    return (byte)(this.registers[(Register)modrm.Reg] >> 24);
                case 0x4:
                    return modrm.Imm8;
                case 0x5:
                    return (byte)(modrm.Imm16 >> 8);
                case 0x6:
                    return (byte)(modrm.Imm32 >> 24);
                case 0x10:
                    return this.memory.GetValue8(this.registers[(Register)modrm.Reg]);
                case 0x14:
                    return this.memory.GetValue8((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp8));
                case 0x15:
                    return this.memory.GetValue8((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp16));
                case 0x16:
                    return this.memory.GetValue8((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp32));
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1D:
                case 0x1F:
                    return this.memory.GetValue8(this.registers[(Register)modrm.Reg] + this.registers[(Register)(modrm.Mode & 0x7U)]);
                default:
                    throw new NotImplementedException($"Not Implemented: Reg: {modrm.Reg:X03}, Mode: {modrm.Mode:X05}");
            }
        }

        /// <summary>
        /// ModRMを元に値を取得します．
        /// </summary>
        /// <param name="modrm">ModRM</param>
        /// <returns>取得した値</returns>
        ushort GetValue16Top(ModRm modrm)
        {
            switch (modrm.Mode)
            {
                case 0x0:
                    return (ushort)(this.registers[(Register)modrm.Reg] >> 16);
                case 0x4:
                    return (ushort)modrm.Imm8;
                case 0x5:
                    return modrm.Imm16;
                case 0x6:
                    return (ushort)(modrm.Imm32 >> 16);
                case 0x10:
                    return this.memory.GetValue16(this.registers[(Register)modrm.Reg]);
                case 0x14:
                    return this.memory.GetValue16((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp8));
                case 0x15:
                    return this.memory.GetValue16((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp16));
                case 0x16:
                    return this.memory.GetValue16((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp32));
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1D:
                case 0x1F:
                    return this.memory.GetValue16(this.registers[(Register)modrm.Reg] + this.registers[(Register)(modrm.Mode & 0x7U)]);
                default:
                    throw new NotImplementedException($"Not Implemented: Reg: {modrm.Reg:X03}, Mode: {modrm.Mode:X05}");
            }
        }

        /// <summary>
        /// ModRMを元に値を取得します．
        /// </summary>
        /// <param name="modrm">ModRM</param>
        /// <returns>取得した値</returns>
        uint GetValue(ModRm modrm)
        {
            switch (modrm.Mode)
            {
                case 0x0:
                    return this.registers[(Register)modrm.Reg];
                case 0x4:
                    return modrm.Imm8;
                case 0x5:
                    return modrm.Imm16;
                case 0x6:
                    return modrm.Imm32;
                case 0x10:
                    return this.memory.GetValue32(this.registers[(Register)modrm.Reg]);
                case 0x14:
                    return this.memory.GetValue32((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp8));
                case 0x15:
                    return this.memory.GetValue32((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp16));
                case 0x16:
                    return this.memory.GetValue32((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp32));
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1D:
                case 0x1F:
                    return this.memory.GetValue32(this.registers[(Register)modrm.Reg] + this.registers[(Register)(modrm.Mode & 0x7U)]);
                default:
                    throw new NotImplementedException($"Not Implemented: Reg: {modrm.Reg:X03}, Mode: {modrm.Mode:X05}");
            }
        }

        /// <summary>
        /// ModRMを元に値を設定します．
        /// </summary>
        /// <param name="modrm">ModRM</param>
        /// <param name="value">設定する値</param>
        void SetValue8Top(ModRm modrm, uint value)
        {
            switch (modrm.Mode)
            {
                case 0x0:
                    this.registers[(Register)modrm.Reg] &= 0x00ffffff;
                    this.registers[(Register)modrm.Reg] |= value << 24;
                    break;
                case 0x10:
                    this.memory.SetValue8(this.registers[(Register)modrm.Reg], (byte)value);
                    break;
                case 0x14:
                    this.memory.SetValue8((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp8), (byte)value);
                    break;
                case 0x15:
                    this.memory.SetValue8((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp16), (byte)value);
                    break;
                case 0x16:
                    this.memory.SetValue8(this.registers[(Register)modrm.Reg] + modrm.Disp32, (byte)value);
                    break;
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1D:
                case 0x1F:
                    this.memory.SetValue8(this.registers[(Register)modrm.Reg]
                        + this.registers[(Register)(modrm.Mode & 0x7U)], (byte)value);
                    break;
                default:
                    throw new NotImplementedException($"Not Implemented: Reg: {modrm.Reg:X01}, Mode: {modrm.Mode:X02}");
            }

            this.flags = false;
        }

        /// <summary>
        /// ModRMを元に値を設定します．
        /// </summary>
        /// <param name="modrm">ModRM</param>
        /// <param name="value">設定する値</param>
        void SetValue16Top(ModRm modrm, uint value)
        {
            switch (modrm.Mode)
            {
                case 0x0:
                    this.registers[(Register)modrm.Reg] &= 0x0000ffff;
                    this.registers[(Register)modrm.Reg] |= value << 16;
                    break;
                case 0x10:
                    this.memory.SetValue16(this.registers[(Register)modrm.Reg], (ushort)value);
                    break;
                case 0x14:
                    this.memory.SetValue16((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp8), (ushort)value);
                    break;
                case 0x15:
                    this.memory.SetValue16((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp16), (ushort)value);
                    break;
                case 0x16:
                    this.memory.SetValue16(this.registers[(Register)modrm.Reg] + modrm.Disp32, (ushort)value);
                    break;
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1D:
                case 0x1F:
                    this.memory.SetValue16(this.registers[(Register)modrm.Reg]
                        + this.registers[(Register)(modrm.Mode & 0x7U)], (byte)value);
                    break;
                default:
                    throw new NotImplementedException($"Not Implemented: Reg: {modrm.Reg:X01}, Mode: {modrm.Mode:X02}");
            }

            this.flags = false;
        }

        /// <summary>
        /// ModRMを元に値を設定します．
        /// </summary>
        /// <param name="modrm">ModRM</param>
        /// <param name="value">設定する値</param>
        void SetValue(ModRm modrm, uint value)
        {
            switch (modrm.Mode)
            {
                case 0x0:
                    this.registers[(Register)modrm.Reg] = value;
                    break;
                case 0x10:
                    this.memory.SetValue32(this.registers[(Register)modrm.Reg], value);
                    break;
                case 0x14:
                    this.memory.SetValue32((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp8), value);
                    break;
                case 0x15:
                    this.memory.SetValue32((uint)(this.registers[(Register)modrm.Reg] + modrm.Disp16), value);
                    break;
                case 0x16:
                    this.memory.SetValue32(this.registers[(Register)modrm.Reg] + modrm.Disp32, value);
                    break;
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1D:
                case 0x1F:
                    this.memory.SetValue32(this.registers[(Register)modrm.Reg]
                        + this.registers[(Register)(modrm.Mode & 0x7U)], value);
                    break;
                default:
                    throw new NotImplementedException($"Not Implemented: Reg: {modrm.Reg:X01}, Mode: {modrm.Mode:X02}");
            }

            this.flags = false;
        }

        #endregion
    }
}
