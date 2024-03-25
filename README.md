# PixHub_ConcilliationConsumer

A console application built in C# as a Consumer (Worker) of the RabbitMQ for PixHub_API.

## Description

This application is responsible for comparing **Payment Service Providers** (PSPs) payment data with the PixHub_API database and returning the differences between them.
As a Worker, it was built as part of the PixHub_API to be able to scale horizontally, avoiding bottlenecks in response time for the PSPs, as it must compare millions of records, which can take a long time to process.
It is also part of the processing part of the processing of the logic of the PIX mechanism within the **Central Bank** (BC).

<br />

## Quick start

Clone the repository:

`git clone https://github.com/422UR4H/PixHub_ConcilliationConsumer`


Enter the folder and run app:

```bash
cd PixHub_ConcilliationConsumer/
dotnet watch
```

## Usage

Just run the program above and this application is already working. You will need services from financial institutions to run the application. It can be a simple mock to test requests.
Furthermore, you will need the main application running as well. The entire environment is dockerized together with RabbitMQ, so just run it and send requests to: POST `/concilliation`.


## Technologies used

For this project, I used:

- C#;
- .NET;
- RabbitMQ;
