using System;
using System.IO;

namespace Aya.UNes.Mapper
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MapperDef : Attribute
    {
        public int Id;
        public string Name;
        public string Description;

        public MapperDef()
        {

        }

        public MapperDef(int id)
        {
            Id = id;
        }
    }

    public abstract class BaseMapper
    {
        protected readonly Emulator _emulator;
        protected readonly byte[] _prgROM;
        protected readonly byte[] _prgRAM = new byte[0x2000];
        protected readonly byte[] _chrROM;
        protected readonly uint _lastBankOffset;

        protected BaseMapper(Emulator emulator)
        {
            _emulator = emulator;
            var cart = emulator.Cartridge;
            _prgROM = cart.PRGROM;
            _chrROM = cart.CHRROM;
            _lastBankOffset = (uint) _prgROM.Length - 0x4000;
        }

        public virtual void InitializeMemoryMap(CPU cpu)
        {

        }

        public virtual void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, address => _chrROM[address]);
            ppu.MapWriteHandler(0x0000, 0x1FFF, (address, val) => _chrROM[address] = val);
        }

        public virtual void ProcessCycle(int scanLine, int cycle)
        {

        }

        public virtual void Save(Stream os)
        {
            os.Write(_prgRAM, 0, _prgRAM.Length);
        }

        public virtual void Load(Stream os)
        {
            using (var binaryReader = new BinaryReader(os))
            {
                var ram = binaryReader.ReadBytes((int)os.Length);
                Array.Copy(ram, _prgRAM, ram.Length);
            }
        }
    }
}