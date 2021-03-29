using static Aya.UNes.Cartridge.VRAMMirroringMode;

namespace Aya.UNes.Mapper
{
    [MapperDef(7)]
    public class AxROM : BaseMapper
    {
        protected int _bankOffset;
        private readonly Cartridge.VRAMMirroringMode[] _mirroringModes = { Lower, Upper };

        public AxROM(Emulator emulator) : base(emulator)
        {
            _emulator.Cartridge.MirroringMode = _mirroringModes[0];
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x8000, 0xFFFF, address => _prgROM[_bankOffset + (address - 0x8000)]);           
            cpu.MapWriteHandler(0x8000, 0xFFFF, (address, val) =>
            {
                _bankOffset = (val & 0x7) * 0x8000;
                _emulator.Cartridge.MirroringMode = _mirroringModes[(val >> 4) & 0x1];
            });
        }
    }
}
