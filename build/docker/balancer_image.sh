#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)
docker build -t docker.pkg.github.com/bandar-monitors/monitors/balancer:$IMG_TAG -t docker.pkg.github.com/bandar-monitors/monitors/balancer:latest -f ./src/ProjectMonitors.Balancer/Dockerfile .
docker push docker.pkg.github.com/bandar-monitors/monitors/balancer:$IMG_TAG
docker push docker.pkg.github.com/bandar-monitors/monitors/balancer:latest