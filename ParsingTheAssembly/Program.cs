using System;
using System.Collections.Immutable;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ParsingTheAssembly
{
    class Program
    {
        private static OpCode opCodeSub = OpCodes.Sub;
        private static OpCode opCodeCall = OpCodes.Call;
        private static string methodRefSub = "op_Subtraction";

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Less or more then 2 args");
                return;
            }

            var dll = args[0];
            var output = args[1];

            var module = ModuleDefinition.ReadModule(dll);


            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    var processor = method.Body.GetILProcessor();
                    var instruction = method.Body.Instructions[0];

                    while (instruction != null)
                    {
                        if (IsOpCodesAdd(instruction))
                        {
                            var newInstruction = GetNewInstruction(processor, instruction);

                            processor.Replace(instruction, newInstruction);

                            instruction = newInstruction;
                        }
                        else if (IsOpCodesAddDecimal(instruction))
                        {
                            var newInstruction = GetNewInstrutionDecimal(processor, instruction);

                            processor.Replace(instruction, newInstruction);

                            instruction = newInstruction;
                        }

                        instruction = instruction.Next;
                    }
                }
            }



            module.Write(output);
        }

        
        private static bool IsOpCodesAdd (Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Add)
                return true;

            return false;
        }

        private static bool IsOpCodesAddDecimal (Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference { Name: "op_Addition" })
                return true;

            return false;
        }

        private static Instruction GetNewInstruction(ILProcessor processor, Instruction instruction)
        {
            var newInstruction = processor.Create(opCodeSub);

            return CopyInstruction(instruction, newInstruction);
        }

        private static Instruction GetNewInstrutionDecimal(ILProcessor processor, Instruction instruction)
        {
            var methodReference = instruction.Operand as MethodReference;

            var newMethodReference = new MethodReference(methodRefSub, methodReference.ReturnType, methodReference.DeclaringType);

            foreach (var param in methodReference.Parameters)
            {
                newMethodReference.Parameters.Add(param);
            }


            var newInstruction = processor.Create(opCodeCall, newMethodReference);

            return CopyInstruction(instruction, newInstruction); ;
        }

        private static Instruction CopyInstruction(Instruction oldInstruction, Instruction newInstruction)
        {
            newInstruction.Offset = oldInstruction.Offset;
            newInstruction.Next = oldInstruction.Next;
            newInstruction.Previous = oldInstruction.Previous;

            return newInstruction;
        }
    }
}
