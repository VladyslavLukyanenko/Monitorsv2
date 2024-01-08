FROM golang:latest as buildenv

WORKDIR /build

COPY go.mod go.sum ./

# Download all the dependencies
RUN go mod download

COPY . .

WORKDIR /build

RUN GOOS=linux go build -a -installsuffix cgo -o solvers/pxgen .

FROM ubuntu
MAINTAINER Andrii Kondratiuk <andriikondratiukv@gmail.com>
WORKDIR /app
COPY --from=buildenv /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/
COPY --from=buildenv /build/solvers/pxgen .

ENTRYPOINT ["./pxgen"]
