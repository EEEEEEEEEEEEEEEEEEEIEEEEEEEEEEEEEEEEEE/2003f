using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ferlesyl.Core
{
    class Memory
    {
        /// <summary>
        /// メモリ内容
        /// </summary>
        private readonly IDictionary<uint, byte> memory;

        /// <summary>
        /// メモリが未初期化だった場合に設定されている値を作成
        /// </summary>
        readonly Random random;
        
        /// <summary>
        /// メモリの内容を表すDictionaryを返す．読み込み専用
        /// </summary>
        public IReadOnlyDictionary<uint, byte> Binaries
        {
            get => new ReadOnlyDictionary<uint, byte>(this.memory);
        }
        
        public Memory()
        {
            this.memory = new Dictionary<uint, byte>();
            this.random = new Random();
        }

        public byte this[uint address]
        {
            get
            {
                if(!this.memory.ContainsKey(address))
                {
                    this.memory[address] = (byte)this.random.Next(0, 255);
                }

                return this.memory[address];
            }
            set => this.memory[address] = value;
        }

        public uint GetValue32(uint address)
        {
            uint result = 0;

            for (int i = 0; i < 4; i++)
            {
                uint addr = (uint)(address + i);
                if (!this.memory.ContainsKey(addr))
                {
                    this.memory[addr] = (byte)this.random.Next(0, 255);
                }

                result |= (uint)(this.memory[addr] << ((3 - i) * 8));
            }

            return result;
        }

        public void SetValue32(uint address, uint value)
        {
            this.memory[address] = (byte)(value >> 24);
            this.memory[address + 1] = (byte)(value >> 16);
            this.memory[address + 2] = (byte)(value >> 8);
            this.memory[address + 3] = (byte)value;
        }

        public ushort GetValue16(uint address)
        {
            ushort result = 0;

            for (int i = 0; i < 2; i++)
            {
                uint addr = (uint)(address + i);
                if (!this.memory.ContainsKey(addr))
                {
                    this.memory[addr] = (byte)this.random.Next(0, 255);
                }

                result |= (ushort)(this.memory[addr] << ((3 - i) * 8));
            }

            return result;
        }

        public void SetValue16(uint address, ushort value)
        {
            this.memory[address] = (byte)(value >> 8);
            this.memory[address + 1] = (byte)value;
        }

        public byte GetValue8(uint address)
        {
            if (!this.memory.ContainsKey(address))
            {
                this.memory[address] = (byte)this.random.Next(0, 255);
            }

            return this.memory[address];
        }

        public void SetValue8(uint address, byte value)
        {
            this.memory[address] = value;
        }
    }
}
