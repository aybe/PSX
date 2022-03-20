namespace PSX.Core.Interfaces;

public interface ISPU
{
    void PushCdBufferSamples(byte[] decodedXaAdpcm);
}