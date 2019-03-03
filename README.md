SharpIRC
--------

SharpIRC is a IRC Client Library which works transparently through App Domains.

[![Build Status](https://travis-ci.org/FreeApophis/SharpIRC.svg?branch=master)](https://travis-ci.org/FreeApophis/SharpIRC)
[![NuGet package](https://buildstats.info/nuget/SharpIRC)](https://www.nuget.org/packages/SharpIRC)


* SharpIRC is a fork of SmartIRC4net
* SharpIRC is a C# class for communication with IRC networks, which conforms to the RFC 2812 (IRC Protocol).
* SmartIRC4Net was a port of SmartIRC (written in PHP),
* SharpIRC is an API that handles all IRC protocol messages and is designed for creating IRC bots or even GUI clients.
* SharpIRC is the base for the Huffelpuff IRC Bot

Why Fork?
---------

* SmartIRC4net was written for C# 1 and updated to C# 2.0 but it has never had a full rehaul.
* SharpIRC is not ABI compatible anymore because of the extensive usage of Generics, and removal of useless calls
* SharpIRC is developed to work with a Pluginsystem which uses AppDomains
* The SharpIRC ABI will change without notice, I regard features as more important than backward compatibility

Howto Use?
----------

Look into the example folder. And use your IDE to inspect the API.

Project Homepage:
https://github.com/FreeApophis/sharpIRC

Please report bugs to:
https://github.com/FreeApophis/sharpIRC/issues

