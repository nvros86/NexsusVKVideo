# Public profile beta release 0.2.1

- **Цель:** выпустить beta-тег, в котором присутствуют MIT License, публичный скриншот и документация открытого проекта.
- **Базовый commit:** `9c62ac3`.
- **Затронутые компоненты:** метаданные приложения, release workflow, README, changelog, roadmap и статус пакета.
- **Результат:** версия приложения и ссылки на артефакты обновлены до `0.2.1-beta.1`; функции WebView2/VK Video не изменены.
- **Проверки:** локальная Release build — успешно, 0 warnings/0 errors; `dotnet test` — 32/32 пройдены; `scripts/Validate-Package.ps1` — 20 required entries; `git diff --check` и проверка секретов — чисто; GitHub Actions publish workflow [#35750099036](https://github.com/nvros86/NexsusVKVideo/actions/runs/35750099036) — успешно.
- **Публикация:** GitHub prerelease [v0.2.1-beta.1](https://github.com/nvros86/NexsusVKVideo/releases/tag/v0.2.1-beta.1) содержит installer и standalone EXE, собранные GitHub Actions из тега.
- **Миграция/совместимость:** отсутствует; обновление совместимо с профилем WebView2 предыдущей beta.
- **Откат:** удалить этот непубликованный commit через `git revert <sha>`; опубликованный релиз не перезаписывать, а выпустить исправляющий beta-тег.
