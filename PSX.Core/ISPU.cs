namespace PSX.Core;

public interface ISPU
{
    void PushCdBufferSamples(byte[] decodedXaAdpcm);
}