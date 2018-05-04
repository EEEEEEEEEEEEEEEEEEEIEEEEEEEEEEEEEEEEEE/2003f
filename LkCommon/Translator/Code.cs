using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LkCommon.Translator
{
    /// <summary>
    /// DynamicAssemblerの中間表現を保持するためのクラスです．
    /// </summary>
    class Code
    {
        public Mnemonic Mnemonic { get; set; }
        public IList<CodeOperand> Operands { get; set; }
        
        public Code()
        {
            Operands = new List<CodeOperand>();
        }

        public int CodeLength
        {
            get
            {
                int count = 1;

                foreach (var opd in Operands)
                {
                    count += 1 + GetDispLength(opd.ModRm);
                }

                return count;
            }
        }

        private int GetDispLength(ModRm modrm)
        {
            if((modrm.Mode & 0x18U) == 0x18U || modrm.Mode == 0U)
            {
                return 0;
            }

            switch (modrm.Mode & 7U)
            {
                case 4:
                    return 1;
                case 5:
                    return 2;
                case 6:
                    return 4;
                default:
                    return 0;
            }
        }

    }

    class CodeOperand
    {
        public ModRm ModRm { get; set; }
        public string Label { get; set; }

        public bool IsLabel
        {
            get => !string.IsNullOrEmpty(Label);
        }
    }
}
