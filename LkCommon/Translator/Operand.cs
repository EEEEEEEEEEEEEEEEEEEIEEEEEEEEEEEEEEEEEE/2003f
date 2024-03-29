﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LkCommon.Translator
{
    public class Operand
    {
        internal Register? Reg { get; }
        internal Register? SecondReg { get; }
        internal uint? Disp { get; }
        internal string Label { get; }
        internal bool IsAddress { get; }

        Operand(Register? reg, Register? second, uint? val, string label, bool address = false)
        {
            this.Reg = reg;
            this.SecondReg = second;
            this.Disp = val;
            this.IsAddress = address;
            this.Label = label;
        }

        internal bool IsImm
        {
            get => Reg == null && SecondReg == null && Disp != null
                && !IsAddress && string.IsNullOrEmpty(Label);
        }

        internal bool IsReg
        {
            get => Reg != null && SecondReg == null && Disp == null
                && !IsAddress && string.IsNullOrEmpty(Label);
        }

        internal bool IsLabel
        {
            get => !string.IsNullOrEmpty(Label);
        }

        internal bool HasSecondReg
        {
            get => Reg != null && SecondReg != null
                && Disp == null && string.IsNullOrEmpty(Label);
        }

        internal Operand(uint val) : this(null, null, val, null, false) { }
        internal Operand(Register reg, bool address = false) : this(reg, null, null, null, address) { }
        internal Operand(Register reg, uint val, bool address = false) : this(reg, null, val, null, address) { }
        internal Operand(Register reg, Register second, bool address = false) : this(reg, second, null, null, address) { }
        internal Operand(string label, bool address = true, uint val = 0) : this(null, null, val, label, address) { }

        internal Operand ToAddressing()
        {
            if (this.IsAddress)
            {
                throw new InvalidOperationException();
            }

            if(this.IsImm || this.IsLabel)
            {
                throw new InvalidOperationException();
            }

            return new Operand(this.Reg, this.SecondReg, this.Disp, null, true);
        }

        public static Operand operator+(Operand left, Operand right)
        {
            if (left.IsImm && right.IsImm)
            {
                throw new ArgumentException();
            }
            if (left.IsAddress || right.IsAddress)
            {
                throw new ArgumentException();
            }
            if (left.IsLabel || right.IsLabel)
            {
                throw new ArgumentException();
            }

            if ((left.Reg.HasValue && (left.Disp.HasValue || left.SecondReg.HasValue))
                || (right.Reg.HasValue && (right.Disp.HasValue || right.SecondReg.HasValue)))
            {
                throw new ArgumentException();
            }
            else
            {
                return new Operand(left.Reg, right.Reg, null, null);
            }
        }

        public static Operand operator+(Operand left, uint disp) {
            return new Operand(left.Reg.Value, disp);
        }

        public static Operand operator+(uint disp, Operand right)
        {
            return new Operand(right.Reg.Value, disp);
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            if (Reg.HasValue)
            {
                buffer.Append(Reg.Value);

                if (SecondReg.HasValue)
                {
                    buffer.Append("+").Append(SecondReg);
                }
                else if (Disp.HasValue)
                {
                    buffer.Append("+").Append(Disp);
                }
            }
            else if (IsLabel)
            {
                buffer.Append(Label);
            }
            else if (Disp.HasValue)
            {
                buffer.Append(Disp.Value);
            }

            if (IsAddress && !IsLabel)
            {
                buffer.Append("@");
            }

            return buffer.ToString();
        }

    }
}
