# NexsusVKVideo
Стартовый пакет разработки лёгкого модульного Windows-приложения для просмотра VK Video.
Это техническое задание и начальный собираемый каркас, а не готовый видеоклиент. Этап 0 завершён; этап 0.1 добавляет WPF-навигацию, демонстрационный Home и проверку официального VK Video widget в WebView2.

## Быстрый старт
1. Распакуйте содержимое ZIP прямо в корень пустого репозитория: рядом должны оказаться AGENTS.md, src/, docs/ и .github/.
2. Откройте эту папку в Codex. Для разработки установите Git и .NET SDK 10.0.401 с Windows desktop tooling; версия закреплена в global.json.
3. Выполните проверки: `dotnet restore NexsusVKVideo.sln`, затем build и test из раздела ниже.
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
