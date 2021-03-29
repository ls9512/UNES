namespace Aya.UNes.Mapper
{
    [MapperDef(0)]
    public class NROM : BaseMapper
    {
        private readonly byte[] _addressSpace = new byte[0x2000 + 0x8000]; // Space for $2000 VRAM + $8000 PRG

        public NROM(Emulator emulator) : base(emulator)
        {
            for (var i = 0; i < 0x8000; i++)
            {
                var offset = _emulator.Cartridge.PRGROMSize == 0x4000 ? i & 0xBFFF : i;
                _addressSpace[0x2000 + i] = _prgROM[offset];
            }
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0xFFFF, address => _addressSpace[address - 0x6000]);
            cpu.MapWriteHandler(0x6000, 0x7FFF, (address, val) => _addressSpace[address - 0x6000] = val);
        }
    }
}
