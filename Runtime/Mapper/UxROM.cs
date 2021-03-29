namespace Aya.UNes.Mapper
{
    [MapperDef(2)]
    public class UxROM : BaseMapper
    {
        protected int _bankOffset;

        public UxROM(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0x7FFF, address => _prgRAM[address - 0x6000]);
            cpu.MapReadHandler(0x8000, 0xBFFF, address => _prgROM[_bankOffset + (address - 0x8000)]);
            cpu.MapReadHandler(0xC000, 0xFFFF, address => _prgROM[_prgROM.Length - 0x4000 + (address - 0xC000)]);

            cpu.MapWriteHandler(0x6000, 0x7FFF, (address, val) => _prgRAM[address - 0x6000] = val);
            cpu.MapWriteHandler(0x8000, 0xFFFF, (address, val) => _bankOffset = (val & 0xF) * 0x4000);
        }
    }
}
