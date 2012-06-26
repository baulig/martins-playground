#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>

int main (int argc, char *argv[])
{
	int sock = socket (PF_INET6, SOCK_STREAM, IPPROTO_TCP);
	printf ("SOCKET: %d\n", sock);
	
	struct sockaddr_in6 saddr;
	memset (&saddr, 0, sizeof (saddr));
	saddr.sin6_family = PF_INET6;
	saddr.sin6_port = htons (8888);
	
	int ret = bind (sock, (struct sockaddr*)&saddr, sizeof (saddr));
	printf ("BIND: %d\n", ret);
	
	ret = listen (sock, 1);
	printf ("LISTEN: %d\n", ret);
	
	struct sockaddr_in6 addr;
	socklen_t addr_len = sizeof (addr);
	ret = getsockname (sock, (struct sockaddr*)&addr, &addr_len);
	printf ("SOCK NAME: %d\n", ret);
	
	int i;
	for (i = 0; i < 16; i++) {
		if (i && ((i % 2) == 0))
			printf (":");
		printf ("%x", addr.sin6_addr.s6_addr [i]);
	}
	printf ("\n");
	
	int accepted = accept (sock, (struct sockaddr*)&addr, &addr_len);
	printf ("ACCEPT: %d\n", accepted);
	
	return 0;
}

