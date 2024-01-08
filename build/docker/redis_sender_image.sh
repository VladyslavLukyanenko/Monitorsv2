#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)
docker build -t docker.pkg.github.com/bandar-monitors/monitors/redis_sender:$IMG_TAG -t docker.pkg.github.com/bandar-monitors/monitors/redis_sender:latest -f ./src/ProjectMonitors.Senders.Redis/Dockerfile .
docker push docker.pkg.github.com/bandar-monitors/monitors/redis_sender:$IMG_TAG
docker push docker.pkg.github.com/bandar-monitors/monitors/redis_sender:latest