using System;
using System.Linq;
using System.Reflection;

namespace Aya.UNES
{
    sealed partial class CPU : Addressable
    {
        private readonly byte[] _ram = new byte[0x800];
        public int Cycle;
        private uint _currentInstruction;

        public delegate void OpCode();

        private readonly OpCode[] _opCodes = new OpCode[256];
        private readonly string[] _opCodeNames = new string[256];
        private readonly OpCodeDef[] _opCodeDefs = new OpCodeDef[256];

        public CPU(Emulator emulator) : base(emulator, 0xFFFF)
        {
            InitializeOpCodes();
            InitializeMemoryMap();
            Initialize();
        }

        private void InitializeOpCodes()
        {
            var opCodeBindings = from opCode in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                 let defs = opCode.GetCustomAttributes(typeof(OpCodeDef), false)
                                 where defs.Length > 0
                                 select new
                                 {
                                     binding = (OpCode)Delegate.CreateDelegate(typeof(OpCode), this, opCode.Name),
                                     name = opCode.Name,
                                     defs = (from d in defs select (OpCodeDef)d)
                                 };

            foreach (var opCode in opCodeBindings)
            {
                foreach (var def in opCode.defs)
                {
                    _opCodes[def.OpCode] = opCode.binding;
                    _opCodeNames[def.OpCode] = opCode.name;
                    _opCodeDefs[def.OpCode] = def;
                }
            }
        }

        public void Execute()
        {
            for (var i = 0; i < 5000; i++)
            {
                ExecuteSingleInstruction();
            }

            uint w;
            ushort x = 6000;
            string z = "";
            while ((w = ReadByte(x)) != '\0')
            {
                z += (char)w;
            }
        }
    }
}
