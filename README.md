# NexsusVKVideo

`v0.1.0-beta.1` — Windows-оболочка официального сайта VK Video. После запуска приложение открывает `vkvideo.ru` во встроенном WebView2; каталог, поиск, вход и воспроизведение предоставляет сам сайт VK.

Для запуска требуется Windows x64, подключение к интернету и установленный [Microsoft Edge WebView2 Evergreen Runtime](https://developer.microsoft.com/microsoft-edge/webview2/). Beta не подписана и не содержит installer.

## Быстрый старт
1. Распакуйте содержимое ZIP прямо в корень пустого репозитория: рядом должны оказаться AGENTS.md, src/, docs/ и .github/.
2. Откройте эту папку в Codex. Для разработки установите Git и .NET SDK 10.0.401 с Windows desktop tooling; версия закреплена в global.json.
3. Выполните проверки: `dotnet restore NexsusVKVideo.sln`, затем build и test из раздела ниже. Для single-file публикации: `dotnet publish src/NexsusVKVideo.App/NexsusVKVideo.App.csproj -c Release -p:PublishProfile=win-x64-single-file`.
4. Для GitHub следуйте [GITHUB_SETUP.md](GITHUB_SETUP.md).
5. Сборка будущего каркаса: dotnet restore NexsusVKVideo.sln; dotnet build NexsusVKVideo.sln -c Release --no-restore; dotnet test NexsusVKVideo.sln -c Release --no-build.
Команды выполняются по очереди. CI требует solution, global.json, исходный и тестовый проекты.

## Материалы
- [Development prompt](NexsusVKVideo_Development_Prompt.md) — продуктовые требования.
- [Архитектура](docs/ARCHITECTURE.md), [модули](docs/MODULES.md), [roadmap](docs/ROADMAP.md).
- [Дизайн](docs/DESIGN_IMPLEMENTATION_GUIDE.md) и assets/mockups/MainWindow_Mockup.png.
- [Рабочий процесс](CODEX_WORKFLOW.md), [безопасность](SECURITY.md), [релизы](RELEASE_PROCESS.md).
- [Состав и ограничения](docs/PACKAGE_STATUS.md).

Логотип, предоставленный пользователем, включён без изменения в assets/branding; происхождение указано в README этой папки. Лицензия проекта владельцем ещё не выбрана.
