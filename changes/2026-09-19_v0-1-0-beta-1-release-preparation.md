# Подготовка v0.1.0-beta.1

Цель: подготовить single-file Windows x64 beta-артефакт и метаданные для GitHub prerelease.

Базовый commit: `6434d0f`.

Затронутые компоненты: единый номер версии WPF App, профиль публикации, changelog и README.

Результат: версия приложения — `0.1.0-beta.1`; профиль `win-x64-single-file` создаёт self-contained single-file `.exe` с включённым извлечением native libraries. Microsoft Edge WebView2 Evergreen Runtime остаётся prerequisite и не включается в артефакт. README и changelog описывают beta-ограничения.

Проверки: restore, Release build, test, publish, состав артефакта и SHA-256 должны быть выполнены до tag/release.

Миграция/совместимость: SQLite и профиль WebView2 не изменяются.

Откат: `git revert <commit>` для commit подготовки, затем повторить проверки. Опубликованный tag/release не перезаписывать.
