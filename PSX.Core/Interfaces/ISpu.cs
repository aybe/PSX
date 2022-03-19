namespace PSX.Core.Interfaces;

public interface ISpu
{
    void PushCdBufferSamples(byte[] decodedXaAdpcm);
}