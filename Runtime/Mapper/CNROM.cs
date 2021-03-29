namespace Aya.UNes.Mapper
{
    [MapperDef(3)]
    public class CNROM : BaseMapper
    {
        protected int _bankOffset;

        public CNROM(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, address => _chrROM[_bankOffset + address]);
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            if (_prgROM.Length == 0x8000)
            {
                cpu.MapReadHandler(0x8000, 0xFFFF, address => _prgROM[address - 0x8000]);
            }
            else
            {
                cpu.MapReadHandler(0x8000, 0xBFFF, address => _prgROM[address - 0x8000]);
                cpu.MapReadHandler(0xC000, 0xFFFF, address => _prgROM[address - 0xC000]);
            }

            cpu.MapWriteHandler(0x8000, 0xFFFF, (address, val) => _bankOffset = (val & 0x3) * 0x2000);
        }
    }
}
