using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aya.UNES.Controller;
using Aya.UNES.Mapper;

namespace Aya.UNES
{
    public class Emulator
    {
        private static readonly Dictionary<int, KeyValuePair<Type, MapperDef>> Mappers = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                                                 let def = (MapperDef)type.GetCustomAttributes(typeof(MapperDef), true).FirstOrDefault()
                                                                 where def != null
                                                                 select new { def, type }).ToDictionary(a => a.def.Id, a => new KeyValuePair<Type, MapperDef>(a.type, a.def));

        public IController Controller;

        public readonly CPU CPU;

        public readonly PPU PPU;

        public readonly BaseMapper Mapper;

        public readonly Cartridge Cartridge;

        public Emulator(byte[] bytes, IController controller)
        {
            Cartridge = new Cartridge(bytes);
            if (!Mappers.ContainsKey(Cartridge.MapperNumber))
            {
                throw new NotImplementedException($"unsupported mapper {Cartridge.MapperNumber}");
            }

            Mapper = (BaseMapper)Activator.CreateInstance(Mappers[Cartridge.MapperNumber].Key, this);
            CPU = new CPU(this);
            PPU = new PPU(this);
            Controller = controller;

            // Load();
        }
    }
}
