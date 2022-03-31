// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

class MyTcpListener
{

  public class RandomData
  {
      public DateTimeOffset Date { get; set; }
      public int value { get; set; }
      public string? type { get; set; }
  }   

  public static void Main()
  {
    TcpListener server=null;
    try
    {
    
      // Set the TcpListener port 5300.
      Int32 port = 5300;
      IPAddress localAddr = IPAddress.Parse("127.0.0.1");

      // TcpListener server = new TcpListener(port);
      server = new TcpListener(localAddr, port);

      // Start listening for client requests.
      server.Start();
     
      int i;
      String data = null;
      // Buffer for reading data
      Byte[] bytes = new Byte[256];

      // Enter the listening loop.
      while(true)
      {
        Console.Write("Waiting for a connection... ");

        // Perform a blocking call to accept requests.
        // You could also use server.AcceptSocket() here.
        TcpClient client = server.AcceptTcpClient();
        Console.WriteLine("Connected!");
        
        Random rnd = new Random();
        int num = rnd.Next();
        Console.WriteLine();

        var randomData = new RandomData
        {
            Date = DateTime.Parse("2019-08-01"),
            value = num,
            type = "random"
        };

        string jsonString = JsonSerializer.Serialize(randomData);
        Console.WriteLine(jsonString);

        // Get a stream object for reading and writing
        NetworkStream stream = client.GetStream();

        // Loop to receive all the data sent by the client.
        while((i = stream.Read(bytes, 0, bytes.Length))!=0)
        {
          // Translate data bytes to a ASCII string.
          data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
          Console.WriteLine("Received: {0}", data);

          // Process the data sent by the client.
          data = jsonString;

          byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

          // Send back a response.
          stream.Write(msg, 0, msg.Length);
          Console.WriteLine("Sent: {0}", data);
        }

        // Shutdown and end connection
        client.Close();
      }
    }
    catch(SocketException e)
    {
      Console.WriteLine("SocketException: {0}", e);
    }
    finally
    {
       // Stop listening for new clients.
       server.Stop();
    }

    Console.WriteLine("\nHit enter to continue...");
    Console.Read();
  }
}

