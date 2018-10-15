using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Transformice.Modules;

namespace Transformice.Base
{
    public class Client
    {
        private Socket socket;

        private byte[] buffer = new byte[1024];

        public Client(Server server, Socket socket)
        {
            this.socket = socket;

            this.ConnectCallback();

        }

        public void Start()
        {
            this.socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReadCallback), this.socket);
        }

        private void Close()
        {
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Close();
        }

        private void Send(String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            this.socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), this.socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                int sent = clientSocket.EndSend(ar);
            }
            catch
            {
                // Do Nothing
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            int received = clientSocket.EndReceive(ar);

            if (received > 0)
            {
                this.ReceivedCallback(buffer);

                try
                {
                    this.socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReadCallback), this.socket);
                }
                catch
                {
                    // Do Nothing
                }
            }
        }

        private void ConnectCallback()
        {
            Console.WriteLine("[Client Log] Player connected!");
        }

        private void ReceivedCallback(byte[] data)
        {
            string content = Encoding.ASCII.GetString(data);
            if (content.StartsWith("<policy-file-request>"))
            {
                this.Send("<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"*\" /></cross-domain-policy>");
                this.Close();
            }
            else
            {
                ByteArray packet = new ByteArray(data);
                int packetLength = packet.ReadByte();
                List<Int32> lengths = new List<int>() { 1, 2, 4 };
                int count = 1;

                if (lengths.Contains(packetLength))
                {
                    int size = 0;

                    if (packetLength == 1)
                    {
                        size = packet.ReadByte();
                    }
                    else if (packetLength == 2)
                    {
                        size = packet.ReadShort();
                    }
                    else
                    {
                        size = packet.ReadInt();
                    }
                    count += size + 1;
                    byte[] toByte = new byte[count];

                    Array.Copy(data, toByte, count);
                    this.ParsePackets(packet, toByte);
                }
            }
        }

        public int GetStructType(int length)
        {
            int type = 1;
            if (length <= byte.MaxValue)
            {
                type = 1;
            }
            else if (length <= short.MaxValue)
            {
                type = 2;
            }
            else if (length <= 0xFFFFFF)
            {
                type = 3;
            }
            else if (length <= int.MaxValue)
            {
                type = 4;
            }
            else
            {
                type = 8;
            }
            return type;
        }

        public void SendData(int token1, int token2, byte[] data)
        {
            int dataLength = data.Length + 2;
            int type = this.GetStructType(dataLength);

            ByteArray packet = new ByteArray();
            packet.WriteByte((byte)(type));
            if (type == 1)
            {
                packet.WriteByte((byte)(dataLength));
            }
            else if (type == 2)
            {
                packet.WriteShort((short)(dataLength));
            }
            else if (type == 3)
            {
                packet.WriteShort((short)(dataLength));
            }
            else if (type == 4)
            {
                packet.WriteInt(dataLength);
            }
            else
            {
                packet.WriteLong((long)(dataLength));
            }
            packet.WriteByte((byte)(token1));
            packet.WriteByte((byte)(token2));
            packet.Write(data, 0, data.Length);

            byte[] byteData = packet.ToByteArray();
            this.socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), this.socket);
        }

        private void ParsePackets(ByteArray packet, byte[] data)
        {
            int ID = packet.ReadByte();
            int C = packet.ReadByte();
            int CC = packet.ReadByte();
            Parse.Packets(this, ID, C, CC, packet);
        }

        public void SendCorrectVersion()
        {
            ByteArray packet = new ByteArray();
            packet.WriteInt(0);    // Connected Players
            packet.WriteByte(1);   // Last Packet ID
            packet.WriteUTF("br"); // Default Langue
            packet.WriteUTF("br"); // Default Langue
            packet.WriteInt(0);
            this.SendData(26, 3, packet.ToByteArray());
        }
    }
}