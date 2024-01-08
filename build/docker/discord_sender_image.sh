#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)
docker build -t docker.pkg.github.com/bandar-monitors/monitors/discord_sender:$IMG_TAG -t docker.pkg.github.com/bandar-monitors/monitors/discord_sender:latest -f ./src/ProjectMonitors.Senders.Discord/Dockerfile .
docker push docker.pkg.github.com/bandar-monitors/monitors/discord_sender:$IMG_TAG
docker push docker.pkg.github.com/bandar-monitors/monitors/discord_sender:latest