#echo off
set SRC=.
set DST=.
set ORG=Packet.proto

protoc -I=%SRC% --csharp_out=%DST% %SRC%\%ORG%