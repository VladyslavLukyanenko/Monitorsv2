#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)
docker build -t docker.pkg.github.com/bandar-monitors/monitors/monitor:$IMG_TAG -t docker.pkg.github.com/bandar-monitors/monitors/monitor:latest -f ./src/ProjectMonitors.Monitor/Dockerfile --build-arg NUGETUSER=$1 --build-arg NUGETPASS=$2 .
docker push docker.pkg.github.com/bandar-monitors/monitors/monitor:$IMG_TAG
docker push docker.pkg.github.com/bandar-monitors/monitors/monitor:latest
