# Этап 0.2 — локальное SQLite-хранилище

Дата: 2026-09-19

Цель: добавить проверяемое локальное хранение истории, избранного и настроек без VK API, сетевых запросов или учётных данных.

База Git: `ed41eb4 feat: add VK Video widget playback shell`.

Компоненты: Core contracts, Infrastructure/Storage, composition root App, Infrastructure tests, пакет `Microsoft.Data.Sqlite` 10.0.12.

Изменение: `SqliteLocalDataStore` реализует `IHistoryRepository`, `IFavoritesRepository` и `ISettingsStore`; создаёт миграцию схемы 1 в `%LocalAppData%\NexsusVKVideo\data\nexsusvkvideo.db` при первой операции. Избранное уникально по provider+videoId; история упорядочена по времени просмотра. Миграция отказывается открывать базу более новой версии, не выполняя downgrade. Реализации зарегистрированы в App DI; UI-представление данных будет подключено следующим изменением этапа 0.2.

Проверки (команда/сценарий, фактический результат): `dotnet restore NexsusVKVideo.sln -m:1 --disable-parallel` — успешно; `dotnet build NexsusVKVideo.sln -c Release --no-restore -m:1` — успешно, 0 warnings/0 errors; `dotnet test NexsusVKVideo.sln -c Release --no-build -m:1` — успешно, 19/19. Тесты покрывают миграцию, запрет downgrade, отмену до открытия базы, персистентность после повторного создания store, дедупликацию/удаление избранного, порядок и очистку истории, настройки.

Данные и совместимость: первая миграция создаёт SQLite-файл и каталог только в профиле пользователя, не в рабочем дереве. Нет миграции со старой схемы, так как до этого изменения база продукта не создавалась. Tokens, cookies и сетевые данные не сохраняются.

Откат (commit/порядок, резервная копия, проверка): перед `git revert <commit>` скопировать `%LocalAppData%\NexsusVKVideo\data`, затем выполнить revert и restore/build/test. Revert кода не меняет и не удаляет пользовательскую базу.

Ограничения: экран Библиотека/Избранное и команды добавления в UI ещё не подключены к этому хранилищу; они остаются следующим вертикальным срезом этапа 0.2. Поиск, авторизация и VK API не добавлены.
