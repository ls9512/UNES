namespace Aya.UNes.Mapper
{
    [MapperDef(Id = 9, Description = "Mike Tyson's Punch-Out!!")]
    public class MMC2 : MMC4
    {
        public MMC2(Emulator emulator) : base(emulator)
        {

        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            base.InitializeMemoryMap(cpu);

            cpu.MapReadHandler(0x8000, 0xBFFF, address => _prgROM[_prgBankOffset + (address - 0x8000)]);
            cpu.MapReadHandler(0xA000, 0xFFFF, address => _prgROM[_prgROM.Length - 0x4000 - 0x2000 + (address - 0xA000)]);

            cpu.MapWriteHandler(0xA000, 0xAFFF, (address, val) => _prgBankOffset = (val & 0xF) * 0x2000);
        }

        protected override void GetLatch(uint address, out uint latch, out bool? on)
        {
            base.GetLatch(address, out latch, out on);

            // For MMC2, only 0xFD8 and 0xFE8 trigger the latch,
            // not the whole range like in MMC4
            if (latch == 0 && (address & 0x3) != 0)
            {
                on = null;
            }
        }
    }
}
