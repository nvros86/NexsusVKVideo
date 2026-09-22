# Надёжная публикация beta-релиза через GitHub Actions

## Причина

Локальная загрузка крупных EXE в GitHub Releases может быть нестабильна из-за сетевой среды. Публикация не должна превращать черновик в релиз, пока assets не доступны.

## Изменение

- Добавлен ручной workflow `publish beta release`.
- Workflow получает уже существующий tag, выполняет restore, build и test на Windows runner, собирает single-file EXE и Inno Setup installer, затем загружает оба файла в GitHub Release и публикует его как prerelease.

## Безопасность

- Используется краткоживущий `GITHUB_TOKEN` GitHub Actions с минимальным требуемым правом `contents: write`.
- Workflow не содержит токенов, сертификатов или секретов проекта.
- Отдельная цифровая подпись EXE не добавляется: она по-прежнему требует сертификат владельца.
