using System;

namespace Transformice.Modules
{
    public class BaseBuffer
    {
        protected byte[] Buffer = new byte[0];
        protected int Length = 0;

        private void IWrite(int data)
        {
            this.Buffer[this.Length - 1] = (byte)(data);
        }

        protected void Write(int data)
        {
            if (this.Buffer.Length < (this.Length + 1))
            {
                this.IncreaseCapacity((this.Length + 1) - (this.Length));
                this.IWrite(data);
            }
            else
            {
                this.IWrite(data);
            }
        }

        public void Write(string data, int off, int count)
        {
            for (int i = off; i < count; i++)
            {
                this.Write((int)(data[i]));
            }
        }

        public void Write(byte[] data, int off, int count)
        {
            for (int i = off; i < count; i++)
            {
                this.Write((int)(data[i]));
            }
        }

        protected int Read()
        {
            this.Length -= 1;
            int data = (int)(this.Buffer[0]);
            byte[] newBuffer = new byte[Length];
            Array.Copy(this.Buffer, 1, newBuffer, 0, this.Length);
            this.Buffer = newBuffer;
            return data;
        }

        protected int Read(int index)
        {
            if (index < 0)
            {
                int data = (int)(this.Buffer[this.Length - index]);
                this.Length -= 1;
                byte[] newBuffer = new byte[Length];
                Array.Copy(this.Buffer, 0, newBuffer, this.Length - index, this.Length);
                this.Buffer = newBuffer;
                return data;
            }
            else
            {
                int data = (int)(this.Buffer[index]);
                this.Length -= 1;

                byte[] newBuffer1 = new byte[index];
                byte[] newBuffer = new byte[Length];

                Array.Copy(this.Buffer, 0, newBuffer1, 0, index);
                Array.Copy(newBuffer1, 0, newBuffer, 0, newBuffer1.Length - 1);
                Array.Copy(this.Buffer, 0, newBuffer, index, this.Length - index);

                this.Buffer = newBuffer;
                return data;
            }
        }

        protected byte[] Read(int off, int count)
        {
            this.Length -= count;
            byte[] data = new byte[count];
            byte[] newBuffer = new byte[this.Length];

            Array.Copy(this.Buffer, off, data, 0, count);
            Array.Copy(this.Buffer, count, newBuffer, 0, this.Length);
            this.Buffer = newBuffer;
            return data;
        }

        protected byte[] Read(byte[] buffer, int off, int count)
        {
            byte[] data = new byte[count];
            byte[] newBuffer = new byte[buffer.Length - count];

            Array.Copy(buffer, off, data, 0, count);
            Array.Copy(buffer, count, newBuffer, 0, buffer.Length - count);
            buffer = newBuffer;
            return data;
        }

        protected void IWriteUTF(string data)
        {
            int byteLength = 0;
            int chr, count = 0;

            for (int i = 0; i < data.Length; i++)
            {
                chr = data[i];
                if (chr <= 0x7F)
                {
                    byteLength += 1;
                }
                else if (chr <= 0x07FF)
                {
                    byteLength += 2;
                }
                else
                {
                    byteLength += 3;
                }
            }

            byte[] bytesLeft = new byte[byteLength + 2];
            bytesLeft[count++] = (byte)((byteLength >> 8) & 255);
            bytesLeft[count++] = (byte)((byteLength >> 0) & 255);

            for (int i = 0; i < data.Length; i++)
            {
                chr = data[i];
                if (!(chr <= 0x7F))
                {
                    break;
                }
                bytesLeft[count++] = (byte)(chr);
            }

            for (int i = 0; i < data.Length; i++)
            {
                chr = data[i];
                if (chr >= 0x01 && chr <= 0x7F)
                {
                    //bytesLeft[count++] = (byte)(chr);
                }
                else if (chr <= 0x07FF)
                {
                    bytesLeft[count++] = (byte)(0xC0 | ((chr >> 6) & 0x1F));
                    bytesLeft[count++] = (byte)(0x80 | ((chr >> 0) & 0x3F));
                }
                else
                {
                    bytesLeft[count++] = (byte)(0xE0 | ((chr >> 12) & 0x0F));
                    bytesLeft[count++] = (byte)(0x80 | ((chr >> 6) & 0x3F));
                    bytesLeft[count++] = (byte)(0x80 | ((chr >> 0) & 0x3F));
                }
            }
            this.Write(bytesLeft, 0, byteLength + 2);
        }

        protected string IReadUTF()
        {
            int dataLength = (this.Read() << 8) | (this.Read() << 0);
            byte[] byteArray = new byte[dataLength];
            char[] charArray = new char[dataLength];

            int count = 0;
            int charCount = 0;

            int chr2 = 0;
            int chr3 = 0;

            byteArray = this.Read(0, dataLength);
            while (count < dataLength)
            {
                int chr = (int)(byteArray[count] & 255);
                if (chr > 0x7F)
                {
                    break;
                }
                count += 1;
                charArray[charCount] = (char)(chr);
                charCount++;
            }

            while (count < dataLength)
            {
                int chr = (int)(byteArray[count] & 255);
                if ((chr >> 4) <= 7)
                {
                    count += 1;
                    charCount++;
                    charArray[charCount] = (char)(chr);
                }
                else if ((chr >> 4) <= 13)
                {
                    count += 2;
                    charCount++;
                    chr2 = (int)(byteArray[count - 1]);
                    charArray[charCount] = (char)((chr & 0x1F) << 6 | (chr2 & 0x3F));
                }
                else if ((chr >> 4) <= 14)
                {
                    count += 3;
                    charCount++;
                    chr2 = (int)(byteArray[count - 2]);
                    chr3 = (int)(byteArray[count - 1]);
                    charArray[charCount] = (char)((chr & 0x0F) << 12 | (chr2 & 0x3F) << 6 | (chr3 & 0x3F) << 0);
                }
            }
            return new string(charArray, 0, charCount);
        }

        private void IncreaseCapacity(int count)
        {
            byte[] newBuffer = new byte[this.Length + count];
            Array.Copy(this.Buffer, newBuffer, this.Length);
            this.Buffer = newBuffer;
            this.Length += count;
        }

        protected byte[] StringToByteArray(string data)
        {
            byte[] stored = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                stored[i] = (byte)(data[i]);
            }
            return stored;
        }

        public byte[] ToByteArray()
        {
            return this.Buffer;
        }
    }

    public class ByteArray : BaseBuffer
    {
        public ByteArray()
        {

        }

        public ByteArray(byte[] buffer)
        {
            if (buffer.Length != 0)
            {
                this.Buffer = new byte[buffer.Length];
                this.Length = buffer.Length;
                Array.Copy(buffer, 0, this.Buffer, 0, buffer.Length);
            }
        }

        public ByteArray(string buffer = "")
        {
            if (buffer != String.Empty || buffer.Length != 0)
            {
                byte[] byteBuffer = StringToByteArray(buffer);
                this.Buffer = new byte[buffer.Length];
                Array.Copy(byteBuffer, 0, this.Buffer, 0, buffer.Length);
            }
        }
        
        public void WriteBytes(byte[] data)
        {
            foreach(byte b in data)
            {
                this.Write(b);
            }
        }

        public void WriteBoolean(bool data)
        {
            this.Write(data ? 1 : 0);
        }

        public void WriteByte(byte data)
        {
            this.Write((data >> 0) & 255);
        }

        public void WriteShort(short data)
        {
            this.Write((data >> 8) & 255);
            this.Write((data >> 0) & 255);
        }

        public void WriteInt(int data)
        {
            this.Write((data >> 24) & 255);
            this.Write((data >> 16) & 255);
            this.Write((data >> 8) & 255);
            this.Write((data >> 0) & 255);
        }

        public void WriteLong(long data)
        {
            this.Write((Convert.ToInt32(data) >> 56) & 255);
            this.Write((Convert.ToInt32(data) >> 48) & 255);
            this.Write((Convert.ToInt32(data) >> 40) & 255);
            this.Write((Convert.ToInt32(data) >> 32) & 255);
            this.Write((Convert.ToInt32(data) >> 24) & 255);
            this.Write((Convert.ToInt32(data) >> 16) & 255);
            this.Write((Convert.ToInt32(data) >> 8) & 255);
            this.Write((Convert.ToInt32(data) >> 0) & 255);
        }

        public void WriteUTF(string data)
        {
            this.IWriteUTF(data);
        }

        public bool ReadBoolean()
        {
            int data = this.Read();
            return data != 0;
        }

        public byte ReadByte()
        {
            int data = this.Read() << 0;
            return (byte)(data);
        }

        public short ReadShort()
        {
            int data1 = this.Read() << 8;
            int data2 = this.Read() << 0;
            return (short)(data1 | data2);
        }

        public int ReadInt()
        {
            int data1 = this.Read() << 24;
            int data2 = this.Read() << 16;
            int data3 = this.Read() << 8;
            int data4 = this.Read() << 0;
            return (int)(data1 | data2 | data3 | data4);
        }

        public long ReadLong()
        {
            int data1 = this.Read() << 56;
            int data2 = this.Read() << 48;
            int data3 = this.Read() << 40;
            int data4 = this.Read() << 32;
            int data5 = this.Read() << 24;
            int data6 = this.Read() << 16;
            int data7 = this.Read() << 8;
            int data8 = this.Read() << 0;
            return (long)(data1 | data2 | data3 | data4 | data5 | data6 | data7 | data8);
        }

        public string ReadUTF()
        {
            return this.IReadUTF();
        }

        public bool BytesAvailable()
        {
            return this.Buffer.Length > 0;
        }

        public string ToRepr()
        {
            string stored = "";

            foreach (int chr in this.Buffer)
            {
                if (chr > 31 && chr < 127)
                {
                    stored += (char)(chr);
                }
                else
                {
                    string hexValue = chr.ToString("X2");
                    stored += $@"\x{hexValue}";
                }
            }
            return $"\"{stored}\"";
        }
    }
}
