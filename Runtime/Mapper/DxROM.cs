namespace Aya.UNES.Mapper
{
    [MapperDef(206)]
    public class DxROM : MMC3
    {
        public DxROM(Emulator emulator) : base(emulator)
        {
            _prgBankingMode = PRGBankingMode.SwitchFix;
            _chrBankingMode = CHRBankingMode.TwoFour;
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x8000, 0xFFFF, address => _prgROM[_prgBankOffsets[(address - 0x8000) / 0x2000] + address % 0x2000]);

            cpu.MapWriteHandler(0x8000, 0x9FFF, (address, val) =>
            {
                if ((address & 0x1) == 0)
                {
                    _currentBank = val & 0x7u;
                }
                else
                {
                    if (_currentBank <= 1) val &= 0x1F;
                    else if (_currentBank <= 5) val &= 0x3F;
                    else val &= 0xF;
                  
                    _banks[_currentBank] = val;
                    UpdateOffsets();
                }
            });
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, address => _chrROM[_chrBankOffsets[address / 0x400] + address % 0x400]);
        }
    }
}
