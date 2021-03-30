using System;
using System.IO;

namespace Aya.UNES.Mapper
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

        public virtual byte[] GetSaveData()
        {
            return _prgRAM;
        }

        public virtual void LoadSaveData(byte[] saveData)
        {
            Array.Copy(saveData, _prgRAM, saveData.Length);
        }
    }
}