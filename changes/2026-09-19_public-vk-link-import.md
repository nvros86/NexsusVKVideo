# Импорт публичной ссылки VK Video

Цель: дать пользователю открыть публичную ссылку VK Video во встроенном плеере без серверного backend, API-ключа или токена.

Базовый commit: `3c2c3d4`.

Затронутые компоненты: `NexsusVKVideo.Infrastructure.Vk`, WPF view models и разметка App, тесты Infrastructure, документация состояния пакета.

Результат: раздел «Поиск» принимает только HTTPS URL точной формы `https://vk.ru/video-{owner}_{video}`. Парсер отбрасывает другие хосты, схемы и пути, затем формирует официальный `video_ext.php?oid=…&id=…` для единственного WebView2. Ссылка добавляется в локальную историю и может быть добавлена в локальное избранное; метаданные и поиск не выполняются.

Проверки: `dotnet restore NexsusVKVideo.sln -m:1 --disable-parallel` — успешно; `dotnet build NexsusVKVideo.sln -c Release --no-restore -m:1` — успешно, 0 warnings/0 errors; `dotnet test NexsusVKVideo.sln -c Release --no-build -m:1` — успешно, 26/26. Unit-тесты не обращаются к VK.

Миграция/совместимость: миграции SQLite отсутствуют. Существующая история и избранное сохраняются; импорт создаёт новые записи с provider `vk`.

Откат: `git revert <commit>` для commit этого изменения; затем повторить restore, build и test. Локальные записи, созданные импортом, при необходимости очищаются через экран Библиотеки.
