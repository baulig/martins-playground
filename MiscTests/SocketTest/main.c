#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>

int main (int argc, char *argv[])
{
	int sock = socket (PF_INET6, SOCK_DGRAM, 0);
	printf ("SOCKET: %d\n", sock);

	struct sockaddr_in6 saddr;
	memset (&saddr, 0, sizeof (saddr));
	saddr.sin6_family = PF_INET6;
	saddr.sin6_port = htons (8888);
	saddr.sin6_addr = in6addr_any;
	
	struct addrinfo* addr;
	if (getaddrinfo ("ff02::1", NULL, NULL, &addr)) {
		fprintf (stderr, "getaddrinfo(): %s\n", strerror (errno));
		return -1;
	}
	
	if (bind (sock, (const struct sockaddr*)&saddr, sizeof (saddr))) {
		fprintf (stderr, "bind(): %s\n", strerror (errno));
		return -1;
	}
	
	int iface = if_nametoindex ("lo0");
	printf ("IFACE: %d\n", iface);
	
	struct ipv6_mreq mreq;
	memset (&mreq, 0, sizeof (mreq));
	memcpy (&mreq.ipv6mr_multiaddr, &((struct sockaddr_in6 *) addr->ai_addr)->sin6_addr, sizeof(mreq.ipv6mr_multiaddr));
	mreq.ipv6mr_interface = 0;
	
	if (setsockopt (sock, IPPROTO_IPV6, IPV6_JOIN_GROUP, &mreq, sizeof(mreq))) {
		fprintf (stderr, "setsockopt(): %s\n", strerror (errno));
		return -1;
	}
	
	printf ("OK\n");
	
	return 0;
}

