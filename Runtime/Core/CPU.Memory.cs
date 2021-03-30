using System;
using System.Linq;
using System.Runtime.CompilerServices;
using static Aya.UNES.CPU.AddressingMode;

namespace Aya.UNES
{
    sealed partial class CPU
    {
        public enum AddressingMode
        {
            None,
            Direct,
            Immediate,
            ZeroPage,
            Absolute,
            ZeroPageX,
            ZeroPageY,
            AbsoluteX,
            AbsoluteY,
            IndirectX,
            IndirectY
        }

        private uint? _currentMemoryAddress;
        private uint _rmwValue;

        private void ResetInstructionAddressingMode() => _currentMemoryAddress = null;

        private uint _Address()
        {
            var def = _opCodeDefs[_currentInstruction];
            switch (def.Mode)
            {
                case Immediate:
                    return PC++;
                case ZeroPage:
                    return NextByte();
                case Absolute:
                    return NextWord();
                case ZeroPageX:
                    return (NextByte() + X) & 0xFF;
                case ZeroPageY:
                    return (NextByte() + Y) & 0xFF;
                case AbsoluteX:
                    var address = NextWord();
                    if (def.PageBoundary && (address & 0xFF00) != ((address + X) & 0xFF00))
                    {
                        Cycle += 1;
                    }

                    return address + X;
                case AbsoluteY:
                    address = NextWord();
                    if (def.PageBoundary && (address & 0xFF00) != ((address + Y) & 0xFF00))
                    {
                        Cycle += 1;
                    }

                    return address + Y;
                case IndirectX:
                    var off = (NextByte() + X) & 0xFF;
                    return ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
                case IndirectY:
                    off = NextByte() & 0xFF;
                    address = ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
                    if (def.PageBoundary && (address & 0xFF00) != ((address + Y) & 0xFF00))
                    {
                        Cycle += 1;
                    }

                    return (address + Y) & 0xFFFF;
            }
            throw new NotImplementedException();
        }

        public uint AddressRead()
        {
            if (_opCodeDefs[_currentInstruction].Mode == Direct)
            {
                return _rmwValue = A;
            }

            if (_currentMemoryAddress == null)
            {
                _currentMemoryAddress = _Address();
            }

            return _rmwValue = ReadByte((uint)_currentMemoryAddress) & 0xFF;
        }

        public void AddressWrite(uint val)
        {
            if (_opCodeDefs[_currentInstruction].Mode == Direct)
            {
                A = val;
            }
            else
            {
                if (_currentMemoryAddress == null)
                {
                    _currentMemoryAddress = _Address();
                }

                if (_opCodeDefs[_currentInstruction].RMW)
                {
                    WriteByte((uint)_currentMemoryAddress, _rmwValue);
                }

                WriteByte((uint)_currentMemoryAddress, val);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint ReadWord(uint address) => ReadByte(address) | (ReadByte(address + 1) << 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint NextByte() => ReadByte(PC++);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint NextWord() => NextByte() | (NextByte() << 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private sbyte NextSByte() => (sbyte)NextByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push(uint what)
        {
            WriteByte(0x100 + SP, what);
            SP--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Pop()
        {
            SP++;
            return ReadByte(0x100 + SP);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushWord(uint what)
        {
            Push(what >> 8);
            Push(what & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint PopWord() => Pop() | (Pop() << 8);

        protected override void InitializeMemoryMap()
        {
            base.InitializeMemoryMap();

            MapReadHandler(0x0000, 0x1FFF, address => _ram[address & 0x07FF]);
            MapReadHandler(0x2000, 0x3FFF, address => _emulator.PPU.ReadRegister((address & 0x7) - 0x2000));
            MapReadHandler(0x4000, 0x4017, ReadIORegister);

            MapWriteHandler(0x0000, 0x1FFF, (address, val) => _ram[address & 0x07FF] = val);
            MapWriteHandler(0x2000, 0x3FFF, (address, val) => _emulator.PPU.WriteRegister((address & 0x7) - 0x2000, val));
            MapWriteHandler(0x4000, 0x401F, WriteIoRegister);

            _emulator.Mapper.InitializeMemoryMap(this);
        }
    }
}
