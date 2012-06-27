#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>

int main (int argc, char *argv[])
{
	int sock = socket (PF_INET, SOCK_DGRAM, IPPROTO_UDP);
	printf ("SOCKET: %d\n", sock);

	struct sockaddr_in saddr;
	memset (&saddr, 0, sizeof (saddr));
	saddr.sin_family = PF_INET;
	saddr.sin_port = htons (1);
	saddr.sin_addr.s_addr = inet_addr ("127.0.0.1");
	
	int ret = connect (sock, (struct sockaddr*)&saddr, sizeof (saddr));
	printf ("CONNECT: %d - %s\n", ret, strerror (errno));
	
	char buffer [1024];
	ret = recv (sock, buffer, 1024, 0);
	
	printf ("RECV: %d\n", ret);
	
	return 0;
}

