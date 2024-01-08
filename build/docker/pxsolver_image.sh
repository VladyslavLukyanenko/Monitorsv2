#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)
cd ./src/golang/PxGen/src
docker build -t docker.pkg.github.com/bandar-monitors/monitors/pxsolver:$IMG_TAG -t docker.pkg.github.com/bandar-monitors/monitors/pxsolver:latest  .
docker push docker.pkg.github.com/bandar-monitors/monitors/pxsolver:$IMG_TAG
docker push docker.pkg.github.com/bandar-monitors/monitors/pxsolver:latest