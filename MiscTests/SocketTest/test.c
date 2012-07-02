#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/select.h>
#include <arpa/inet.h>
#include <net/if.h>
#include <netdb.h>
#include <assert.h>
#include <signal.h>

int
main (void)
{
	signal (SIGPIPE, SIG_IGN);

	int server = socket (AF_INET, SOCK_STREAM, IPPROTO_TCP);
	assert (server);
	
#if 0
	struct linger linger;
	linger.l_onoff = 0;
	linger.l_linger = 0;
	
	int ret = setsockopt (server, SOL_SOCKET, SO_LINGER, &linger, sizeof (linger));
	printf ("SETSOCKOPT: %d\n", ret);
#endif
	
	struct sockaddr_in addr;
	memset (&addr, 0, sizeof (addr));
	addr.sin_family = AF_INET;
	addr.sin_addr.s_addr = htonl (INADDR_LOOPBACK);
	addr.sin_port = htons (8888);
	
	int ret = bind (server, (struct sockaddr*)&addr, sizeof (addr));
	printf ("BIND: %d - %s\n", ret, strerror (errno));
	assert (!ret);
	listen (server, 1);
	
	int client = socket (AF_INET, SOCK_STREAM, IPPROTO_TCP);
	ret = connect (client, (struct sockaddr*)&addr, sizeof (addr));
	assert (!ret);
	
#if 0
	ret = setsockopt (client, SOL_SOCKET, SO_LINGER, &linger, sizeof (linger));
	printf ("SETSOCKOPT: %d\n", ret);
#endif

	int connected = accept (server, NULL, NULL);
	assert (connected);
	
	char buffer [100];
	memset (buffer, 0, sizeof (buffer));
	
	ret = send (connected, buffer, sizeof (buffer), 0);
	printf ("SEND: %d - %s\n", ret, strerror (errno));
	
	sleep (1);
	
	// ret = shutdown (client, SHUT_RDWR);
	// printf ("SHUTDOWN: %d\n", ret);
	
	ret = close (client);
	printf ("CLOSE: %d\n", ret);
	
	// sleep (1);

#if 0	
	fd_set read;
	FD_ZERO (&read);
	FD_SET (connected, &read);
	fd_set write;
	FD_ZERO (&write);
	FD_SET (connected, &write);
	fd_set error;
	FD_ZERO (&error);
	FD_SET (connected, &error);

	ret = select (connected+1, &read, &write, &error, NULL);
	printf ("SELECT: %d - %d,%d,%d\n", ret, FD_ISSET (connected, &read),
		FD_ISSET (connected, &write), FD_ISSET (connected, &error));
#endif
	
	// ret = send (connected, buffer, sizeof (buffer), 0);
	// printf ("SEND #1: %d - %s\n", ret, strerror (errno));
	
	// sleep (1);
	
	errno = 0;
	ret = recv (connected, buffer, 10, 0);
	printf ("READ: %d - %s\n", ret, strerror (errno));
	
	// sleep (1);
	
	ret = recv (connected, buffer, 10, 0);
	printf ("READ: %d - %s\n", ret, strerror (errno));

	close (server);
}

