using System;
using System.Runtime.CompilerServices;

namespace Aya.UNes
{
    partial class PPU
    {
        private readonly byte[] _oam = new byte[0x100];
        private readonly byte[] _vRam = new byte[0x2000];
        private readonly byte[] _paletteRAM = new byte[0x20];

        private static readonly uint[][] _vRamMirrorLookup =
        {
            new uint[]{0, 0, 1, 1}, // H
            new uint[]{0, 1, 0, 1}, // V
            new uint[]{0, 1, 2, 3}, // All
            new uint[]{0, 0, 0, 0}, // Upper
            new uint[]{1, 1, 1, 1}, // Lower
        };

        private int _lastWrittenRegister;

        public void WriteRegister(uint reg, byte val)
        {
            reg &= 0xF;
            _lastWrittenRegister = val & 0xFF;
            switch (reg)
            {
                case 0x0000:
                    PPUCTRL = val;
                    return;
                case 0x0001:
                    PPUMASK = val;
                    return;
                case 0x0002: return;
                case 0x0003:
                    OAMADDR = val;
                    return;
                case 0x0004:
                    OAMDATA = val;
                    return;
                case 0x005:
                    PPUSCROLL = val;
                    return;
                case 0x0006:
                    PPUADDR = val;
                    return;
                case 0x0007:
                    PPUDATA = val;
                    return;
            }

            throw new NotImplementedException($"{reg:X4} = {val:X2}");
        }

        public byte ReadRegister(uint reg)
        {
            reg &= 0xF;
            switch (reg)
            {
                case 0x0000: return (byte)_lastWrittenRegister;
                case 0x0001: return (byte)_lastWrittenRegister;
                case 0x0002:
                    return (byte)PPUSTATUS;
                case 0x0003:
                    return (byte)OAMADDR;
                case 0x0004:
                    return (byte)OAMDATA;
                case 0x0005: return (byte)_lastWrittenRegister;
                case 0x0006: return (byte)_lastWrittenRegister;
                case 0x0007:
                    return (byte)PPUDATA;
            }
            throw new NotImplementedException(reg.ToString("X2"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVRamMirror(long address)
        {
            long entry;
            var table = Math.DivRem(address - 0x2000, 0x400, out entry);
            return _vRamMirrorLookup[(int)_emulator.Cartridge.MirroringMode][table] * 0x400 + (uint)entry;
        }

        protected override void InitializeMemoryMap()
        {
            base.InitializeMemoryMap();

            MapReadHandler(0x2000, 0x2FFF, address => _vRam[GetVRamMirror(address)]);
            MapReadHandler(0x3000, 0x3EFF, address => _vRam[GetVRamMirror(address - 0x1000)]);
            MapReadHandler(0x3F00, 0x3FFF, address =>
            {
                if (address == 0x3F10 || address == 0x3F14 || address == 0x3F18 || address == 0x3F0C)
                {
                    address -= 0x10;
                }

                return _paletteRAM[(address - 0x3F00) & 0x1F];
            });
            
            MapWriteHandler(0x2000, 0x2FFF, (address, val) => _vRam[GetVRamMirror(address)] = val);
            MapWriteHandler(0x3000, 0x3EFF, (address, val) => _vRam[GetVRamMirror(address - 0x1000)] = val);
            MapWriteHandler(0x3F00, 0x3FFF, (address, val) =>
            {
                if (address == 0x3F10 || address == 0x3F14 || address == 0x3F18 || address == 0x3F0C)
                {
                    address -= 0x10;
                }

                _paletteRAM[(address - 0x3F00) & 0x1F] = val;
            });

            _emulator.Mapper.InitializeMemoryMap(this);
        }

        public void PerformDMA(uint from)
        {
            //Console.WriteLine("OAM DMA");
            from <<= 8;
            for (uint i = 0; i <= 0xFF; i++)
            {
                _oam[F.OAMAddress] = (byte)_emulator.CPU.ReadByte(from);
                from++;
                F.OAMAddress++;
            }

            _emulator.CPU.Cycle += 513 + _emulator.CPU.Cycle % 2;
        }
    }
}
