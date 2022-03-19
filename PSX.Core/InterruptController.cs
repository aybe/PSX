﻿using System;

namespace ProjectPSX.Devices {
    public class InterruptController : IInterruptController
    {

        private uint ISTAT; //IF Trigger that needs to be ACK
        private uint IMASK; //IE Global Interrupt enable

        public void set(Interrupt interrupt) {
            ISTAT |= (uint)interrupt;
            //Console.WriteLine($"ISTAT SET MANUAL FROM DEVICE: {ISTAT:x8} IMASK {IMASK:x8}");
        }


        internal void writeISTAT(uint value) {
            ISTAT &= value & 0x7FF;
            //Console.ForegroundColor = ConsoleColor.Magenta;
            //Console.WriteLine($"[IRQ] [ISTAT] Write {value:x8} ISTAT {ISTAT:x8}");
            //Console.ResetColor();
            //Console.ReadLine();
        }

        internal void writeIMASK(uint value) {
            IMASK = value & 0x7FF;
            //Console.WriteLine($"[IRQ] [IMASK] Write {IMASK:x8}");
            //Console.ReadLine();
        }

        internal uint loadISTAT() {
            //Console.WriteLine($"[IRQ] [ISTAT] Load {ISTAT:x8}");
            //Console.ReadLine();
            return ISTAT;
        }

        internal uint loadIMASK() {
            //Console.WriteLine($"[IRQ] [IMASK] Load {IMASK:x8}");
            //Console.ReadLine();
            return IMASK;
        }

        public bool interruptPending() {
            return (ISTAT & IMASK) != 0;
        }

        public void write(uint addr, uint value) {
            uint register = addr & 0xF;
            if(register == 0) {
                ISTAT &= value & 0x7FF;
            } else if(register == 4) {
                IMASK = value & 0x7FF;
            }
        }

        public uint load(uint addr) {
            uint register = addr & 0xF;
            if (register == 0) {
                return ISTAT;
            } else if (register == 4) {
                return IMASK;
            } else {
                return 0xFFFF_FFFF;
            }
        }
    }
}