using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LkCommon.Processor
{
    class Memory
    {
        /// <summary>
        /// メモリ内容
        /// </summary>
        private readonly IDictionary<uint, byte> memory;

        /// <summary>
        /// メモリが未初期化だった場合に設定されている値を作成するRandom
        /// </summary>
        readonly Random random;

        /// <summary>
        /// メモリの内容を表す読み込み専用のDictionary
        /// </summary>
        public IReadOnlyDictionary<uint, byte> Binaries
        {
            get => new ReadOnlyDictionary<uint, byte>(this.memory);
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Memory()
        {
            this.memory = new Dictionary<uint, byte>();
            this.random = new Random();
        }

        /// <summary>
        /// 指定されたアドレスの値を返します．
        /// 未使用のアドレスが指定された場合にはランダムな値を返します．
        /// </summary>
        /// <param name="address">アドレス</param>
        /// <returns></returns>
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

        /// <summary>
        /// 指定されたアドレスの値から4byte分だけ取得します．
        /// </summary>
        /// <param name="address">アドレス</param>
        /// <returns>取得した値</returns>
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

        /// <summary>
        /// 指定されたアドレスの値に指定した値を設定します．
        /// </summary>
        /// <param name="address">アドレス</param>
        /// <param name="value">設定する値</param>
        public void SetValue32(uint address, uint value)
        {
            this.memory[address] = (byte)(value >> 24);
            this.memory[address + 1] = (byte)(value >> 16);
            this.memory[address + 2] = (byte)(value >> 8);
            this.memory[address + 3] = (byte)value;
        }

        /// <summary>
        /// 指定されたアドレスの値から2byte分だけ取得します．
        /// </summary>
        /// <param name="address">アドレス</param>
        /// <returns>取得した値</returns>
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

                result |= (ushort)(this.memory[addr] << ((1 - i) * 8));
            }

            return result;
        }

        /// <summary>
        /// 指定されたアドレスの値に指定した値を設定します．
        /// </summary>
        /// <param name="address">アドレス</param>
        /// <param name="value">設定する値</param>
        public void SetValue16(uint address, ushort value)
        {
            this.memory[address] = (byte)(value >> 8);
            this.memory[address + 1] = (byte)value;
        }

        /// <summary>
        /// 指定されたアドレスの値から1byte分だけ取得します．
        /// </summary>
        /// <param name="address">アドレス</param>
        /// <returns>取得した値</returns>
        public byte GetValue8(uint address)
        {
            if (!this.memory.ContainsKey(address))
            {
                this.memory[address] = (byte)this.random.Next(0, 255);
            }

            return this.memory[address];
        }

        /// <summary>
        /// 指定されたアドレスの値に指定した値を設定します．
        /// </summary>
        /// <param name="address">アドレス</param>
        /// <param name="value">設定する値</param>
        public void SetValue8(uint address, byte value)
        {
            this.memory[address] = value;
        }
    }
}
