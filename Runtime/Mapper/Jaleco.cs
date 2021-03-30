namespace Aya.UNES.Mapper
{
    [MapperDef(140)]
    public class Jaleco : BaseMapper
    {
        protected int _prgBankOffset;
        protected int _chrBankOffset;

        public Jaleco(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, address => _chrROM[_chrBankOffset + address]);
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x8000, 0xFFFF, address => _prgROM[_prgBankOffset + (address - 0x8000)]);

            cpu.MapWriteHandler(0x6000, 0x7FFF, (address, val) =>
            {
                _prgBankOffset = ((val >> 4) & 0x3) * 0x8000;
                _chrBankOffset = (val & 0x3) * 0x2000;
            });
        }
    }
}
