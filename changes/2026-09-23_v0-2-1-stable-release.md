# Stable release 0.2.1

- **Цель:** перевести проверенную `v0.2.1-beta.1` в обычный публичный release без изменения функций приложения.
- **Базовый commit:** `331936e`.
- **Затронутые компоненты:** версия приложения, README, changelog, roadmap, package status и workflow публикации.
- **Результат:** `v0.2.1` публикуется как обычный GitHub Release с installer и standalone EXE; workflow поддерживает и обычные, и prerelease-публикации.
- **Проверки:** локальная Release build — успешно, 0 warnings/0 errors; `dotnet test` — 32/32 пройдены; `scripts/Validate-Package.ps1` — 20 required entries; `git diff --check` и проверка секретов — чисто; GitHub Actions [#35860010204](https://github.com/nvros86/NexsusVKVideo/actions/runs/35860010204) — успешно.
- **Публикация:** обычный GitHub Release [v0.2.1](https://github.com/nvros86/NexsusVKVideo/releases/tag/v0.2.1) опубликован из тега; он не является draft или prerelease и содержит installer и standalone EXE, собранные GitHub Actions.
- **Миграция/совместимость:** отсутствует; совместимо с профилем WebView2 из `v0.2.1-beta.1`.
- **Откат:** не перезаписывать опубликованный tag; при дефекте выпустить новую исправляющую версию либо пометить release как draft/удалить assets с явным сообщением пользователям.
