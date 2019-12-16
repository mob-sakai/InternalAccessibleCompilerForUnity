#!/bin/sh

VERSION=$1
NAME=NoAccessibilityCompiler
FRAMEWORK=netcoreapp3.1

for rid in win-x64 osx-x64 linux-x64
do
    FILENAME=${NAME}
    BINAME=${NAME}-${VERSION}-${rid}
    [ "${rid}" = "win-x64" ] && FILENAME=${FILENAME}.exe && BINAME=${BINAME}.exe

    dotnet publish -r ${rid} -c Release -p:PublishTrimmed=true -p:ReadyToRun=true -p:PublishSingleFile=true -p:PackageVersion=${VERSION}
    mv bin/Release/${FRAMEWORK}/${rid}/publish/${FILENAME} bin/${BINAME}
done

dotnet pack -c Release -p:PackageVersion=${VERSION}
