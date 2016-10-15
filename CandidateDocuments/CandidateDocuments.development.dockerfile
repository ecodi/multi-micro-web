FROM microsoft/dotnet:latest
COPY . /app

WORKDIR /app/CandidateDocuments.Application
RUN ["dotnet", "restore"]

WORKDIR /app/CandidateDocuments.Infrastructure
RUN ["dotnet", "restore"]

WORKDIR /app/CandidateDocuments.API
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_MEMORYDB true

ENTRYPOINT ["dotnet", "run"]

# Build the image:
# docker build -f CandidateDocuments.development.dockerfile -t dotnet/candidatedocuments:dev .

# Run
# docker run -it -p 5000:5000 dotnet/candidatedocuments:dev