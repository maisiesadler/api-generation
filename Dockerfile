# Used for running tests in github actions
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /source

COPY . .
RUN dotnet restore
RUN dotnet test

RUN echo $?

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

CMD ["echo", "hi"]
