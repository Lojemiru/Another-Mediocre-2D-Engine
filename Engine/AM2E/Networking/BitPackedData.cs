using System;
using System.Text;

namespace AM2E.Networking;

public class BitPackedData
{
    private const int MAX_BITPACKED_DATA_SIZE = 1024;
    private int writeBitPosition;
    private readonly byte[] data = new byte[MAX_BITPACKED_DATA_SIZE];
    private int dataSize;
    private int readBitPosition;
    private int readBytePosition;

    private bool canRead = false;

    public BitPackedData(byte[] data = null)
    {
        if (data == null)
        {
            canRead = false;
        }
        else
        {
            this.data = data;
            canRead = true;
        }
    }

    public void WriteBits(int value, int numBits)
    {
        if (numBits <= 0)
            return;

        canRead = true;
        while (numBits > 0)
        {
            if (writeBitPosition == 0)
            {
                data[dataSize] = 0;
                dataSize++;
            }
                
            var put = 8 - writeBitPosition;
                
            if (put > numBits)
            {
                put = numBits;
            }
                
            var part = value & ((1 << put) - 1);
            value >>= put;
            data[dataSize - 1] |= (byte)(part << writeBitPosition);
            numBits -= put;
            // Modulo 8 but without dividing.
            writeBitPosition = (writeBitPosition + put) & 7;
        }
    }

    public void WriteBool(bool value)
    {
        var intValue = (value ? 1 : 0);
        WriteBits(intValue, 1);
    }

    public void WriteID(string value)
    {
        var guid = new Guid(value);
        var bytes = guid.ToByteArray();
            
        for (var i = 0; i < 16; i++)
        {
            WriteBits(bytes[i], 8);
        }
    }

    public void WriteString(string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
            
        foreach (var t in bytes)
        {
            WriteBits(t, 8);
        }
            
        WriteBits(0, 8);
    }

    public int ReadBits(int numBits) 
    {
        var valueBits = 0;
        var value = 0;
            
        if (!canRead)
            return -1;

        while (valueBits < numBits)
        {
            if (readBitPosition == 0)
            {
                readBytePosition++;
            }
                
            var get = 8 - readBitPosition;
                
            if (get > (numBits - valueBits))
            {
                get = numBits - valueBits;
            }
                
            int part = data[readBytePosition - 1];
            part >>= readBitPosition;
            part &= (1 << get) - 1;
            value |= part << valueBits;
            valueBits += get;
            readBitPosition = (readBitPosition + get) & 7;
        }
            
        return value;
    }
        
    public bool ReadBool()
    {
        var value = ReadBits(1);
            
        return value != 0;
    }

    public string ReadID()
    {
        var bytes = new byte[16];
            
        for (var i = 0; i < 16; i++)
        {
            var value = ReadBits(8);
            bytes[i] = (byte)value;
        }
            
        var guid = new Guid(bytes);
            
        return guid.ToString();
    }

    public string ReadString(int maxCharacters)
    {
        var bytes = new byte[1024];
        var count = 0;
            
        for (var i = 0; i < maxCharacters; i++)
        {
            var value = ReadBits(8);
                
            if (value == 0)
                break;

            bytes[i] = (byte)value;
            count++;
        }
            
        var truncatedBytes = new byte[count];
        Array.Copy(bytes, truncatedBytes, count);
            
        return Encoding.ASCII.GetString(truncatedBytes);
    }

    public byte[] CopyData()
    {
        var res = new byte[dataSize];
            
        for (var i = 0; i < dataSize; i++)
        {
            res[i] = data[i];
        }
            
        return res;
    }

    public void Reset()
    {
        dataSize = 0;
        writeBitPosition = 0;
        readBitPosition = 0;
        readBytePosition = 0;
        canRead = false;
    }
}