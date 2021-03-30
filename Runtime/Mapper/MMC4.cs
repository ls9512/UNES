using static Aya.UNES.Cartridge.VRAMMirroringMode;

namespace Aya.UNES.Mapper
{
    [MapperDef(10)]
    public class MMC4 : BaseMapper
    {
        protected readonly Cartridge.VRAMMirroringMode[] _mirroringModes = { Vertical, Horizontal };

        protected int _prgBankOffset;
        protected int[,] _chrBankOffsets = new int[2, 2];
        protected bool[] _latches = new bool[2];

        public MMC4(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0x7FFF, address => _prgRAM[address - 0x6000]);
            cpu.MapReadHandler(0x8000, 0xBFFF, address => _prgROM[_prgBankOffset + (address - 0x8000)]);
            cpu.MapReadHandler(0xC000, 0xFFFF, address => _prgROM[_prgROM.Length - 0x4000 + (address - 0xC000)]);

            cpu.MapWriteHandler(0x6000, 0x7FFF, (address, val) => _prgRAM[address - 0x6000] = val);
            cpu.MapWriteHandler(0xA000, 0xAFFF, (address, val) => _prgBankOffset = (val & 0xF) * 0x4000);
            cpu.MapWriteHandler(0xB000, 0xEFFF, (address, val) =>
            {
                var bank = (address - 0xB000) / 0x2000;
                var latch = ((address & 0x1FFF) == 0).AsByte();
                _chrBankOffsets[bank, latch] = (val & 0x1F) * 0x1000;
            });

            cpu.MapWriteHandler(0xF000, 0xFFFF, (address, val) => _emulator.Cartridge.MirroringMode = _mirroringModes[val & 0x1]);
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, address =>
            {
                var bank = address / 0x1000;
                var ret = _chrROM[_chrBankOffsets[bank, _latches[bank].AsByte()] + address % 0x1000];
                if ((address & 0x08) > 0)
                {
                    GetLatch(address, out uint latch, out bool? on);

                    if (on != null)
                    {
                        _latches[latch] = (bool)on;
                    }
                }

                return ret;
            });
        }

        protected virtual void GetLatch(uint address, out uint latch, out bool? on)
        {
            latch = (address >> 12) & 0x1;
            on = null;

            address = (address >> 4) & 0xFF;

            if (address == 0xFE)
            {
                on = true;
            }
            else if (address == 0xFD)
            {
                on = false;
            }
        }
    }
}
