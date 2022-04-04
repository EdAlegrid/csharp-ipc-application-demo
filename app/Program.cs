/*
 * File:   Program.cs
 * Author: Ed Alegrid
 *
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

class MyTcpListener
{

  public class RandomData
  {
      public DateTimeOffset Date { get; set; }
      public int value { get; set; }
      public string? type { get; set; }
      public string? source { get; set; }
  } 

  public static void Main()
  {
    TcpListener server=null;
    try
    {
    
      // Set the TcpListener port 5300.
      Int32 port = 5400;
      IPAddress localAddr = IPAddress.Parse("127.0.0.1");

      // TcpListener server = new TcpListener(port);
      server = new TcpListener(localAddr, port);

      // Start listening for client requests.
      server.Start();
     
      int i;

      String data = null;
      // Buffer for reading data
      Byte[] bytes = new Byte[256];

      Console.WriteLine("\n*** C# Application Server  ***");
      Console.WriteLine("\nServer listeing on " + localAddr + ":" + port);

      // Enter the listening loop.
      while(true)
      {
        Console.Write("Waiting for a connection... ");

        // Perform a blocking call to accept requests.
        // You could also use server.AcceptSocket() here.
        TcpClient client = server.AcceptTcpClient();
        Console.WriteLine("Connected!");
        
        // Generate a random number.
        Random rnd = new Random();
        int num = rnd.Next();

        Console.WriteLine();

        // Option to create a new json random data.
        var randomData = new RandomData
        {
            Date = DateTime.Parse("2019-08-01"),
            value = num,
            type = "random",
            source = "C#-server",
        };

        string jsonRandomData = JsonSerializer.Serialize(randomData);

        // Get a stream object for reading and writing
        NetworkStream stream = client.GetStream();

        // Loop to receive all the data sent by the client.
        while((i = stream.Read(bytes, 0, bytes.Length))!=0)
        {
          // Translate data bytes to a UTF8 string.
          data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
          Console.WriteLine("Received: {0}", data);

          // Parse the received data using JsonNode.
          JsonNode jsonData = JsonNode.Parse(data)!;
          // Create a value property and set its value equal to the generated random data.
          jsonData["value"] = num;

          // Check and verify the received JSON source property. 
          var sourceProp = jsonData["source"];

          if(string.Equals("C#-server", sourceProp.ToString())){
            Console.WriteLine("source " + sourceProp);
          }

          // Re-use the current received json data.
          var currentJsonData = jsonData.ToJsonString();
          byte[] msg = System.Text.Encoding.UTF8.GetBytes(currentJsonData);

          // or use a new json random data 
          // byte[] msg = System.Text.Encoding.UTF8.GetBytes(jsonRandomData);

          // Send back a response.
          stream.Write(msg, 0, msg.Length);
          Console.WriteLine("Sent: {0}", currentJsonData);
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

