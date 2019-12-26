# What is it?

Experimental Redis server written in C# for [.NET Core](https://github.com/dotnet/core),
using the [.NET port](https://github.com/disruptor-net/Disruptor-net)
of the [LMAX Disruptor](https://github.com/LMAX-Exchange/disruptor).

I'm using this for learning about performance; specifically Sockets, TCP stream parsing,
Garbage Collection, built-in Collections, and of course the Disruptor.

# Usage

This is not a library. There is no NuGet package.

When running the server, use the redis-cli or redis-benchmark client tools to connect.

Fork/Clone and run any of the "unit tests" in the namespace DisruptorNetRedis.LongRunningTests.
See comments on each test method for appropriate command-line options for redis-benchmark.

Pull requests welcome! Just bear in mind this is for learning & teaching,
so there are limits to what I can include with regard complexity.

## Implemented Commands

**SET**, **GET**

**LPUSH**, **RPUSH**, **LRANGE**, **LINDEX**

**SADD**, **SCARD**, **SUNION**

## Coming Soon

SETNX

MSET, MGET, ...

MULTI..{EXEC|DISCARD}

EXPIRE, SETEX, ...

...and lots more. Pull requests welcome!

## Almost certainly not happening

Blocking commands such as BLPOP/BRPOP.

PubSub.

# Thanks

Special thanks to the [Redis](http://www.redis.io) community and in particular
Salvatore Sanfilippo aka [antirez](https://github.com/antirez) 
as this would not have been a viable approach without the 
redis-benchmark utility and the easy-to-parse RESP (Redis Serialization Protocol).
