using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MySecretChatServer
{
    class Client
    {
        internal string Id;
        internal NetworkStream stream;
        string user_name;
        TcpClient tcpClient;
        Server server;
        private bool isntClose = true;
        private bool isFile = false;
       
        public Client(TcpClient client, Server server)
        {
            Id = Guid.NewGuid().ToString();
            tcpClient = client;
            this.server = server;

        }

        public void Process()
        {
            try
            {
                string message;
                stream = tcpClient.GetStream();
                message = GetMessage();
                string pack = "You are join the chat.";
                byte[] dataMessage = Encoding.Unicode.GetBytes(pack);
                byte[] data = Encoding.Unicode.GetBytes(dataMessage.Length + " " + pack);
                stream.Write(data, 0, data.Length);

                user_name = message;
                message = "~~~~~~[" + user_name + "] computer join the chat.";
                server.AddConnection(this);
                Console.WriteLine(message);
                server.Broadcast(message, this.Id);
                while (isntClose)
                {
                    try
                    {
                        message = GetMessage();
                        if(isFile)
                        {
                            isFile = false;
                            server.BroadcastFile(message, null);
                            continue;
                        }
                        message = String.Format("[{0}][{1}] {2}", 
                            MyTime.getTime(), user_name, message);
                        Console.WriteLine(message);
                        server.Broadcast(message, this.Id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Close();
            }
        }

        private string GetMessage()
        {
            byte[] lenData = new byte[64];
            byte[] buf = new byte[64];
            stream.Read(buf, 0, buf.Length);
            if(buf[0] == 94)
            {
                isFile = false;
                string fileName = ReadFile(lenData, buf);
                return String.Format("File {0} is load.", fileName);
            }
            int i = 0;
            for (; i < buf.Length; i++)
            {
                if (buf[i] == 32)
                {
                    i+=2;
                    break;
                }
                lenData[i] = buf[i];
            }
            

            int len = int.Parse(Encoding.Unicode.GetString(lenData));

            StringBuilder builder = new StringBuilder();



            byte[] data = new byte[64-i<len ? 64 - i : len];
            int m = 0;
            for(; m < lenData.Length; m++)
            {
                if (m < i) continue;
                if (m - i > len - 1) break;
                data[m-i] = buf[m];
            }
            builder.Append(Encoding.Unicode.GetString(data, 0, data.Length));
            if (m >= len)
            {
                return builder.ToString();
            }

            int pos = 0;
            byte[] data2 = new byte[len + i - m];
            while (pos < len + i - m)
            {
                pos += stream.Read(data2, 0, data2.Length);
            }

            builder.Append(Encoding.Unicode.GetString(data2, 0, data2.Length));
            
            return builder.ToString();
        }

        protected string ReadFile(byte[] lenData, byte[] buf)
        {
            int i = 2;
            int counter = 0;
            int z = 0;
            string fileType = "";
            byte[] bufFileName = new byte[64];
            for (; i < buf.Length; i++)
            {
                if (buf[i] == 32 && counter == 0)
                {
                    i += 2;
                    counter = 1;
                }
                if (counter == 0) lenData[i - 2] = buf[i];
                if (counter == 1)
                {
                    if (buf[i] == 32)
                    {
                        i += 2;
                        break;
                    }
                    bufFileName[z] = buf[i];
                    z++;
                }

            }

            int len = int.Parse(Encoding.Unicode.GetString(lenData));
            fileType = Encoding.Unicode.GetString(bufFileName);
            StringBuilder builder = new StringBuilder();



            /*
            byte[] data = new byte[20 - i  < len ? 20 - i : len];
            int m = 0;
            for (; m < lenData.Length; m++)
            {
                if (m < i) continue;
                if (m - i > len - 1) break;
                data[m - i] = buf[m];
            }
            builder.Append(Encoding.Unicode.GetString(data, 0, data.Length));
            */

            int pos = 0;
            i += 2;
            byte[] data2 = new byte[len];//len + i - m
            while (pos < len) //len + i - m
            {
                pos += stream.Read(data2, 0, data2.Length);
            }
            //byte[] data3 = new byte[data.Length + data2.Length];
            /*for(int j = 0; j<data3.Length; j++)
            {
                if(j < data.Length)
                {
                    data3[j] = data[j];
                }
                else
                {
                    data3[j] = data2[j - data.Length];
                }
            }*/
            string fileName = String.Format("[{0}][{1}]", MyTime.getTimeForFileName(), user_name);
            fileType = fileType.Split("\0")[0];
            server.BroadcastFile(data2, Id, fileName, fileType);
            return fileName;
            
        }
    
    protected internal void Close()
        {
            isntClose = false;
            if (stream != null) stream.Close();
            if (tcpClient != null) tcpClient.Close();
            Console.WriteLine("Connection [" + user_name + "] is closed...");
            server.RemoveConnecton(Id);
            
        }
    }
}
