# Старт
Пакет содержит спецификации; приложение ещё не реализовано.
1. Учитывай AGENTS.md; проверь git status, доступный SDK и права GitHub.
2. Прочитай development prompt, docs/ARCHITECTURE.md и этап 0 в docs/ROADMAP.md.
3. Создай NexsusVKVideo.sln и проекты по docs/PROJECT_STRUCTURE.md. Выбери поддерживаемую стабильную .NET LTS на дату реализации, зафиксируй конкретный SDK в global.json и согласуй CI.
4. Создай стартовое WPF-окно, навигацию и демонстрационный Home. Подключай WebView2 на следующем этапе после проверки официального способа воспроизведения.
5. При UI-работе открой PNG и docs/DESIGN_IMPLEMENTATION_GUIDE.md.
6. Добавь содержательные тесты Core и проверь restore/build/test. Обнови workflow: отсутствие solution после этапа 0 должно быть ошибкой.
7. Подготовь change record, коммит и PR согласно запросу пользователя.
Не читай весь репозиторий заранее. Дальнейший процесс — CODEX_WORKFLOW.md.
