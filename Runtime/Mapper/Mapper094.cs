namespace Aya.UNES.Mapper
{
    [MapperDef(Id = 94, Description = "Senjou no Ookami")]
    public class Mapper094 : UxROM
    {
        public Mapper094(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            base.InitializeMemoryMap(cpu);
    
            cpu.MapWriteHandler(0x8000, 0xFFFF, (address, val) => _bankOffset = (val & 0x1C) << 12);
        }
    }
}
