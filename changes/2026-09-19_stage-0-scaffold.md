# Этап 0 — собираемый каркас

Дата: 2026-09-19

Цель: создать воспроизводимый WPF/MVVM-каркас NexsusVKVideo согласно этапу 0 ROADMAP.

База Git: initial import; в полученном пакете отсутствует каталог `.git`.

Компоненты: root solution и SDK-конфигурация, App, Core, Infrastructure, Modules, Core tests, CI, документация решения.

Изменение: добавлены `NexsusVKVideo.sln`, четыре проекта и два тестовых проекта. App является composition root с DI и минимальным стартовым окном; Core содержит модели, контракты и дедупликацию избранного. CI требует solution и global.json, затем выполняет restore/build/test.

Проверки (команда/сценарий, фактический результат): подпись установщика .NET 10.0.401 — `Valid`, подписант Microsoft; `./scripts/Validate-Package.ps1` — успешно; XML проектов и JSON SDK — успешно разобраны; `dotnet sln NexsusVKVideo.sln list` — успешно, 6 проектов; `dotnet restore NexsusVKVideo.sln` — успешно; `dotnet build NexsusVKVideo.sln -c Release --no-restore` — успешно, 0 warnings/0 errors; `dotnet test NexsusVKVideo.sln -c Release --no-build` — успешно, 7/7 Core-тестов. В Infrastructure.Tests нет тестовых случаев: проект оставлен каркасом для следующего этапа. Smoke: Release WPF-приложение запущено, показало окно «NexsusVKVideo» и отвечало; тестовый экземпляр затем завершён.

Данные и совместимость: пользовательские данные и миграции не добавлялись. Требуется .NET SDK 10.0.401.

Откат (commit/порядок, резервная копия, проверка): после первого коммита выполнить `git revert <commit>` и повторить restore/build/test. Данных для резервного копирования нет.

Ограничения: навигация, Home demo, провайдер VK, WebView2 и хранилища являются задачами следующих этапов. Визуальная автоматизация этой среды не видит нативные окна, поэтому проверены запуск, заголовок и отзывчивость процесса, но не сделан скриншотный UI-review. Неполные временные файлы альтернативной загрузки SDK удалены.
