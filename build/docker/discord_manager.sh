#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)
docker build -t docker.pkg.github.com/bandar-monitors/monitors/discord_manager:$IMG_TAG -t docker.pkg.github.com/bandar-monitors/monitors/discord_manager:latest -f ./src/ProjectMonitors.Managers.Discord/Dockerfile .
docker push docker.pkg.github.com/bandar-monitors/monitors/discord_manager:$IMG_TAG
docker push docker.pkg.github.com/bandar-monitors/monitors/discord_manager:latest