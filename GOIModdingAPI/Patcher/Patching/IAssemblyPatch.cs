using Mono.Cecil;

namespace Patcher.Patching
{
    public interface IAssemblyPatch
    {
        bool IsPatched(ModuleDefinition gameModule, ModuleDefinition apiModule);
        bool Patch(ModuleDefinition gameModule, ModuleDefinition apiModule);
    }
}