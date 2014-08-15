using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleArc
{
    public sealed class Processor
    {
        private byte[] memory;
        private int instructionPointer;
        
        public Processor(int memorySize, string programPath)
        {
            this.memory = new byte[memorySize];
            
            using (FileStream stream = File.OpenRead(programPath))
            {
                if (this.memory.Length < stream.Length)
                {
                    Console.WriteLine("Invalid memory size for program.");
                    Console.ReadKey();
                    throw new ArgumentException();
                }

                while (stream.Position < stream.Length)
                {
                    this.memory[stream.Position] = (byte)stream.ReadByte();
                }
            }

            this.instructionPointer = 0;
        }

        public void ExecuteNext()
        {
            // Instructions:
            // jmp {x}: Set the instruction pointer to {x}, which is a 4-byte int
            // cjmp {x} {y}: Set the instruction pointer to {x} if the value at {y} is non-zero
            // mov {x} {y}: Set the value at {y} to the value at {x}.

            OpCode instruction = (OpCode)this.memory[this.instructionPointer];
            this.instructionPointer++;

            switch (instruction)
            {
                case OpCode.OpJump:
                    int address = this.ReadInt();
                    if (address >= this.memory.Length)
                    {
                        Console.WriteLine("Jump out of range.");
                        throw new Exception();
                    }
                    this.instructionPointer = address;
                    break;
                case OpCode.OpCJump:
                    int jumpAddress = this.ReadInt();
                    bool condition = (this.memory[this.ReadInt()] != 0);
                    
                    if (condition)
                    {
                        this.instructionPointer = jumpAddress;
                    }
                    break;
                case OpCode.OpMove:
                    int sourceAddress = this.ReadInt();
                    int destAddress = this.ReadInt();
                    this.memory[destAddress] = this.memory[sourceAddress];
                    break;
                default:
                    break;
            }
        }

        private int ReadInt()
        {
            int i = this.instructionPointer;
            int result = (this.memory[i] << 24) + (this.memory[i + 1] << 16) + (this.memory[i + 2] << 8) + this.memory[i + 3];
            this.instructionPointer += 4;
            return result;
        }
    }

    public enum OpCode
    {
        OpJump = 0,
        OpCJump = 1,
        OpMove = 2
    }
}
