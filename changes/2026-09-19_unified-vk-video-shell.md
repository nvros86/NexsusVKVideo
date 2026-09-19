# Единая оболочка официального VK Video

Цель: убрать дублирование навигации NexsusVKVideo и официального сайта VK Video.

Базовый commit: `19386ec`.

Затронутые компоненты: начальный маршрут view model, главная WPF-разметка и документация браузерного режима.

Результат: приложение запускается сразу на официальном сайте VK Video. В браузерном режиме WebView2 занимает всё окно, а sidebar, toolbar и player panel NexsusVKVideo скрыты. Пользователь работает только с меню, поиском и каталогом самого VK Video, не вставляя ссылки и не переключаясь в отдельный браузер.

Проверки: изолированная `dotnet build NexsusVKVideo.sln -c Release --no-restore -m:1 -p:BaseOutputPath=artifacts/unified-shell-validation` — успешно, 0 warnings/0 errors; изолированная `dotnet test NexsusVKVideo.sln -c Release --no-build --no-restore -m:1 -p:BaseOutputPath=artifacts/unified-shell-validation` — успешно, 32/32. Обычная папка `bin/Release` была занята запущенным приложением и не изменялась. Ручная проверка: запуск, отсутствие нативных панелей поверх сайта, поиск и вход VK.

Миграция/совместимость: данных и настроек не изменяет; сессия VK по-прежнему находится в локальном профиле WebView2.

Откат: `git revert <commit>` для commit этого изменения, затем повторить build и test.
