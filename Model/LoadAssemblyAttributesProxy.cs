using System;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core.Components;

namespace PackageManager.Model
{
    public class LoadAssemblyAttributesProxy : MarshalByRefObject
    {
        public LoadAssemblyAttributesProxy() { }
        public Attribute[] LoadAssemblyAttributes(string assFile)
        {
            Assembly asm = Assembly.LoadFrom(assFile);
            Attribute[] plugInAttribute = asm.GetCustomAttributes(typeof(Attribute), false) as Attribute[];
            return plugInAttribute;
        }

        public AssemblyName[] GetDependencies(string assFile)
        {
            Assembly asm = Assembly.LoadFrom(assFile);
            AssemblyName[] refAssys = asm.GetReferencedAssemblies();
            return refAssys;
        }


        public Guid GetPluginGuidFromAssembly(string assFile)
        {
            Assembly asm = Assembly.LoadFrom(assFile);
            PlugInGuidAttribute[] plugInAttribute = asm.GetCustomAttributes(typeof(PlugInGuidAttribute), false) as PlugInGuidAttribute[];
            if (plugInAttribute.Count() > 0)
            {
                return plugInAttribute[0].Guid;
            }
            return Guid.Empty;
        }
    }
}
