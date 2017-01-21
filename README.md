# CSharp_AsyncTcpSockets


A server / multi client app on async sockets. Threading and multi process creation for clients included.

Developed with Visual Studio 2015 Community

---

![Screen Shot](https://github.com/Apollo013/CSharp_AsyncTcpSockets/blob/master/ScreenShot.png?raw=true "Screen shot")

---

###Techs
|Tech|
|----|
|C#|
|Sockets|
|Threading|
|Diagnostics|

---

#### To Run
Launch the server assembly using ctrl-f5, and then launch the client assembly in the same fashion.

The client assembly will create multiple client processes, each of which will connect to the server.

Clients will send data to the server on a regular interval.

Disconnect server and watch clients try to re-connect.

Reconnect server and clients will resume sending data.

Disconnect a client and watch the server catch this.

Server keeps track of all connected clients.
