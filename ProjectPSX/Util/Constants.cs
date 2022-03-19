namespace ProjectPSX {

    public enum EX {
        INTERRUPT = 0x0,
        LOAD_ADRESS_ERROR = 0x4,
        STORE_ADRESS_ERROR = 0x5,
        BUS_ERROR_FETCH = 0x6,
        SYSCALL = 0x8,
        BREAK = 0x9,
        ILLEGAL_INSTR = 0xA,
        COPROCESSOR_ERROR = 0xB,
        OVERFLOW = 0xC
    }

    public enum Width {
        WORD,
        BYTE,
        HALF
    }

}