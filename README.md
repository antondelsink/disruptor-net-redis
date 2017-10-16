# What

Redis server written in C# for [.NET Core 2](https://github.com/dotnet/core),
using the [.NET port](https://github.com/disruptor-net/Disruptor-net)
of the [LMAX Disruptor](https://github.com/LMAX-Exchange/disruptor).

# Why

Intended as a starting point for exploring performance of .NET Core related to
Sockets, TCP stream parsing, Collections, etc.

# Thanks

Special thanks to the [Redis](http://www.redis.io) community and in particular
Salvatore Sanfilippo aka [antirez](https://github.com/antirez) 
as this would not have been a viable training approach without the 
redis-benchmark utility and the easy-to-parse RESP (Redis Serialization Protocol).

# Usage

This is not a library. There is no NuGet package.

Fork/Clone and run any of the "unit tests" in the namespace DisruptorNetRedis.LongRunningTests.
See comments on each test method for appropriate command-line options for redis-benchmark.

# Learn

Fork and edit!
* Beginner: start with adding new Redis Commands. Use the redis-benchmark tool to baseline and test your new commands. Use a profiler and aim for zero garbage.
* Intermediate: establish a performance baseline with 'redis-benchmark' for a selection of commands, then replace and improve on an existing database class.
* Advanced: replace the networking code with Rx, TPL, Non-Blocking Sockets, etc.

Pull requests welcome! Just bear in mind this is for teaching, so there are limits to what I can include with regard complexity.

## Implemented Commands

**SET**, **GET**

**LPUSH**, **RPUSH**, **LRANGE**

**SADD**, **SCARD**, **SUNION**

## Coming Soon - contributions welcome!

SETNX

MSET, MGET,

MULTI..{EXEC|DISCARD}

EXPIRE, SETEX

...and lots more. Pull requests welcome!

## Almost certainly not happening

Blocking commands such as BLPOP/BRPOP.

PubSub.