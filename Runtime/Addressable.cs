using System.Runtime.CompilerServices;

namespace Aya.UNes
{
    public abstract class Addressable
    {
        public delegate uint ReadDelegate(uint address);

        public delegate void WriteDelegate(uint address, byte val);

        protected readonly Emulator _emulator;
        protected readonly ReadDelegate[] _readMap;
        protected readonly WriteDelegate[] _writeMap;
        protected readonly uint _addressSize;

        protected Addressable(Emulator emulator, uint addressSpace)
        {
            _emulator = emulator;
            _addressSize = addressSpace;
            _readMap = new ReadDelegate[addressSpace + 1];
            _writeMap = new WriteDelegate[addressSpace + 1];
        }

        protected virtual void InitializeMemoryMap()
        {
            _readMap.Fill(address => 0);

            // Some games write to addresses not mapped and expect to continue afterwards
            _writeMap.Fill((address, val) => { });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadByte(uint address)
        {
            address &= _addressSize;
            return _readMap[address](address);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(uint address, uint val)
        {
            address &= _addressSize;
            _writeMap[address](address, (byte)val);
        }

        public void MapReadHandler(uint start, uint end, CPU.ReadDelegate func)
        {
            for (uint i = start; i <= end; i++)
            {
                _readMap[i] = func;
            }
        }

        public void MapWriteHandler(uint start, uint end, CPU.WriteDelegate func)
        {
            for (uint i = start; i <= end; i++)
            {
                _writeMap[i] = func;
            }
        }
    }
}
