define PL1 = 1.0.0.0/24
define PL2 = 1.0.1.0/24
define PG1 = 2.0.0.0/24
define PG2 = 2.0.1.0/24

define Peer = {Peer1, Peer2}

define transit(X,Y) = enter(X) & exit(Y)

define routing = {
	PG1 => end(A),
	PG2 => end(B),
	PL1 => end(C),
	PL2 => end(D),
	true => exit(Peer1 >> Peer2)
}

define locality = {
	PL1 or PL2 => internal
}

define notransit = {
	true => not transit(Peer, Peer)
}

define main = routing & locality & notransit