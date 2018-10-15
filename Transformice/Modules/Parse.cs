using System;

namespace Transformice.Modules
{
    public static class Parse
    {
        public static void Packets(Base.Client client, int ID, int C, int CC, ByteArray packet)
        {
            if (C == 28)
            {
                if (CC == 1)
                {
                    string version = "1." + packet.ReadShort();
                    string ckey = packet.ReadUTF();
                    client.SendCorrectVersion();
                    Console.WriteLine($"[Client Log] Player connected with version {version} and ckey {ckey}");
                }
            }
            else
            {
                //Console.WriteLine("[{0}:{1},{2}] {3}", ID, C, CC, packet.ToRepr());
            }
        }
    }
}