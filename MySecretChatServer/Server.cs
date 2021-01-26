using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MySecretChatServer
{
    class Server
    {
        static TcpListener tcpListener;
        List<Client> clients = new List<Client>();

        protected internal void AddConnection(Client client)
        {
            clients.Add(client);
        }

        protected internal void RemoveConnecton(string id)
        {
            Client client = clients.FirstOrDefault(c => c.Id == id);
            if(client != null)
            {
                clients.Remove(client);

            }
        }

        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 1111);
                tcpListener.Start();
                Console.WriteLine("Server is started. Wait for connecting...");
                while (true)
                {
                    TcpClient myClient = tcpListener.AcceptTcpClient();
                    Client clientObj = new Client(myClient, this);
                    Thread t = new Thread(new ThreadStart(clientObj.Process));
                    t.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        protected internal void Broadcast(string message, string id)
        {
            byte[] messageData = Encoding.Unicode.GetBytes(message);
            byte[] fullData = Encoding.Unicode.GetBytes(messageData.Length + " " + message);
            for(int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id) continue;
                clients[i].stream.Write(fullData, 0, fullData.Length);
            }
        }
        protected internal void BroadcastFile(string message, string id)
        {
            byte[] messageData = Encoding.Unicode.GetBytes(message);
            byte[] fullData = Encoding.Unicode.GetBytes("^" + messageData.Length + " " + message);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].stream.Write(fullData, 0, fullData.Length);
            }
        }
        protected internal void BroadcastFile(byte[] message, string id, string fileName, string fileType)
        {
            string messageInfo = String.Format("~~~~{0} send file.~~~~", fileName + fileType);
            
            for (int i = 0; i < clients.Count; i++)
            {
                
                byte[] data = Encoding.Unicode.GetBytes("^" + message.Length + " " + fileName + fileType + " ");
                clients[i].stream.Write(data);
                clients[i].stream.Write(message, 0, message.Length);
                /*if (id == clients[i].Id)
                {
                    string myMessageToSendler = "Your file has been successfully delivered.";
                    byte[] dataMyMessage = Encoding.Unicode.GetBytes(myMessageToSendler);
                    byte[] dataToWrite = Encoding.Unicode.GetBytes(dataMyMessage.Length + " " + myMessageToSendler);
                    clients[i].stream.Write(dataToWrite, 0, dataToWrite.Length);
                }*/

            }
            
            Broadcast(messageInfo, id);

        }

        protected internal void StopServer()
        {
            tcpListener.Stop();
            for(int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}
