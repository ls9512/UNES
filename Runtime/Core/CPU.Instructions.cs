﻿using System;
using static Aya.UNES.CPU.AddressingMode;

namespace Aya.UNES
{
    public sealed partial class CPU
    {
        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        public class OpCodeDef : Attribute
        {
            public int OpCode;
            public int Cycles = 1;
            public bool PageBoundary;
            public bool RMW;
            public AddressingMode Mode = None;
        }

        [OpCodeDef(OpCode = 0x20, Cycles = 6)]
        private void JSR()
        {
            PushWord(PC + 1);
            PC = NextWord();
        }

        [OpCodeDef(OpCode = 0x40, Cycles = 6)]
        private void RTI()
        {
            // TODO: this dummy fetch should happen for all single-byte instructions
            NextByte();
            P = Pop();
            PC = PopWord();
        }

        [OpCodeDef(OpCode = 0x60, Cycles = 6)]
        private void RTS()
        {
            NextByte();
            PC = PopWord() + 1;
        }

        [OpCodeDef(OpCode = 0xC8, Cycles = 2)]
        private void INY() => Y++;

        [OpCodeDef(OpCode = 0x88, Cycles = 2)]
        private void DEY() => Y--;

        [OpCodeDef(OpCode = 0xE8, Cycles = 2)]
        private void INX() => X++;

        [OpCodeDef(OpCode = 0xCA, Cycles = 2, RMW = true)]
        private void DEX() => X--;

        [OpCodeDef(OpCode = 0xA8, Cycles = 2)]
        private void TAY() => Y = A;

        [OpCodeDef(OpCode = 0x98, Cycles = 2)]
        private void TYA() => A = Y;

        [OpCodeDef(OpCode = 0xAA, Cycles = 2, RMW = true)]
        private void TAX() => X = A;

        [OpCodeDef(OpCode = 0x8A, Cycles = 2, RMW = true)]
        private void TXA() => A = X;

        [OpCodeDef(OpCode = 0xBA, Cycles = 2)]
        private void TSX() => X = SP;

        [OpCodeDef(OpCode = 0x9A, Cycles = 2, RMW = true)]
        private void TXS() => SP = X;

        [OpCodeDef(OpCode = 0x08, Cycles = 3)]
        private void PHP() => Push(P | BreakSourceBit);

        [OpCodeDef(OpCode = 0x28, Cycles = 4)]
        private void PLP() => P = (uint)(Pop() & ~BreakSourceBit);

        [OpCodeDef(OpCode = 0x68, Cycles = 4)]
        private void PLA() => A = Pop();

        [OpCodeDef(OpCode = 0x48, Cycles = 3)]
        private void PHA() => Push(A);

        [OpCodeDef(OpCode = 0x24, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x2C, Mode = Absolute, Cycles = 4)]
        private void BIT()
        {
            uint val = AddressRead();
            F.Overflow = (val & 0x40) > 0;
            F.Zero = (val & A) == 0;
            F.Negative = (val & 0x80) > 0;
        }

        private void Branch(bool cond)
        {
            uint nPC = (uint)(PC + NextSByte() + 1);
            if (cond)
            {
                PC = nPC;
                Cycle++;
            }
        }

        [OpCodeDef(OpCode = 0x4C, Cycles = 3)]
        [OpCodeDef(OpCode = 0x6C, Cycles = 5)]
        private void JMP()
        {
            if (_currentInstruction == 0x4C)
            {
                PC = NextWord();
            }
            else if (_currentInstruction == 0x6C)
            {
                uint off = NextWord();
                // AN INDIRECT JUMP MUST NEVER USE A VECTOR BEGINNING ON THE LAST BYTE OF A PAGE
                //
                // If address $3000 contains $40, $30FF contains $80, and $3100 contains $50, 
                // the result of JMP ($30FF) will be a transfer of control to $4080 rather than
                // $5080 as you intended i.e. the 6502 took the low byte of the address from
                // $30FF and the high byte from $3000.
                //
                // http://www.6502.org/tutorials/6502opcodes.html
                uint hi = (off & 0xFF) == 0xFF ? off - 0xFF : off + 1;
                uint oldPC = PC;
                PC = ReadByte(off) | (ReadByte(hi) << 8);

                if ((oldPC & 0xFF00) != (PC & 0xFF00))
                {
                    Cycle += 2;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [OpCodeDef(OpCode = 0xB0, Cycles = 2)]
        private void BCS() => Branch(F.Carry);

        [OpCodeDef(OpCode = 0x90, Cycles = 2)]
        private void BCC() => Branch(!F.Carry);

        [OpCodeDef(OpCode = 0xF0, Cycles = 2)]
        private void BEQ() => Branch(F.Zero);

        [OpCodeDef(OpCode = 0xD0, Cycles = 2)]
        private void BNE() => Branch(!F.Zero);

        [OpCodeDef(OpCode = 0x70, Cycles = 2)]
        private void BVS() => Branch(F.Overflow);

        [OpCodeDef(OpCode = 0x50, Cycles = 2)]
        private void BVC() => Branch(!F.Overflow);

        [OpCodeDef(OpCode = 0x10, Cycles = 2)]
        private void BPL() => Branch(!F.Negative);

        [OpCodeDef(OpCode = 0x30, Cycles = 2)]
        private void BMI() => Branch(F.Negative);

        [OpCodeDef(OpCode = 0x81, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0x91, Mode = IndirectY, Cycles = 6)]
        [OpCodeDef(OpCode = 0x95, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0x99, Mode = AbsoluteY, Cycles = 5)]
        [OpCodeDef(OpCode = 0x9D, Mode = AbsoluteX, Cycles = 5)]
        [OpCodeDef(OpCode = 0x85, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x8D, Mode = Absolute, Cycles = 4)]
        private void STA() => AddressWrite(A);

        [OpCodeDef(OpCode = 0x96, Mode = ZeroPageY, Cycles = 4)]
        [OpCodeDef(OpCode = 0x86, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x8E, Mode = Absolute, Cycles = 4)]
        private void STX() => AddressWrite(X);

        [OpCodeDef(OpCode = 0x94, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0x84, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x8C, Mode = Absolute, Cycles = 4)]
        private void STY() => AddressWrite(Y);

        [OpCodeDef(OpCode = 0x18, Cycles = 2)]
        private void CLC() => F.Carry = false;

        [OpCodeDef(OpCode = 0x38, Cycles = 2)]
        private void SEC() => F.Carry = true;

        [OpCodeDef(OpCode = 0x58, Cycles = 2)]
        private void CLI() => F.InterruptsDisabled = false;

        [OpCodeDef(OpCode = 0x78, Cycles = 2)]
        private void SEI() => F.InterruptsDisabled = true;

        [OpCodeDef(OpCode = 0xB8, Cycles = 2)]
        private void CLV() => F.Overflow = false;

        [OpCodeDef(OpCode = 0xD8, Cycles = 2)]
        private void CLD() => F.DecimalMode = false;

        [OpCodeDef(OpCode = 0xF8, Cycles = 2)]
        private void SED() => F.DecimalMode = true;

        [OpCodeDef(OpCode = 0xEA, Cycles = 2)]
        [OpCodeDef(OpCode = 0x1A, Cycles = 2)] // Unofficial
        [OpCodeDef(OpCode = 0x3A, Cycles = 2)] // Unofficial
        [OpCodeDef(OpCode = 0x5A, Cycles = 2)] // Unofficial
        [OpCodeDef(OpCode = 0x7A, Cycles = 2)] // Unofficial
        [OpCodeDef(OpCode = 0xDA, Cycles = 2)] // Unofficial
        [OpCodeDef(OpCode = 0xFA, Cycles = 2)] // Unofficial
        private void NOP() { }

        [OpCodeDef(OpCode = 0xA1, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0xA5, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0xA9, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0xAD, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0xB1, Mode = IndirectY, Cycles = 5, PageBoundary = true)]
        [OpCodeDef(OpCode = 0xB5, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0xB9, Mode = AbsoluteY, Cycles = 4, PageBoundary = true)]
        [OpCodeDef(OpCode = 0xBD, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void LDA() => A = AddressRead();

        [OpCodeDef(OpCode = 0xA0, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0xA4, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0xAC, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0xB4, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0xBC, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void LDY() => Y = AddressRead();

        [OpCodeDef(OpCode = 0xA2, Mode = Immediate, Cycles = 2, RMW = true)]
        [OpCodeDef(OpCode = 0xA6, Mode = ZeroPage, Cycles = 3, RMW = true)]
        [OpCodeDef(OpCode = 0xAE, Mode = Absolute, Cycles = 4, RMW = true)]
        [OpCodeDef(OpCode = 0xB6, Mode = ZeroPageY, Cycles = 4, RMW = true)]
        [OpCodeDef(OpCode = 0xBE, Mode = AbsoluteY, Cycles = 4, PageBoundary = true, RMW = true)]
        private void LDX() => X = AddressRead();

        [OpCodeDef(OpCode = 0x01, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0x05, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x09, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0x0D, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0x11, Mode = IndirectY, Cycles = 5, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x15, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0x19, Mode = AbsoluteY, Cycles = 4, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x1D, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void ORA() => A |= AddressRead();

        [OpCodeDef(OpCode = 0x21, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0x25, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x29, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0x2D, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0x31, Mode = IndirectY, Cycles = 5, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x35, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0x39, Mode = AbsoluteY, Cycles = 4, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x3D, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void AND() => A &= AddressRead();

        [OpCodeDef(OpCode = 0x41, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0x45, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x49, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0x4D, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0x51, Mode = IndirectY, Cycles = 5, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x55, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0x59, Mode = AbsoluteY, Cycles = 4, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x5D, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void EOR() => A ^= AddressRead();

        [OpCodeDef(OpCode = 0xE1, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0xE5, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x69, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0xE9, Mode = Immediate, Cycles = 2)] // Official duplicate of $69
        [OpCodeDef(OpCode = 0xEB, Mode = Immediate, Cycles = 2)] // Unofficial duplicate of $69
        [OpCodeDef(OpCode = 0xED, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0xF1, Mode = IndirectY, Cycles = 5, PageBoundary = true)]
        [OpCodeDef(OpCode = 0xF5, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0xF9, Mode = AbsoluteY, Cycles = 4, PageBoundary = true)]
        [OpCodeDef(OpCode = 0xFD, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void SBC() => ADCImpl((byte)~AddressRead());

        [OpCodeDef(OpCode = 0x61, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0x65, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0x69, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0x6D, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0x71, Mode = IndirectY, Cycles = 5, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x75, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0x79, Mode = AbsoluteY, Cycles = 4, PageBoundary = true)]
        [OpCodeDef(OpCode = 0x7D, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void ADC() => ADCImpl(AddressRead());

        private void ADCImpl(uint val)
        {
            int nA = (sbyte)A + (sbyte)val + (sbyte)(F.Carry ? 1 : 0);
            F.Overflow = nA < -128 || nA > 127;
            F.Carry = (A + val + (F.Carry ? 1 : 0)) > 0xFF;
            A = (byte)(nA & 0xFF);
        }

        [OpCodeDef(OpCode = 0x00, Cycles = 7)]
        private void BRK()
        {
            NextByte();
            Push(P | BreakSourceBit);
            F.InterruptsDisabled = true;
            PC = ReadByte(0xFFFE) | (ReadByte(0xFFFF) << 8);
        }

        [OpCodeDef(OpCode = 0xC1, Mode = IndirectX, Cycles = 6)]
        [OpCodeDef(OpCode = 0xC5, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0xC9, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0xCD, Mode = Absolute, Cycles = 4)]
        [OpCodeDef(OpCode = 0xD1, Mode = IndirectY, Cycles = 5, PageBoundary = true)]
        [OpCodeDef(OpCode = 0xD5, Mode = ZeroPageX, Cycles = 4)]
        [OpCodeDef(OpCode = 0xD9, Mode = AbsoluteY, Cycles = 4, PageBoundary = true)]
        [OpCodeDef(OpCode = 0xDD, Mode = AbsoluteX, Cycles = 4, PageBoundary = true)]
        private void CMP() => CMPImpl(A);

        [OpCodeDef(OpCode = 0xE0, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0xE4, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0xEC, Mode = Absolute, Cycles = 4)]
        private void CPX() => CMPImpl(X);

        [OpCodeDef(OpCode = 0xC0, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0xC4, Mode = ZeroPage, Cycles = 3)]
        [OpCodeDef(OpCode = 0xCC, Mode = Absolute, Cycles = 4)]
        private void CPY() => CMPImpl(Y);

        private void CMPImpl(uint reg)
        {
            long d = reg - (int)AddressRead();

            F.Negative = (d & 0x80) > 0 && d != 0;
            F.Carry = d >= 0;
            F.Zero = d == 0;
        }

        [OpCodeDef(OpCode = 0x46, Mode = ZeroPage, Cycles = 5, RMW = true)]
        [OpCodeDef(OpCode = 0x4E, Mode = Absolute, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x56, Mode = ZeroPageX, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x5E, Mode = AbsoluteX, Cycles = 7, RMW = true)]
        [OpCodeDef(OpCode = 0x4A, Mode = Direct, Cycles = 2, RMW = true)]
        private void LSR()
        {
            uint D = AddressRead();
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            _F(D);
            AddressWrite(D);
        }

        [OpCodeDef(OpCode = 0x06, Mode = ZeroPage, Cycles = 5, RMW = true)]
        [OpCodeDef(OpCode = 0x0E, Mode = Absolute, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x16, Mode = ZeroPageX, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x1E, Mode = AbsoluteX, Cycles = 7, RMW = true)]
        [OpCodeDef(OpCode = 0x0A, Mode = Direct, Cycles = 2, RMW = true)]
        private void ASL()
        {
            uint D = AddressRead();
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            _F(D);
            AddressWrite(D);
        }

        [OpCodeDef(OpCode = 0x66, Mode = ZeroPage, Cycles = 5, RMW = true)]
        [OpCodeDef(OpCode = 0x6E, Mode = Absolute, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x76, Mode = ZeroPageX, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x7E, Mode = AbsoluteX, Cycles = 7, RMW = true)]
        [OpCodeDef(OpCode = 0x6A, Mode = Direct, Cycles = 2, RMW = true)]
        private void ROR()
        {
            uint D = AddressRead();
            bool c = F.Carry;
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            if (c) D |= 0x80;
            _F(D);
            AddressWrite(D);
        }

        [OpCodeDef(OpCode = 0x26, Mode = ZeroPage, Cycles = 5, RMW = true)]
        [OpCodeDef(OpCode = 0x2E, Mode = Absolute, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x36, Mode = ZeroPageX, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0x3E, Mode = AbsoluteX, Cycles = 7, RMW = true)]
        [OpCodeDef(OpCode = 0x2A, Mode = Direct, Cycles = 2, RMW = true)]
        private void ROL()
        {
            uint D = AddressRead();
            bool c = F.Carry;
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            if (c) D |= 0x1;
            _F(D);
            AddressWrite(D);
        }

        [OpCodeDef(OpCode = 0xE6, Mode = ZeroPage, Cycles = 5, RMW = true)]
        [OpCodeDef(OpCode = 0xEE, Mode = Absolute, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0xF6, Mode = ZeroPageX, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0xFE, Mode = AbsoluteX, Cycles = 7, RMW = true)]
        private void INC()
        {
            byte D = (byte)(AddressRead() + 1);
            _F(D);
            AddressWrite(D);
        }

        [OpCodeDef(OpCode = 0xC6, Mode = ZeroPage, Cycles = 5, RMW = true)]
        [OpCodeDef(OpCode = 0xCE, Mode = Absolute, Cycles = 3, RMW = true)]
        [OpCodeDef(OpCode = 0xD6, Mode = ZeroPageX, Cycles = 6, RMW = true)]
        [OpCodeDef(OpCode = 0xDE, Mode = AbsoluteX, Cycles = 7, RMW = true)]
        private void DEC()
        {
            byte D = (byte)(AddressRead() - 1);
            _F(D);
            AddressWrite(D);
        }

        #region Unofficial Opcodes

        [OpCodeDef(OpCode = 0x80, Cycles = 2)]
        [OpCodeDef(OpCode = 0x82, Cycles = 2)]
        [OpCodeDef(OpCode = 0x89, Cycles = 2)]
        [OpCodeDef(OpCode = 0xC2, Cycles = 2)]
        [OpCodeDef(OpCode = 0xE2, Cycles = 2)]
        private void SKB() => NextByte(); // Essentially a 2-byte NOP

        [OpCodeDef(OpCode = 0x0B, Mode = Immediate, Cycles = 2)]
        [OpCodeDef(OpCode = 0x2B, Mode = Immediate, Cycles = 2)]
        private void ANC()
        {
            A &= AddressRead();
            F.Carry = F.Negative;
        }

        [OpCodeDef(OpCode = 0x4B, Mode = Immediate, Cycles = 2)]
        private void ALR()
        {
            A &= AddressRead();
            F.Carry = (A & 0x1) > 0;
            A >>= 1;
            _F(A);
        }

        [OpCodeDef(OpCode = 0x6B, Mode = Immediate, Cycles = 2)]
        private void ARR()
        {
            A &= AddressRead();
            bool c = F.Carry;
            F.Carry = (A & 0x1) > 0;
            A >>= 1;
            if (c) A |= 0x80;
            _F(A);
        }

        [OpCodeDef(OpCode = 0xAB, Mode = Immediate, Cycles = 2)]
        private void ATX()
        {
            // This opcode ORs the A register with #$EE, ANDs the result with an immediate 
            // value, and then stores the result in both A and X.
            A |= ReadByte(0xEE);
            A &= AddressRead();
            X = A;
        }

        #endregion
    }
}
