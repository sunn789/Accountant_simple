# مرحله ۱: بیلد کردن پروژه
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Api.csproj", "./"]
RUN dotnet restore "Api.csproj"
COPY . .
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# مرحله ۲: اجرای برنامه
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# تنظیم پورت پیش‌فرض دات‌نت در داکر
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]