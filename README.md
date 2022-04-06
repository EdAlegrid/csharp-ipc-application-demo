
## Monitor Data From Remote C# Application Through IPC
![](https://raw.githubusercontent.com/EdoLabs/src2/master/csharpAppQuicktour.svg?sanitize=true)
[](quicktour.svg)

In this quick tour, the client will watch data from a C# application through inter-process communication (ipc) using *tcp* with the remote device.

The client will send a *json* payload data { type:"random", source:"cs-server" } to a remote device and should receive a random value from the remote device e.g. { type:"random", source:"cs-server", value: 287798093 };

### Remote Device Setup

#### 1. Create a device project directory and install m2m.
```js
$ npm install m2m
```
#### 2. Save the code below as device.js in your project directory.
```js
'use strict';

const net = require('net');
const { Device } = require('m2m');

const device = new Device(400);

device.connect(() => {

  device.setData('ipc-channel', (data) => {

    let pl = JSON.stringify({source:'cs-server', type:'random'});

    TcpClient('127.0.0.1', 5400, pl, (err, d) => {
      if(err) return console.error('TcpClient error:', err.message);
      if(d){
        data.send(d);
      }
    });
  });

});

function TcpClient(ip, port, payload, cb){
  const client = new net.Socket();
  client.connect(port, ip);

  client.setEncoding('utf8');

  client.on("connect", () => {
    if(payload){
      client.write(payload);
    }
  });

  client.on('error', (error) => {
    console.log("Tcp client socket error:", error);
    if(error && cb){
      cb(error, null);
    }
    client.destroy();
  });

  client.on("data", (data) => {		
    if(cb){
      setImmediate(cb, null, data);
    }
    client.end();
  });

  client.on("close", (error) => {
    if(error && cb){
      console.log("Tcp client socket is closed:", error);
      cb(error, null);
    }
  });

  client.on("end", (error) => {
    if(error && cb){
      console.log("Tcp client socket connection is terminated:", error);
      cb(error, null);
    }
    client.unref();
  });
};
```
#### 3. Start your device application.
```js
$ node device.js
```

### C# Application Server Setup on Remote Device

#### 1. First, download and install the .NET SDK on your computer.

Next, open a terminal such as PowerShell, Command Prompt, or bash. Enter the following dotnet commands to create and run a C# server application:
```js
$ dotnet new console --output m2m-c#-server
```
```js
$ dotnet run --project m2m-c#-server
```

You should see the following output:

Hello World!

Navigate through the m2m-c#-server project directory and open the Program.cs file. Later on, we will modify its content from the downloaded github project *csharp-ipc-application-demo*.

#### 2. Download the *csharp-ipc-application-demo* example project.
```js
$ git clone https://github.com/EdAlegrid/csharp-ipc-application-demo.git
```
Check the *Program.cs* source file inside the *csharp-ipc-application-demo/app* sub-directory as shown below.

```js
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
        int num = rnd.Next(0, 100);

        Console.WriteLine();

        // Option to create a new json random data.
        var randomData = new RandomData
        {
            Date = DateTime.Parse("2019-08-01"),
            value = num,
            type = "random",
            source = "cs-server",
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

          // Check and verify the received JSON source property. 
          var sourceProp = jsonData["source"];

          if(string.Equals("cs-server", sourceProp.ToString())){
            Console.WriteLine("source " + sourceProp);
            jsonData["value"] = num;
          }
          else{
            Console.WriteLine("invalid payload source");
            jsonData["error"] = "invalid payload source";
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
```

#### 3. Copy the downloaded Program.cs content as shown above into your m2m-c#-server Program.cs file and save it. 

You need to open the Program.cs file from the m2m-c#-server directory and click save, not just replace it by copying the file. Otherwise it will not detect the changes. 

#### 4. Run again the application. It will recompile and start the application.
```js
$ dotnet run --project m2m-c#-server
```
Once the C# Application server is up and running, you should see a server output as shown below.
```js
*** C# Application Server ***

Server listening on: 127.0.0.1:5400
Waiting for a connection...
```
### Remote Client Setup

#### 1. Create a client project directory and install m2m.
```js
$ npm install m2m
```
#### 2. Save the code below as client.js in your client project directory.
```js
const { Client } = require('m2m');

let client = new Client();

client.connect(() => {

    client.watch({id:400, channel:'ipc-channel'}, (data) => {  
    try{
      let jd = JSON.parse(data);
      console.log('rcvd json data:', jd);
    }
    catch (e){
      console.log('rcvd string data:', data);
    }
  });

});
```
#### 3. Run your client application.
```js
$ node client.js
```
The client should receive a *json* data with a random value similar below.

*rcvd json data: { type: 'random', source: 'C#-server', value: 1605906241 }*
