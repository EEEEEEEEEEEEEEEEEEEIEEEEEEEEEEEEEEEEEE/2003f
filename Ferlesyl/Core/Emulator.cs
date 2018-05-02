using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LkCommon;

namespace Ferlesyl.Core
{
    class Emulator
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
        /// NXレジスタ
        /// </summary>
        //uint nx;

        /// <summary>
        /// 汎用レジスタ
        /// </summary>
        readonly IDictionary<Register, uint> registers;

        #endregion

        #region Properties

        /// <summary>
        /// メモリの内容を表すDictionaryを返す．読み込み専用
        /// </summary>
        public IReadOnlyDictionary<uint, byte> Memory
        {
            get => this.memory.Binaries;
        }

        #endregion

        public Emulator()
        {
            this.memory = new Memory();
            this.flags = false;

            this.registers = new Dictionary<Register, uint>
            {
                [Register.F0] = 0,
                [Register.F1] = 0,
                [Register.F2] = 0,
                [Register.F3] = 0,
                [Register.F5] = DEFAULT_INITIAL_F5,
                [Register.XX] = DEFAULT_INITIAL_NX,
            };

            this.memory.SetValue32(DEFAULT_INITIAL_F5, DEFAULT_RETURN_ADDRESS);
        }

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

        public void Read(string filepath)
        {
            Read(File.ReadAllBytes(filepath));
        }

        public void Run()
        {
            while (this.registers[Register.XX] != DEFAULT_RETURN_ADDRESS)
            {
                byte code = this.memory.GetValue8(this.registers[Register.XX]);
                Console.WriteLine("nx = {0:X08}, code = {1:X02}", this.registers[Register.XX], code);

                this.registers[Register.XX] += 1;

                switch (code)
                {
                    case 0x00:
                        Ata();
                        break;
                    case 0x01:
                        Nta();
                        break;
                    case 0x02:
                        Ada();
                        break;
                    case 0x03:
                        Ekc();
                        break;
                    case 0x04:
                        Dto();
                        break;
                    case 0x05:
                        Dro();
                        break;
                    case 0x06:
                        Dtosna();
                        break;
                    case 0x07:
                        Dal();
                        break;
                    case 0x08:
                        Krz();
                        break;
                    case 0x09:
                        Malkrz();
                        break;
                    case 0x10:
                        Llonys();
                        break;
                    case 0x11:
                        Xtlonys();
                        break;
                    case 0x12:
                        Xolonys();
                        break;
                    case 0x13:
                        Xylonys();
                        break;
                    case 0x16:
                        Clo();
                        break;
                    case 0x17:
                        Niv();
                        break;
                    case 0x18:
                        Llo();
                        break;
                    case 0x19:
                        Xtlo();
                        break;
                    case 0x1A:
                        Xolo();
                        break;
                    case 0x1B:
                        Xylo();
                        break;
                    case 0x20:
                        Inj();
                        break;
                    case 0x28:
                        Lat();
                        break;
                    case 0x29:
                        Latsna();
                        break;
                    default:
                        throw new NotImplementedException($"Not Implemented: {code:X}");
                }
            }

            for (int i = 0; i < this.registers.Count; i++)
            {
                if (this.registers.ContainsKey((Register)i))
                {
                    Console.WriteLine("{0} = {1:X08}", (Register)i, this.registers[(Register)i]);
                }
            }

            foreach (var item in this.memory.Binaries.OrderBy(x => x.Key))
            {
                Console.WriteLine("{0:X08}: {1:X02}", item.Key, item.Value);
            }
        }

        #region Operators

        void Ata()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = GetValue(second) + GetValue(first);
            SetValue(second, val);
        }

        void Nta()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = GetValue(second) - GetValue(first);
            SetValue(second, val);
        }

        void Ada()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = GetValue(second) & GetValue(first);
            SetValue(second, val);
        }

        void Ekc()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = GetValue(second) | GetValue(first);
            SetValue(second, val);
        }

        void Dto()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = GetValue(second) >> (int)GetValue(first);
            SetValue(second, val);
        }

        void Dro()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = GetValue(second) << (int)GetValue(first);
            SetValue(second, val);
        }

        void Dtosna()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = (uint)((int)GetValue(second) >> (int)GetValue(first));
            SetValue(second, val);
        }

        void Dal()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = ~(GetValue(second) ^ GetValue(first));
            SetValue(second, val);
        }

        void Krz()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            uint val = GetValue(first);
            SetValue(second, val);
        }

        void Malkrz()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            if(flags)
            {
                uint val = GetValue(first);
                SetValue(second, val);
                flags = false;
            }
        }

        void Llonys()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = GetValue(second) > GetValue(first);
        }

        void Xtlonys()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = GetValue(second) <= GetValue(first);
        }

        void Xolonys()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = GetValue(second) >= GetValue(first);
        }

        void Xylonys()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = GetValue(second) < GetValue(first);
        }

        void Clo()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = GetValue(second) == GetValue(first);
        }

        void Niv()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = GetValue(second) != GetValue(first);
        }

        void Llo()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = (int)GetValue(second) > (int)GetValue(first);
        }

        void Xtlo()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = (int)GetValue(second) <= (int)GetValue(first);
        }

        void Xolo()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = (int)GetValue(second) >= (int)GetValue(first);
        }

        void Xylo()
        {
            ModRm first, second;
            ParseModRm(out first);
            ParseModRm(out second);

            this.flags = (int)GetValue(second) < (int)GetValue(first);
        }

        void Inj()
        {
            ModRm first, second, third;
            ParseModRm(out first);
            ParseModRm(out second);
            ParseModRm(out third);

            uint val1 = GetValue(first);
            uint val2 = GetValue(second);
            SetValue(second, val1);
            SetValue(third, val2);
        }

        void Lat()
        {
            ModRm first, second, third;
            ParseModRm(out first);
            ParseModRm(out second);
            ParseModRm(out third);

            ulong val1 = (ulong)GetValue(second) * GetValue(first);
            SetValue(second, (uint)(val1 & 0xFFFFFFFFU));
            SetValue(third, (uint)(val1 >> 32));
        }

        void Latsna()
        {
            ModRm first, second, third;
            ParseModRm(out first);
            ParseModRm(out second);
            ParseModRm(out third);

            long val1 = (long)GetValue(second) * GetValue(first);
            SetValue(second, (uint)(val1 & 0xFFFFFFFFU));
            SetValue(third, (uint)(val1 >> 32));
        }

        #endregion

        #region ModRm

        void ParseModRm(out ModRm modrm)
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
        }

        #endregion
    }
}
