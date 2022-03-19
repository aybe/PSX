namespace ProjectPSX.Sound;

public interface ISPU
{
    void PushCdBufferSamples(byte[] decodedXaAdpcm);
}