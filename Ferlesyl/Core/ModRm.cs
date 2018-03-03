using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferlesyl.Core
{
    class ModRm
    {
        byte reg;
        byte mode;
        uint disp;

        public byte Code
        {
            get => (byte)((this.reg << 5) | this.mode);
            set {
                this.reg = (byte)(value >> 5);
                this.mode = (byte)(value & 0x17U);
            }
        }

        public byte Reg
        {
            get => this.reg;
            set => this.reg = value;
        }

        public byte Mode
        {
            get => this.mode;
            set => this.mode = value;
        }

        public uint DispImm
        {
            get => this.disp;
            set => this.disp = value;
        }

        public uint Disp32
        {
            get => this.disp;
            set => this.disp = value;
        }

        public short Disp16
        {
            get => (short)this.disp;
            set => this.disp = (uint)(value & 0xFFFFU);
        }

        public sbyte Disp8
        {
            get => (sbyte)this.disp;
            set => this.disp = (uint)(value & 0xFFU);
        }

        public uint Imm32
        {
            get => this.disp;
            set => this.disp = value;
        }

        public ushort Imm16
        {
            get => (ushort)this.disp;
            set => this.disp = value;
        }

        public byte Imm8
        {
            get => (byte)this.disp;
            set => this.disp = value;
        }
    }
}
