# kafka-basic
## Overview
A code example for the [Kafka](https://kafka.apache.org/) project.

## How to run
- Run `docker-compose up` to create the Kafka cluster.
- Go to `src/kafka-test` and run `dotnet build` to build the project.
- Navigate to `src/kafka-test/bin/Debug/net6.0`.
  - Run `kafka-test -p` to create a producer.
  - Run `kafka-test -c` to create a consumer.