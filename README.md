# CSharp_AsyncTcpSockets

---

A server / multi client app on async sockets. Threading and multi process creation for clients included.

---

Developed with Visual Studio 2015 Community

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
launch the server assembly using ctrl-f5, and then launch the client assembly in the same fashion.

The client assembly wil create multiple client processes that will connect to the server.

Clients will send data to the server on a regular interval.

Disconnect server and watch clients try to re-connect.

Reconnect server and clients will resume sending data.

Disconnect a server and watch server the server catch this.
