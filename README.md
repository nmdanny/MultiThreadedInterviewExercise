Service Exercise

The goal of this exercise is to implement a service that handles requests using a pool of connections.
You will need to create a class that implements the interface IService.  This class will receive a stream of requests and will execute each request on a server using a connection.
A connection can only handle a single command at a time, and so in order to improve the computation time of the requests, this class will need to create a pool of connections. The maximum number of connections is in a constant named CONNETION_COUNT.
Each command returns an integer value, once all requests have been processed, the summary of all these integers must be returned.

The interface IService contains the following methods:
1.	sendRequest – send a request to be processed (should be non blocking).
2.	notifyFinishLoading – used to signal to the service that no more requests will be sent.
3.	getSummary – returns the summary of all integers from all commands.

The following classes are given to you, and do not require any change:
1.	The Connection class which simulates a connection to a remote server and allows a command to be run on the remote server. You are also given the rest of the logic around the service.  
2.	RequestsSender – simulates sending requests from a remote client.
3.	Request – This class represents each request from the Client. The value inside the Command should be sent to the remote server.

After you have created the service class, create it in the main function in the marked location.
Good Luck!
