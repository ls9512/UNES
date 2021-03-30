namespace Aya.UNES.Mapper
{
    // Mapper used strictly for Crazy Climber; logic is slighly different
    [MapperDef(Id = 180, Description = "Crazy Climber")]
    public class Mapper180 : UxROM
    {
        public Mapper180(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            base.InitializeMemoryMap(cpu);

            // $8000-$C000 is fixed to *first* bank
            cpu.MapReadHandler(0x8000, 0xBFFF, address => _prgROM[address - 0x8000]);
            // $C000-$FFFF is switchable, controlled the same as UxROM
            cpu.MapReadHandler(0xC000, 0xFFFF, address => _prgROM[_bankOffset + (address - 0xC000)]);
        }
    }
}
