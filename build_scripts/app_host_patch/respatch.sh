#!/bin/sh

exe=$1
dll=$2

if [ ! -f $exe ]; then
    echo "$exe not found!"
    exit 1
fi
if [ ! -f $dll ]; then
    echo "$dll not found!"
    exit 1
fi
if [ ! -f Dockerfile ]; then
    echo "Dockerfile not found!"
    exit 1
fi

podman build -t app-host-patcher -f Containerfile
cid=$(podman run -dit app-host-patcher)
podman cp $exe $cid:w.exe
podman cp $dll $cid:w.dll
podman exec -it $cid wine64 AppHostPatcher.exe w.exe w.dll
podman cp $cid:w.exe $exe
podman kill $cid
podman rm $cid
podman image rm app-host-patcher
