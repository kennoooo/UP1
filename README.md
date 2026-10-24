# Бронирование билетов в музеи и на выставки

Учебная практика УП.01.01. WPF-приложение (.NET Framework) с базой данных Microsoft SQL Server
для платформы бронирования билетов в музеи и на выставки.

## Структура репозитория

- `README.md` - описание работы и структуры решения
- `db/schema.sql` - скрипт создания базы данных `MuseumTicketsDB` с тестовыми данными
- `res/` - ресурсы приложения (`logo.png` - логотип, `icon.ico` - иконка приложения)
- `WpfApp1/` - проект WPF

## База данных

Три таблицы со связями «один ко многим» и каскадным удалением (`ON DELETE CASCADE`):

| Таблица | Поля |
|---|---|
| Users | ID, Login, Password (SHA1-хэш), Role (User / Admin), FIO, Photo |
| Museums | ID, Name, City |
| Bookings | ID, MuseumID, UserID, Date, Name (выставка), Num (билетов, 1-10), Price (цена билета) |

Связи: `Bookings.UserID -> Users.ID`, `Bookings.MuseumID -> Museums.ID`.

## Структура проекта

| Файл | Назначение |
|---|---|
| `Core.cs` | контекст базы данных `Core.DB` и текущий пользователь `Core.CurrentUser` |
| `Helper.cs` | хэширование пароля, аутентификация, генерация и проверка капчи, проверки форматов, сообщения, смена темы |
| `Partial.cs` | вычисляемое поле `Total` (цена * количество) для сущности `Bookings` |
| `Dictionary.xaml` | словарь стилей по Руководству по стилю (светлая тема) |
| `DarkDictionary.xaml` | собственный словарь стилей (тёмная тема) |
| `MainWindow` | главное окно: кнопка «Назад», заголовок страницы, смена темы, дата и время, фрейм `MainFrame` |
| `Pages/AuthPage` | авторизация, капча после 3 неверных попыток |
| `Pages/RegPage` | регистрация с выбором роли |
| `Pages/ChangePassPage` | смена пароля |
| `Pages/UserPage` | страница пользователя: свои бронирования, поиск, фильтр по музею, сортировка |
| `Pages/AdminPage` | страница администратора: переход к таблицам и диаграммам |
| `Pages/UsersTabPage`, `AddUserPage` | таблица пользователей и форма добавления / редактирования |
| `Pages/MuseumsTabPage`, `AddMuseumPage` | таблица музеев и форма |
| `Pages/BookingsTabPage`, `AddBookingPage` | таблица бронирований и форма |
| `Pages/DiagrammPage` | диаграмма сумм по музеям для выбранного пользователя, экспорт в .docx, .pdf, .xlsx |

## Запуск

1. Выполнить `db/schema.sql` в SQL Server Management Studio.
2. Открыть проект в Visual Studio, проверить строку подключения в `App.config`.
3. Для экспорта отчётов на компьютере должны быть установлены Microsoft Word и Excel.
4. Запустить приложение.

## Тестовые учётные записи

| Логин | Пароль | Роль |
|---|---|---|
| admin | Admin123 | Admin |
| ivanova | User123 | User |
| smirnov | User234 | User |
