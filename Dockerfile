FROM mcr.microsoft.com/dotnet/sdk:5.0-focal

RUN set -xe \
    && apt-get update \
    && DEBIAN_FRONTEND="noninteractive" apt-get -y install build-essential devscripts debhelper intltool fakeroot \
    && apt-get --purge  autoremove -y \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

RUN set -xe \
    && dotnet tool install --global Cake.Tool --version 0.38.5

ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /data
