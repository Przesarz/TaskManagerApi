Task Manager
Opis projektu

Task Manager to aplikacja Web API stworzona w technologii ASP.NET Core.
Projekt służy do zarządzania zadaniami i pozwala na ich tworzenie, edycję, usuwanie oraz pobieranie.

API obsługuje również filtrowanie, wyszukiwanie, sortowanie i paginację wyników.

Projekt został stworzony jako praktyczny projekt edukacyjny do nauki tworzenia aplikacji backendowych w .NET.

Technologie
C#
.NET 8
ASP.NET Core Web API
Entity Framework Core
SQLite
Swagger / OpenAPI
xUnit
Moq
Microsoft.AspNetCore.Mvc.Testing
Uruchomienie
Wymagania
.NET 8 SDK
Git
Klonowanie repozytorium
git clone https://github.com/Przesarz/TaskManager.git
cd TaskManager
Przywrócenie zależności
dotnet restore
Utworzenie bazy danych
dotnet ef database update --project TaskManager.Api

Jeżeli nie masz zainstalowanego narzędzia Entity Framework Core CLI:

dotnet tool install --global dotnet-ef
Uruchomienie aplikacji
dotnet run --project TaskManager.Api

Po uruchomieniu API można otworzyć Swaggera pod adresem wyświetlonym w konsoli.

API
Metoda	Endpoint	Opis
GET	/api/tasks	Pobiera listę zadań
GET	/api/tasks/{id}	Pobiera zadanie po ID
POST	/api/tasks	Tworzy nowe zadanie
PUT	/api/tasks/{id}	Aktualizuje zadanie
DELETE	/api/tasks/{id}	Usuwa zadanie
Filtrowanie, wyszukiwanie i sortowanie

Endpoint GET /api/tasks obsługuje m.in.:

filtrowanie po statusie wykonania,
filtrowanie po priorytecie,
wyszukiwanie po tytule,
sortowanie po tytule,
sortowanie po terminie wykonania,
sortowanie rosnące i malejące,
paginację.

Przykład:

GET /api/tasks?IsCompleted=false&Priority=3&SortBy=Title&SortDirection=desc&Page=1&PageSize=10
Testy

Projekt zawiera kilka rodzajów testów:

testy serwisu,
testy kontrolera,
testy walidacji DTO,
testy integracyjne całego API.

Do testów integracyjnych wykorzystywana jest baza SQLite działająca w pamięci.

Uruchomienie wszystkich testów:

dotnet test
Architektura

Projekt wykorzystuje prosty podział odpowiedzialności:

Controller
    ↓
ITaskService
    ↓
TaskService
    ↓
TaskManagerDbContext
    ↓
SQLite

Kontroler odpowiada za obsługę żądań HTTP, serwis za logikę aplikacji, a Entity Framework Core za komunikację z bazą danych.

Repozytorium

GitHub:

https://github.com/Przesarz/TaskManager