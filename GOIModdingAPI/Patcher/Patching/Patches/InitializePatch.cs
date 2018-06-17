using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace Patcher.Patching.Patches
{
    public class InitializePatch : IAssemblyPatch
    {
        public bool IsPatched(ModuleDefinition gameModule, ModuleDefinition apiModule)
        {
            var method = GetStartMethodDefinition(gameModule);
            var il = method.Body.GetILProcessor();

            foreach (var instruction in il.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Call)
                {
                    var methodReference = (MethodReference) instruction.Operand;

                    if (methodReference.DeclaringType.FullName == "ModAPI.Program")
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public bool Patch(ModuleDefinition gameModule, ModuleDefinition apiModule)
        {
            var method = GetStartMethodDefinition(gameModule);
            var il = method.Body.GetILProcessor();

            MethodReference initMethod = gameModule.ImportReference(apiModule.Types.Single(type => type.FullName == "ModAPI.Program").Methods.Single(method2 => method2.Name == "Main"));

            var call = il.Create(OpCodes.Call, initMethod);
            il.InsertBefore(il.Body.Instructions.Last(), call);
            
            return true;
        }

        private MethodDefinition GetStartMethodDefinition(ModuleDefinition module)
        {
            TypeDefinition loaderType = module.Types.Single(type => type.Name == "Loader");
            MethodDefinition startMethod = loaderType.Methods.Single(method => method.Name == "Start");
            
            return startMethod;
        }
    }
}