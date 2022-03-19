namespace ProjectPSX;

public struct Instr {
    public uint value;                 //raw
    public uint opcode => value >> 26; //Instr opcode

    //I-Type
    public uint rs => (value >> 21) & 0x1F; //Register Source
    public uint rt => (value >> 16) & 0x1F; //Register Target
    public uint imm => value & 0xFFFF;      //Immediate value
    public uint imm_s => (uint)(short)imm;  //Immediate value sign extended

    //R-Type
    public uint rd => (value >> 11) & 0x1F;
    public uint sa => (value >> 6) & 0x1F; //Shift Amount
    public uint function => value & 0x3F;  //Function

    //J-Type                                       
    public uint addr => value & 0x3FFFFFF; //Target Address

    //id / Cop
    public uint id => opcode & 0x3; //This is used mainly for coprocesor opcode id but its also used on opcodes that trigger exception
}