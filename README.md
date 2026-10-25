# StudentTeacher Practical

This is the starting project used in our PROG6212 practicals. Build on it until it has the features shown in StudentTeacher Final.

The project includes an ASP.NET Core API, a SQL Server database setup, unit tests and a basic MVC website. The MVC website is **not finished**. You'll add pages, connect MVC to the API and test.

## Getting started

1. Open `StudentTeacher_practical.slnx` in Visual Studio.
2. Check the SQL Server connection in `part2/Authentication_API/appsettings.json`. Change the server name if yours is different.
3. Apply the existing EF Core migrations. From the main project folder, run:

```powershell
dotnet ef database update --project .\part2\Authentication_API\Authentication_API.csproj --startup-project .\part2\Authentication_API\Authentication_API.csproj
```

4. Run `Authentication_API` in Visual Studio. Swagger is available at `https://localhost:7261/swagger` when using the supplied HTTPS launch profile.
5. Run `StudentTeacherPractical.Mvc` to view the basic MVC website.

## Docker practical

Start Docker Desktop and use Linux containers. Open a terminal in the main project folder:

```powershell
cd .\part3\StudentTeacherPractical.Mvc
docker build -t studentteacherpracticalmvc:latest .
docker run -d --name studentteacher-practical-mvc -p 8080:8080 studentteacherpracticalmvc:latest
```

Open `http://localhost:8080` to check that MVC runs in Docker. The starter MVC site does **not** connect to the API yet; we will add that later. If port 8080 is busy, use another host port such as `-p 8082:8080` and open `http://localhost:8082`.

To stop and remove this container:

```powershell
docker stop studentteacher-practical-mvc
docker rm studentteacher-practical-mvc
```

Rebuild the image after changing the MVC code, then create a new container to run the updated version.

## For your POE

This is a learning example, **not the RaceDay solution**. Use the techniques to build your own RaceDay MVC website. For Part 3, your finished MVC app must run in Docker, have a Dockerfile, include a working `docker run` command in its README and work the same way as it does locally. Follow the POE document for the other requirements.
